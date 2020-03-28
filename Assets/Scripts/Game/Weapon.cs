using System;
using System.Collections;
using System.Collections.Generic;
using Game.Player;
using Photon.Pun;
using UnityEngine;

namespace Game {
    public class Weapon : MonoBehaviourPun {
        [Header("Settings")]
        [SerializeField] private float damage;
        [SerializeField] private float range = 5f;
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private WeaponAnimationInt animationInt;
        [SerializeField] private Transform spawnPos;
        [SerializeField] private ParticleSystem shootParticle;

        [Header("Only for debug")]
        [SerializeField] private Collider collider;
        [SerializeField] private Animator animator;
        [SerializeField] private bool isFiring;
        [SerializeField] private float shootTimer;

        private void Awake() {
            this.collider = GetComponent<Collider>();
            this.animator = GetComponent<Animator>();
        }

        public float GetDamage() {
            return this.damage;
        }

        public WeaponType GetWeaponType() {
            return this.weaponType;
        }

        public int GetAnimationIntValue() {
            return (int) Enum.Parse(typeof(WeaponAnimationInt), this.animationInt.ToString());
        }

        public void SetOwner(PhotonView playerView) {
            photonView.TransferOwnership(PhotonNetwork.CurrentRoom.GetPlayer(playerView.OwnerActorNr));
            photonView.RPC("RPC_WeaponOwnerChanged", RpcTarget.All, playerView.ViewID);
        }

        private void Update() {
            if (this.isFiring) {
                this.shootTimer += Time.deltaTime;

                if (this.shootTimer >= this.fireRate) {
                    RaycastHit hit;

                    Debug.DrawRay(GameManager.camera.transform.position, GameManager.camera.transform.TransformDirection(Vector3.forward) * range, Color.blue);
                    if (Physics.Raycast(GameManager.camera.transform.position, GameManager.camera.transform.TransformDirection(Vector3.forward), out hit, range)) {
                        if (hit.collider.CompareTag("Player")) {
                            hit.collider.GetComponent<PlayerEntity>().TakeDamage(this.damage);
                        }
                    }

                    this.shootTimer = 0f;
                }
            }
        }

        public void UseWeapon() {
            this.isFiring = true;
            this.shootTimer = 0f;
            photonView.RPC("RPC_SetShootParticleState", RpcTarget.All, true);
        }

        public void StopUsingWeapon() {
            this.isFiring = false;
            photonView.RPC("RPC_SetShootParticleState", RpcTarget.All, false);
        }

        [PunRPC]
        public void RPC_SetShootParticleState(bool state) {
            if (state) {
                this.shootParticle.Play();
            } else {
                this.shootParticle.Stop();
            }
        }

        [PunRPC]
        public void RPC_WeaponOwnerChanged(int playerViewID) {
            Debug.Log("Weapon owner changed");
            this.collider.enabled = false;
            this.animator.enabled = false;

            if (photonView.IsMine) {
                this.animator.SetBool("hasOwner", true);
            }
            
            this.MoveWeapon(playerViewID);
        }

        public void SetVisible(bool status) {
            photonView.RPC("RPC_SetVisible", RpcTarget.All, status);
        }

        [PunRPC]
        public void RPC_SetVisible(bool status) {
            this.gameObject.SetActive(status);
        }

        private void MoveWeapon(int playerViewID) {
            PlayerHands playerHands = PhotonView.Find(playerViewID).GetComponent<PlayerHands>();
            Transform handPosToUse = playerHands.GetHandPos();

            this.transform.parent = handPosToUse;
            this.transform.rotation = handPosToUse.rotation;
            this.transform.position = handPosToUse.position;

            if (photonView.IsMine) {
                playerHands.HandleWeapon();
            }
        }
    }

    public enum WeaponType {
        HAND_SANITIZER,
        THERMOMETER,
        GLOVES,
        CHLOROQUINE
    }

    public enum WeaponAnimationInt {
        HAND_SANITIZER = 4,
        THERMOMETER = 1,
        GLOVES = 1,
        CHLOROQUINE = 4
    }
}