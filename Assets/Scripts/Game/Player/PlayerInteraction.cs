using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Game.Player {
    public class PlayerInteraction : MonoBehaviourPun {
        [Header("Only for debug")]
        [SerializeField] private PlayerHands hands;

        [SerializeField] private PlayerEntity playerEntity;

        private Coroutine covidCoroutine;

        private RaycastHit forwardHit;

        private void Awake() {
            this.playerEntity = GetComponent<PlayerEntity>();
            this.hands = GetComponent<PlayerHands>();
        }

        private void Update() {
            if (!photonView.IsMine) {
                return;
            }

            // Manage interaction with environment
            if (Physics.Raycast(GameManager.camera.transform.position, GameManager.camera.transform.TransformDirection(Vector3.forward), out forwardHit, 4, (1 << 12))) {
                Interactable interactable = forwardHit.collider.GetComponentInParent<Interactable>();

                // TODO UI
                Debug.Log(interactable.GetInformation());

                if (Input.GetKeyDown(KeyCode.E)) {
                    interactable.Interact();
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!photonView.IsMine) {
                return;
            }

            if (other.CompareTag("Weapon") && !this.hands.HasWeapon()) {
                this.hands.SetWeapon(other.GetComponent<Weapon>());
            }
        }

        private void OnTriggerStay(Collider other) {
            if (!photonView.IsMine) {
                return;
            }

            if (other.CompareTag("CovidArea") && covidCoroutine == null && !this.playerEntity.IsContaminated()) {
                this.covidCoroutine = StartCoroutine(this.TakeDamageFromCovid());
            }
        }

        private void OnTriggerExit(Collider other) {
            if (!photonView.IsMine) {
                return;
            }

            if (other.CompareTag("CovidArea") && this.covidCoroutine != null) {
                StopCoroutine(this.covidCoroutine);
                this.covidCoroutine = null;
            }
        }

        private IEnumerator TakeDamageFromCovid() {
            while (!this.playerEntity.IsContaminated()) {
                yield return new WaitForSeconds(3f);
                this.playerEntity.TakeDamageFromCovid();
            }
        }
    }
}