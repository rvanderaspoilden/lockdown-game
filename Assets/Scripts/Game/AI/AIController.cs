using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Game.AI {
    public class AIController : MonoBehaviourPun {
        [Header("Only for debug")]
        [SerializeField] private NavMeshAgent agent;

        [SerializeField] private Transform target;

        [SerializeField] private Animator animator;
        [SerializeField] private AIState state;

        private void Awake() {
            this.agent = GetComponent<NavMeshAgent>();
            this.animator = GetComponent<Animator>();
        }

        private void Start() {
            if (!PhotonNetwork.IsMasterClient) {
                return;
            }

            StartCoroutine(this.Idle());
        }

        private void OnDestroy() {
            StopAllCoroutines();
        }

        private IEnumerator Idle() {
            this.state = AIState.IDLE;

            while (this.state == AIState.IDLE) {
                yield return new WaitForSeconds(1);

                // 1/3 to patrol
                if (Random.Range(0, 3) == 0) {
                    Transform destination = GameManager.instance.GetRandomAIDestination();

                    if (Mathf.Abs(Vector3.Distance(this.transform.position, destination.transform.position)) > 2f) {
                        this.SetTarget(destination);
                    }
                }
            }
        }

        public void SetTarget(Transform target) {
            if (this.target) {
                this.target = target;
                this.agent.SetDestination(this.target.position);
            } else {
                this.target = target;
                this.agent.SetDestination(this.target.position);
                StartCoroutine(this.MoveToTarget());
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
                    StartCoroutine(this.Idle());
                }

                yield return null;
            }

            this.animator.SetFloat("Speed_f", 0);
        }
    }

    public enum AIState {
        MOVE_TO_TARGET,
        IDLE
    }
}