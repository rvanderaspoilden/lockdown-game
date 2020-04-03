using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using Game.AI;
using Photon.Pun;
using UnityEngine;
using Utils;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace Game.Player {
    public class PlayerEntity : MonoBehaviour {
        [Header("Fields to complete")]
        [SerializeField] private CinemachineVirtualCamera fpsCamera;

        [SerializeField] private CinemachineFreeLook tpsCamera;
        [SerializeField] private new Camera camera;
        [SerializeField] private GameObject skinObject;
        [SerializeField] private float sensitivityX;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float sprintMultiplier = 1.2f;
        [SerializeField] private float slowMultiplier = 0.5f;
        [SerializeField] private float jumpSpeed = 10;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float maxLife = 100f;
        [SerializeField] private GameObject covidArea;
        [SerializeField] private float coughRadius = 2f;

        [Header("Only for debug")]
        [SerializeField] private PhotonView photonView;

        [SerializeField] private PlayerHands playerHands;
        [SerializeField] private PlayerSound playerSound;
        [SerializeField] private Animator animator;
        
        [SerializeField] private bool patientZero;
        
        [SerializeField] private bool isFrozen = false;

        [SerializeField] private bool contaminated;

        [SerializeField] private CharacterController characterController;
        [SerializeField] private Vector3 moveDirection = Vector3.zero;
        [SerializeField] private float currentLife;
        [SerializeField] private bool canCough;

        [SerializeField] private bool isSlowed;
        [SerializeField] private float slowTimer;
        private Coroutine slowEffectCoroutine;

        private void Awake() {
            this.photonView = GetComponent<PhotonView>();
            this.animator = GetComponent<Animator>();
            this.camera = GetComponentInChildren<Camera>();
            this.playerSound = GetComponent<PlayerSound>();
            this.playerHands = GetComponent<PlayerHands>();
            this.characterController = GetComponent<CharacterController>();
        }

        private void OnDestroy() {
            StopAllCoroutines();
        }

        private void Start() {
            // Manage life and contamination
            this.currentLife = this.maxLife;
            this.contaminated = false;
            this.covidArea.SetActive(false);
            this.canCough = true;

            // Manage skin
            int skinId = (int) this.photonView.Owner.CustomProperties["skinId"];
            foreach (SkinnedMeshRenderer renderer in this.skinObject.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                renderer.material = GameManager.instance.GetSkinMaterialAt(skinId);
            }

            this.skinObject.SetActive(true);

            if (!this.photonView.IsMine) {
                this.tpsCamera.enabled = false;
                this.fpsCamera.enabled = false;
                this.camera.gameObject.SetActive(false);
            } else {
                this.camera.gameObject.SetActive(true);
                this.Freeze();
            }
        }

        private void Update() {
            if (!this.photonView.IsMine) {
                return;
            }

            if (this.isFrozen) {
                return;
            }

            // Manage horizontal rotation
            if (!HUDManager.isHudOpened) {
                this.transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * this.sensitivityX);

                if (Input.GetMouseButtonDown(1) && this.canCough) {
                    StartCoroutine(this.Cough());
                }
            }

            this.ManageMovement();
        }

        public PhotonView GetPhotonView() {
            return this.photonView;
        }

        public void LockCamera() {
            this.fpsCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = 0f;
        }

        public void UnlockCamera() {
            this.fpsCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = 300f;
        }

        public bool IsPatientZero() {
            return this.patientZero;
        }

        public void SetAsPatientZero() {
            photonView.RPC("RPC_SetAsPatientZero", RpcTarget.All);
        }

        public bool IsDied() {
            return this.currentLife == 0f;
        }

        [PunRPC]
        public void RPC_SetAsPatientZero() {
            this.patientZero = true;
            this.SetAsContaminated();
        }

        public void TakeDamageFromWeapon(Weapon weapon) {
            if (this.contaminated) {
                photonView.RPC("RPC_TakeDamage", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, weapon.GetDamage());
            }

            photonView.RPC("RPC_ApplyDamageEffects", photonView.Owner, weapon.photonView.ViewID);
        }

        public void TakeDamageFromCovid(Photon.Realtime.Player owner) {
            if (!this.contaminated) {
                photonView.RPC("RPC_TakeDamage", RpcTarget.All, owner.ActorNumber, GameManager.instance.GetCovidDamage());
            }
        }

        public bool IsContaminated() {
            return this.contaminated;
        }

        [PunRPC]
        public void RPC_ApplyDamageEffects(int weaponViewID) {
            PhotonView weapongView = PhotonView.Find(weaponViewID);

            if (weapongView == null) {
                Debug.LogError("Weapon view not found");
            }

            foreach (WeaponDamageEffect effect in weapongView.GetComponent<Weapon>().GetDamageEffects()) {
                if (effect == WeaponDamageEffect.BLIND) {
                    HUDManager.instance.Blind();
                } else if (effect == WeaponDamageEffect.SLOW) {
                    this.slowTimer = 3;

                    if (this.slowEffectCoroutine == null) {
                        this.slowEffectCoroutine = StartCoroutine(this.SlowEffect());
                    }
                }
            }
        }

        private IEnumerator SlowEffect() {
            this.isSlowed = true;
            while (this.slowTimer > 0) {
                this.slowTimer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            this.isSlowed = false;

            this.slowEffectCoroutine = null;
        }

        [PunRPC]
        public void RPC_TakeDamage(int actorNbr, float damage) {
            if (!photonView.IsMine) {
                return;
            }

            Debug.Log("I took RPC damage from actor nbr: " + actorNbr);

            this.currentLife -= damage;

            if (this.currentLife <= 0) {
                this.currentLife = 0;
                this.animator.SetBool("Death_b", true);
                this.playerHands.ResetAll();
                this.Freeze();

                // Todo send message to killer
            }

            // Adjust contamined camera filter in function of lost life
            HUDManager.instance.SetContaminedCameraFilterOpacity((this.maxLife - this.currentLife) / this.maxLife);

            photonView.RPC("RPC_UpdateLife", RpcTarget.All, this.currentLife);
        }

        public void Heal(float value) {
            photonView.RPC("RPC_Heal", RpcTarget.All, value);
        }

        [PunRPC]
        public void RPC_Heal(float value) {
            if (!photonView.IsMine) {
                return;
            }

            this.currentLife += value;

            if (this.currentLife >= this.maxLife) {
                this.currentLife = maxLife;
            }

            photonView.RPC("RPC_UpdateLife", RpcTarget.Others, this.currentLife);
        }

        [PunRPC]
        public void RPC_UpdateLife(float currentLife) {
            this.currentLife = currentLife;

            if (this.currentLife == 0f) {
                this.contaminated = false;
                StopAllCoroutines();
                this.characterController.enabled = false;
                this.covidArea.SetActive(false);
                GameManager.instance.CheckContaminedNumber();
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

            if (photonView.IsMine) {
                HUDManager.instance.SetContaminedStatus(true);
                StartCoroutine(this.CoughRoutine());
            }

            if (PhotonNetwork.IsMasterClient) {
                GameManager.instance.CheckContaminedNumber();
            }
        }

        public void Freeze() {
            this.isFrozen = true;
            RoomUtils.SetLayerRecursively(this.skinObject, LayerMask.NameToLayer("Player"));
            this.tpsCamera.enabled = true;
            this.fpsCamera.enabled = false;
            this.playerHands.UnHandleWeapon();
        }

        public void UnFreeze() {
            this.isFrozen = false;
            this.StartCoroutine(this.HidePlayerSkin());
            this.tpsCamera.enabled = false;
            this.fpsCamera.enabled = true;
            this.playerHands.HandleWeapon();
        }

        private IEnumerator HidePlayerSkin() {
            yield return new WaitForSeconds(1);
            RoomUtils.SetLayerRecursively(this.skinObject, LayerMask.NameToLayer("MinePlayerSkin"));
        }

        private IEnumerator CoughRoutine() {
            while (this.contaminated && !GameManager.gameEnded) {
                yield return new WaitForSeconds(UnityEngine.Random.Range(this.IsPatientZero() ? 30 : 3, this.IsPatientZero() ? 90 : 30));

                if (this.canCough) {
                    StartCoroutine(this.Cough());
                }
            }
        }

        private IEnumerator Cough() {
            // In case of concurrent coroutines
            if (!this.canCough) {
                yield return null;
            }

            this.canCough = false;

            this.playerSound.Cough();

            if (this.IsContaminated()) {
                Collider[] hitColliders = Physics.OverlapSphere(this.transform.position + new Vector3(0, 2f, 0), this.coughRadius, (1 << 9 | 1 << 13));

                foreach (Collider hit in hitColliders) {
                    if (hit.CompareTag("Player")) {
                        hit.GetComponent<PlayerEntity>().TakeDamageFromCovid(PhotonNetwork.LocalPlayer);
                    }

                    if (hit.CompareTag("AI")) {
                        hit.GetComponent<AIController>().TakeDamageFromCovid(); // todo manager owner
                    }
                }
            }

            yield return new WaitForSeconds(2f);

            this.canCough = true;
        }

        private void ManageMovement() {
            this.animator.SetBool("Jump_b", false); // todo
            
            float moveDirectionY = this.moveDirection.y;

            this.moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            this.moveDirection = this.transform.TransformDirection(moveDirection);

            float speedMultiplier = this.moveSpeed;
            
            if (Input.GetKey(KeyCode.LeftShift)) {
                speedMultiplier *= this.sprintMultiplier;
            }

            if (this.isSlowed) {
                speedMultiplier *= this.slowMultiplier;
            }
            
            this.moveDirection *= speedMultiplier;

            if (this.characterController.isGrounded) {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    this.moveDirection.y = this.jumpSpeed;
                    this.animator.SetBool("Jump_b", true);
                }
            } else {
                this.moveDirection.y = moveDirectionY;
            }

            this.moveDirection.y -= this.gravity * Time.deltaTime;

            Vector3 animMoveSpeed = new Vector3(this.moveDirection.x, 0, this.moveDirection.z);
            this.animator.SetFloat("Speed_f", (animMoveSpeed.magnitude / this.moveSpeed));

            this.characterController.Move(this.moveDirection * Time.deltaTime);
        }
    }
}