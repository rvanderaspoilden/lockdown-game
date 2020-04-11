using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Photon.Pun;
using UnityEngine;

namespace Game {
    public class Sink : Interactable {
        [Header("Settings")]
        [SerializeField] private AudioClip waterSound;

        [SerializeField] private string information;

        [Header("Only for debug")]
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private Animator animator;
        
        private Photon.Realtime.Player isUsedBy;

        private void Awake() {
            this.audioSource = GetComponent<AudioSource>();
            this.animator = GetComponent<Animator>();
        }

        public override string GetInformation() {
            if (this.isUsedBy == null) {
                return this.information;
            }
            return String.Empty;
        }

        public override void Interact(Photon.Realtime.Player player) {
            if (this.isUsedBy == null) {
                photonView.RPC("RPC_Interact", RpcTarget.All, player);
            }
        }

        [PunRPC]
        public void RPC_Interact(Photon.Realtime.Player player) {
            if (this.isUsedBy == null) {
                this.isUsedBy = player;

                this.audioSource.PlayOneShot(this.waterSound);

                StartCoroutine(this.Cooldown());

                if (PhotonNetwork.LocalPlayer.ActorNumber == player.ActorNumber && !GameManager.localPlayer.IsContaminated()) {
                    GameManager.localPlayer.Heal(20f);
                }
            }
        }

        private IEnumerator Cooldown() {
            this.animator.SetTrigger("use");
            yield return new WaitForSeconds(5f);
            this.isUsedBy = null;
        }
    }
}