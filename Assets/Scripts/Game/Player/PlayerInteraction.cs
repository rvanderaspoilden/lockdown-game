using System;
using System.Collections;
using System.Collections.Generic;
using Game.AI;
using Game.Weapons;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations;

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
            if (!photonView.IsMine || HUDManager.isHudOpened) {
                return;
            }
            
            HUDManager.instance.SetInformation(String.Empty);

            // Manage interaction with environment
            if (Physics.Raycast(GameManager.camera.transform.position, GameManager.camera.transform.TransformDirection(Vector3.forward), out forwardHit, 4, (1 << 12 | 1 << 9 | 1 << 13))) {
                if (forwardHit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable")) {
                    Interactable interactable = forwardHit.collider.GetComponentInParent<Interactable>();

                    // Set HUD information
                    HUDManager.instance.SetInformation(interactable.GetInformation());

                    if (Input.GetKeyDown(KeyCode.E)) {
                        interactable.Interact();
                    }
                } else if (this.playerEntity.IsPatientZero() && forwardHit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) {
                    PlayerEntity targetPlayerEntity = forwardHit.collider.GetComponentInParent<PlayerEntity>();

                    if (targetPlayerEntity.IsContaminated()) {
                        HUDManager.instance.SetInformation("Press [E] to exchange skin with " + targetPlayerEntity.GetPhotonView().Owner.NickName);

                        if (Input.GetKeyDown(KeyCode.E)) {
                            int playerSkinId = this.playerEntity.GetSkinId();
                            
                            this.playerEntity.SetSkinId(targetPlayerEntity.GetSkinId());
                            targetPlayerEntity.SetSkinId(playerSkinId);
                        }
                    }
                } else if (this.playerEntity.IsPatientZero() && forwardHit.collider.gameObject.layer == LayerMask.NameToLayer("AI")) {
                    AIController aiController = forwardHit.collider.GetComponentInParent<AIController>();

                    if (aiController.IsContaminated()) {
                        HUDManager.instance.SetInformation("Press [E] to exchange skin");

                        if (Input.GetKeyDown(KeyCode.E)) {
                            int playerSkinId = this.playerEntity.GetSkinId();
                            
                            this.playerEntity.SetSkinId(aiController.GetSkinId());
                            aiController.SetSkinMaterial(playerSkinId);
                        }
                    }
                } 
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!photonView.IsMine) {
                return;
            }

            if (other.CompareTag("Weapon") && !this.hands.HasWeapon() && (!this.playerEntity.IsContaminated() || (this.playerEntity.IsContaminated() && other.GetComponent<Weapon>().GetWeaponType() != WeaponType.CHLOROQUINE))) {
                this.hands.SetWeapon(other.GetComponent<Weapon>());
            }
        }

        private void OnTriggerStay(Collider other) {
            if (!photonView.IsMine) {
                return;
            }

            if (other.CompareTag("CovidArea") && covidCoroutine == null && !this.playerEntity.IsContaminated()) {
                this.covidCoroutine = StartCoroutine(this.TakeDamageFromCovid(other.GetComponentInParent<PhotonView>().Controller));
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

        private IEnumerator TakeDamageFromCovid(Photon.Realtime.Player owner) {
            while (!this.playerEntity.IsContaminated()) {
                yield return new WaitForSeconds(0.1f);
                this.playerEntity.TakeDamageFromCovid(owner);
            }
        }
    }
}