using System;
using System.Collections;
using System.Collections.Generic;
using Game.AI;
using Game.Player;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Game.Weapons {
    public class Weapon : MonoBehaviourPun {
        [Header("Settings")]
        [SerializeField] protected float damage;

        [SerializeField] protected float range = 5f;
        [SerializeField] protected float fireRate = 0.1f;
        [SerializeField] protected WeaponType weaponType;
        [SerializeField] protected WeaponAnimationInt animationInt;
        [SerializeField] protected Transform spawnPos;
        [SerializeField] protected ParticleSystem shootParticle;
        [SerializeField] protected LayerMask targetLayers;
        [SerializeField] protected AudioClip shootSound;
        [SerializeField] protected bool playSoundLoop;
        [SerializeField] protected WeaponDamageEffect[] damageEffects;
        [SerializeField] protected TextMeshPro displayScreenAmmoText;
        [SerializeField] protected int maxAmmo = 100;
        [SerializeField] protected int ammoPerShoot = 4;

        [Header("Only for debug")]
        [SerializeField] protected Collider collider;

        [SerializeField] protected int currentAmmo;

        [SerializeField] protected Animator animator;
        [SerializeField] protected bool isFiring;
        [SerializeField] protected float shootTimer;
        [SerializeField] protected AudioSource audioSource;

        protected RaycastHit hit;

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

        public WeaponDamageEffect[] GetDamageEffects() {
            return this.damageEffects;
        }

        public int GetCurrentAmmo() {
            return this.currentAmmo;
        }

        private void Update() {
            if (!photonView.IsMine) {
                return;
            }

            if (this.isFiring && this.currentAmmo > 0) {
                if (this.shootTimer == 0) {
                    this.Shoot();

                    this.ConsumeAmmo();
                }

                this.shootTimer += Time.deltaTime;

                if (this.shootTimer >= this.fireRate) {
                    this.shootTimer = 0f;
                }
            }
        }

        protected virtual void Shoot() {
            if (Physics.Raycast(GameManager.camera.transform.position, GameManager.camera.transform.TransformDirection(Vector3.forward), out hit, range, this.targetLayers)) {
                if (this.hit.collider.CompareTag("Player")) {
                    this.hit.collider.GetComponentInParent<PlayerEntity>().TakeDamageFromWeapon(this);
                }

                if (this.hit.collider.CompareTag("AI")) {
                    this.hit.collider.GetComponentInParent<AIController>().TakeDamage(this.damage);
                }
            }
        }

        protected virtual void ConsumeAmmo() {
            this.currentAmmo -= this.ammoPerShoot;

            if (this.currentAmmo < 0) {
                this.currentAmmo = 0;
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
                if (this.shootParticle) {
                    this.shootParticle.Play();
                }

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
                if (this.shootParticle) {
                    this.shootParticle.Stop();
                }

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

    public enum WeaponDamageEffect {
        SLOW = 0,
        BLIND = 1
    }
}