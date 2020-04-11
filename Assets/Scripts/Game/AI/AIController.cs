using System.Collections;
using Game.Player;
using Game.Weapons;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

namespace Game.AI {
    public class AIController : MonoBehaviourPun {
        [Header("Settings")]
        [SerializeField] private float maxLife;

        [SerializeField] private GameObject covidArea;

        [SerializeField] private GameObject contaminedParticle;

        [Header("Only for debug")]
        [SerializeField] private NavMeshAgent agent;

        [SerializeField] private float currentLife;

        [SerializeField] private Transform target;

        [SerializeField] private int skinId;

        [SerializeField] private Animator animator;
        [SerializeField] private AIState state;
        [SerializeField] private bool contaminated;
        [SerializeField] private AISound aiSound;
        [SerializeField] private CharacterController controller;

        private Coroutine covidCoroutine;
        private Coroutine idleCoroutine;
        private Coroutine moveToTargetCoroutine;

        private void Awake() {
            this.agent = GetComponent<NavMeshAgent>();
            this.animator = GetComponent<Animator>();
            this.aiSound = GetComponent<AISound>();
            this.controller = GetComponent<CharacterController>();
            this.currentLife = this.maxLife;
        }

        private void Start() {
            if (!PhotonNetwork.IsMasterClient) {
                return;
            }

            this.idleCoroutine = StartCoroutine(this.Idle());
        }

        private void OnDestroy() {
            StopAllCoroutines();
        }

        private void OnTriggerStay(Collider other) {
            if (!PhotonNetwork.IsMasterClient) {
                return;
            }

            if (other.CompareTag("CovidArea") && covidCoroutine == null && !this.IsContaminated()) {
                this.covidCoroutine = StartCoroutine(this.CovidDamageCoroutine());
            }
        }

        private IEnumerator CovidDamageCoroutine() {
            while (!this.IsContaminated()) {
                yield return new WaitForSeconds(0.1f);
                this.TakeDamageFromCovid();
            }
        }

        public void TakeDamageFromWeapon(Weapon weapon) {
            if ((weapon.GetWeaponType() == WeaponType.HAND_SANITIZER && this.contaminated) || weapon.GetWeaponType() == WeaponType.CHLOROQUINE) {
                photonView.RPC("RPC_TakeDamage", RpcTarget.MasterClient, weapon.GetDamage());
            }
        }

        public void TakeDamageFromCovid(bool fromCough = false) {
            if (!this.contaminated) {
                photonView.RPC("RPC_TakeDamage", RpcTarget.All, fromCough ? GameManager.instance.GetCoughDamage() : GameManager.instance.GetCovidDamage());
            }
        }

        [PunRPC]
        public void RPC_TakeDamage(float damage) {
            if (!PhotonNetwork.IsMasterClient) {
                return;
            }

            this.currentLife -= damage;

            if (this.currentLife <= 0) {
                this.state = AIState.DIED;
                this.currentLife = 0;
                this.animator.SetInteger("DeathType_int", Random.Range(1, 3));
                this.animator.SetBool("Death_b", true);
                StopAllCoroutines();
                this.agent.ResetPath();
                this.target = null;
            }

            photonView.RPC("RPC_UpdateLife", RpcTarget.All, this.currentLife);
        }

        [PunRPC]
        public void RPC_UpdateLife(float currentLife) {
            this.currentLife = currentLife;

            if (this.currentLife == 0f) {
                this.contaminated = false;
                this.state = AIState.DIED;
                this.controller.enabled = false;
                this.covidArea.SetActive(false);
                return;
            }

            if (this.currentLife < 30f && !this.contaminated) {
                this.SetAsContaminated();
            }
        }

        public void SetAsContaminated() {
            this.contaminated = true;
            this.currentLife = this.maxLife;
            this.covidArea.SetActive(true);

            if (GameManager.localPlayer.IsContaminated()) {
                this.InstantiateContaminedParticle();
            }

            if (PhotonNetwork.IsMasterClient) {
                StartCoroutine(this.CoughRoutine());
            }
        }

        public void InstantiateContaminedParticle() {
            Instantiate(this.contaminedParticle, this.transform.position, Quaternion.identity, this.transform);
        }

        private IEnumerator CoughRoutine() {
            while (this.contaminated) {
                yield return new WaitForSeconds(Random.Range(3, 30));
                this.aiSound.Cough();
            }
        }

        private void OnTriggerExit(Collider other) {
            if (!PhotonNetwork.IsMasterClient) {
                return;
            }

            if (other.CompareTag("CovidArea") && this.covidCoroutine != null) {
                StopCoroutine(this.covidCoroutine);
                this.covidCoroutine = null;
            }
        }

        public bool IsContaminated() {
            return this.contaminated;
        }

        private IEnumerator Idle() {
            this.state = AIState.IDLE;

            while (this.state == AIState.IDLE && !this.target) {
                yield return new WaitForSeconds(1);

                // 1/3 to patrol
                if (Random.Range(0, 3) == 0) {
                    Transform destination = GameManager.instance.GetRandomAIDestination();

                    if (!this.target && Mathf.Abs(Vector3.Distance(this.transform.position, destination.transform.position)) > 2f) {
                        this.SetTarget(destination);
                    }
                }
            }
        }

        public void SetSkinMaterial(int skinIdx) {
            photonView.RPC("RPC_SetSkinMaterial", RpcTarget.All, skinIdx);
        }

        public int GetSkinId() {
            return this.skinId;
        }

        [PunRPC]
        public void RPC_SetSkinMaterial(int skinIdx) {
            this.skinId = skinIdx;

            SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            Material skinMaterial = GameManager.instance.GetSkinMaterialAt(skinIdx);

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers) {
                skinnedMeshRenderer.material = skinMaterial;
            }
        }

        public void SetTarget(Transform target) {
            if (this.idleCoroutine != null) {
                StopCoroutine(this.idleCoroutine);
                this.idleCoroutine = null;
            }

            this.target = target;
            this.agent.SetDestination(this.target.position);

            if (this.moveToTargetCoroutine == null) {
                this.moveToTargetCoroutine = StartCoroutine(this.MoveToTarget());
            }
        }

        private IEnumerator MoveToTarget() {
            this.state = AIState.MOVE_TO_TARGET;

            this.animator.SetFloat("Speed_f", 1);

            while (this.target) {
                if (this.agent.pathPending) {
                    yield return null;
                }

                if (this.agent.remainingDistance <= this.agent.stoppingDistance) {
                    this.target = null;
                    this.agent.ResetPath();
                    this.moveToTargetCoroutine = null;

                    if (this.idleCoroutine != null) {
                        StopCoroutine(this.idleCoroutine);
                    }

                    this.idleCoroutine = StartCoroutine(this.Idle());
                }

                yield return null;
            }

            this.animator.SetFloat("Speed_f", 0);
        }
    }

    public enum AIState {
        MOVE_TO_TARGET,
        IDLE,
        DIED
    }
}