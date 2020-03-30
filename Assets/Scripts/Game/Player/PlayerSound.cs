using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Game.Player {
    public class PlayerSound : MonoBehaviourPun {
        [Header("Settings")]
        [SerializeField] private AudioClip coughSound;

        [Header("Only for debug")]
        [SerializeField] private AudioSource audioSource;

        private void Awake() {
            this.audioSource = GetComponentInChildren<AudioSource>();
        }

        public void Cough() {
            photonView.RPC("RPC_Cough", RpcTarget.All);
        }

        [PunRPC]
        public void RPC_Cough() {
            this.audioSource.PlayOneShot(this.coughSound);
        }
    }
}