using System;
using System.Collections;
using System.Collections.Generic;
using Game.AI;
using Game.Player;
using Photon.Pun;
using UnityEngine;

namespace Game.Weapons {
    public class Thermometer : Weapon {
        private void Start() {
            StartCoroutine(this.UpdateDisplayText(String.Empty, Color.white));
        }

        protected override void Shoot() {
            photonView.RPC("RPC_SetShootState", RpcTarget.All, true);

            if (Physics.Raycast(GameManager.camera.transform.position, GameManager.camera.transform.TransformDirection(Vector3.forward), out hit, range, this.targetLayers)) {
                if (this.hit.collider.CompareTag("Player")) {
                    bool isContaminated = this.hit.collider.GetComponentInParent<PlayerEntity>().IsContaminated();
                    StartCoroutine(this.UpdateDisplayText(isContaminated ? "41°" : "37.5°", isContaminated ? Color.red : Color.green));
                }

                if (this.hit.collider.CompareTag("AI")) {
                    bool isContaminated = this.hit.collider.GetComponentInParent<AIController>().IsContaminated();
                    StartCoroutine(this.UpdateDisplayText(isContaminated ? "41°" : "37.5°", isContaminated ? Color.red : Color.green));
                }
            }
        }
        
        private IEnumerator UpdateDisplayText(string message, Color color) {
            this.displayScreenAmmoText.text = message;
            this.displayScreenAmmoText.color = color;

            yield return new WaitForSeconds(2f);
            
            this.displayScreenAmmoText.text = String.Empty;
            this.displayScreenAmmoText.color = Color.white;
        }
    }
}