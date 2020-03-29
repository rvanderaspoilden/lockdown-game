using System;
using Photon.Pun;
using UnityEngine;

namespace Game.Player {
    public class PlayerHands : MonoBehaviourPun {
        [Header("Settings")]
        [SerializeField] private Transform skinHandPos;

        [SerializeField] private Transform cameraHandPos;

        [Header("Only for debug")]
        [SerializeField] private Weapon currentWeapon;

        [SerializeField] private Animator animator;

        [SerializeField] private bool showWeapon = false;

        private void Awake() {
            this.animator = GetComponent<Animator>();
            this.UnHandleWeapon();
        }

        public void SetWeapon(Weapon weapon) {
            this.currentWeapon = weapon;
            this.currentWeapon.SetOwner(photonView);
        }

        public bool HasWeapon() {
            return this.currentWeapon != null;
        }

        public void ResetAll() {
            if (!photonView.IsMine) {
                return;
            }

            if (this.currentWeapon) {
                this.currentWeapon.SetVisible(false);
            }
        }

        private void Update() {
            if (!photonView.IsMine) {
                return;
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0f && this.currentWeapon) {
                this.showWeapon = !this.showWeapon;

                if (this.showWeapon) {
                    this.currentWeapon.SetVisible(true);
                    this.HandleWeapon();
                } else {
                    this.currentWeapon.SetVisible(false);
                    this.UnHandleWeapon();
                }
            }

            if (this.currentWeapon && this.currentWeapon.gameObject.activeSelf) {
                // Used to reset reload animation
                if (this.animator.GetBool("Reload_b")) {
                    this.animator.SetBool("Reload_b", false);
                }

                if (Input.GetKeyDown(KeyCode.R)) {
                    this.animator.SetBool("Reload_b", true);
                }

                if (Input.GetMouseButtonDown(0)) {
                    this.currentWeapon.UseWeapon();
                    this.animator.SetBool("Shoot_b", true);
                }

                if (Input.GetMouseButtonUp(0)) {
                    this.currentWeapon.StopUsingWeapon();
                    this.animator.SetBool("Shoot_b", false);
                }  
            }
        }

        public void HandleWeapon() {
            this.animator.SetInteger("WeaponType_int", this.currentWeapon.GetAnimationIntValue());
        }

        public void UnHandleWeapon() {
            this.animator.SetInteger("WeaponType_int", 0);
        }

        public Transform GetHandPos() {
            return photonView.IsMine ? this.cameraHandPos : this.skinHandPos;
        }
    }
}