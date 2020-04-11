using System;
using Game;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Door : Interactable {
    [Header("Settings")]
    [SerializeField] private AudioClip wristSound;

    [SerializeField] private string information;

    [Header("Only for debug")]
    [SerializeField] private bool isOpen;

    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    private void Awake() {
        this.animator = GetComponent<Animator>();
        this.audioSource = GetComponentInChildren<AudioSource>();
    }

    public override string GetInformation() {
        return String.Format(this.information, this.isOpen ? "close" : "open");
    }

    public override void Interact(Player player) {
        photonView.RPC("RPC_Interact", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void RPC_Interact() {
        this.isOpen = !this.isOpen;

        this.audioSource.PlayOneShot(wristSound);

        if (PhotonNetwork.IsMasterClient) {
            this.animator.SetBool("isOpen", isOpen);
        }
    }
}