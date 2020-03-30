using System;
using Game;
using Photon.Pun;
using UnityEngine;

public class Door : Interactable {
    [Header("Settings")]
    [SerializeField] private string information;

    [SerializeField] private AudioClip wristSound;

    [Header("Only for debug")]
    [SerializeField] private bool isOpen;

    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    private void Awake() {
        this.animator = GetComponent<Animator>();
        this.audioSource = GetComponentInChildren<AudioSource>();
    }

    public override string GetInformation() {
        return this.information;
    }

    public override void Interact() {
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