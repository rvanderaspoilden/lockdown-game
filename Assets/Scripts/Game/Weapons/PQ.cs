using System;
using System.Collections;
using Game.AI;
using Photon.Pun;
using UnityEngine;

namespace Game.Weapons {
    public class PQ : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private float autoDestructDuration = 10f;
        [SerializeField] private float moveSpeed = 5;
        [SerializeField] private float radius = 10f;

        [Header("Only for debug")]
        [SerializeField] private Rigidbody rigidbody;

        private bool hasContactedAll = false;

        private void Awake() {
            this.rigidbody = GetComponent<Rigidbody>();
        }

        private void Start() {
            this.rigidbody.AddForce(this.transform.TransformDirection(Vector3.forward) * this.moveSpeed);
        }

        private void OnCollisionEnter(Collision other) {
            if (!PhotonNetwork.IsMasterClient || this.hasContactedAll) {
                return;
            }

            this.hasContactedAll = true;
            
            Collider[] hits = Physics.OverlapSphere(this.transform.position, this.radius, (1 << 13));
            Debug.Log("Pq touched : " + hits.Length);

            foreach (Collider aiCollider in hits) {
                aiCollider.GetComponent<AIController>().SetTarget(this.transform);
            }
            
            StartCoroutine(this.AutoDestruct());
        }

        private IEnumerator AutoDestruct() {
            yield return new WaitForSeconds(this.autoDestructDuration);
            Destroy(this.gameObject);
        }
    }   
}
