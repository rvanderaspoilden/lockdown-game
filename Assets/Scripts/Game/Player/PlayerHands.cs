using System;
using Game.Weapons;
using Photon.Pun;
using UnityEngine;

namespace Game.Player {
    public class PlayerHands : MonoBehaviourPun {
        [Header("Settings")]
        [SerializeField] private Transform tpsTwoHandPos;
        [SerializeField] private Transform tpsOneHandPos;

        [SerializeField] private Transform fpsTwoHandPos;
        [SerializeField] private Transform fpsOneHandPos;

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
                this.UnHandleWeapon();
            }
        }

        private void Update() {
            if (!photonView.IsMine || HUDManager.isHudOpened) {
                return;
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0f && this.currentWeapon) {
                this.showWeapon = !this.showWeapon;

                if (this.showWeapon) {
                    this.HandleWeapon();
                } else {
                    this.UnHandleWeapon();
                }
            }

            if (this.currentWeapon && this.currentWeapon.gameObject.activeSelf && this.currentWeapon.GetCurrentAmmo() > 0) {
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
            } else if (this.animator.GetBool("Shoot_b")){
                this.currentWeapon.StopUsingWeapon();
                this.animator.SetBool("Shoot_b", false);
            }
        }

        public void HandleWeapon() {
            if (this.currentWeapon) {
                this.currentWeapon.SetVisible(true);
                this.animator.SetInteger("WeaponType_int", this.currentWeapon.GetAnimationIntValue());
                HUDManager.instance.SetAimVisibility(true);
            }
        }

        public void UnHandleWeapon() {
            if (this.currentWeapon) {
                this.currentWeapon.SetVisible(false);
                this.animator.SetInteger("WeaponType_int", 0);
                HUDManager.instance.SetAimVisibility(false);
            }
        }

        public Transform GetHandPos(WeaponAnimationInt weaponAnimationInt) {
            if (!photonView.IsMine) {
                return weaponAnimationInt == WeaponAnimationInt.ONE_HAND ? this.tpsOneHandPos : this.tpsTwoHandPos;
            }
            
            return weaponAnimationInt == WeaponAnimationInt.ONE_HAND ? this.fpsOneHandPos : this.fpsTwoHandPos;
        }
    }
}