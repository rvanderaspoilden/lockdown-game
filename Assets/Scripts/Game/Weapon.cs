using System;
using System.Collections;
using System.Collections.Generic;
using Game.AI;
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
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private bool playSoundLoop;

        [Header("Only for debug")]
        [SerializeField] private Collider collider;

        [SerializeField] private Animator animator;
        [SerializeField] private bool isFiring;
        [SerializeField] private float shootTimer;
        [SerializeField] private AudioSource audioSource;

        private RaycastHit hit;

        private void Awake() {
            this.collider = GetComponent<Collider>();
            this.animator = GetComponent<Animator>();
            this.audioSource = GetComponent<AudioSource>();
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
                if (this.shootTimer == 0) {
                    if (Physics.Raycast(GameManager.camera.transform.position, GameManager.camera.transform.TransformDirection(Vector3.forward), out hit, range, this.targetLayers)) {
                        if (this.hit.collider.CompareTag("Player")) {
                            Debug.Log("Touched player : " + this.hit.collider.name);
                            this.hit.collider.GetComponentInParent<PlayerEntity>().TakeDamage(this.damage);
                        }

                        if (this.hit.collider.CompareTag("AI")) {
                            Debug.Log("Touched AI : " + this.hit.collider.name);
                            this.hit.collider.GetComponentInParent<AIController>().TakeDamage(this.damage);
                        }
                    }
                }

                this.shootTimer += Time.deltaTime;

                if (this.shootTimer >= this.fireRate) {
                    this.shootTimer = 0f;
                }
            }
        }

        public void UseWeapon() {
            this.isFiring = true;
            this.shootTimer = 0f;

            photonView.RPC("RPC_SetShootState", RpcTarget.All, true);
        }

        public void StopUsingWeapon() {
            this.isFiring = false;

            photonView.RPC("RPC_SetShootState", RpcTarget.All, false);
        }

        [PunRPC]
        public void RPC_SetShootState(bool state) {
            if (state) {
                this.shootParticle.Play();
                
                if (this.shootSound) {
                    if (this.playSoundLoop) {
                        this.audioSource.clip = this.shootSound;
                        this.audioSource.loop = true;
                        this.audioSource.Play();
                    } else {
                        this.audioSource.PlayOneShot(this.shootSound);
                    }
                }
            } else {
                this.shootParticle.Stop();
                
                if (this.shootSound && this.playSoundLoop) {
                    this.audioSource.clip = null;
                    this.audioSource.loop = false;
                    this.audioSource.Stop();
                }
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