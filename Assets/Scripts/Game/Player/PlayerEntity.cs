using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using Photon.Pun;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Game.Player {
    public class PlayerEntity : MonoBehaviour {
        [Header("Fields to complete")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        [SerializeField] private GameObject skinObject;
        [SerializeField] private float sensitivityX;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float jumpSpeed = 10;

        [Header("Only for debug")]
        [SerializeField] private PhotonView photonView;

        [SerializeField] private CharacterController characterController;
        [SerializeField] private Vector3 moveDirection = Vector3.zero;

        private void Awake() {
            this.photonView = GetComponent<PhotonView>();
            this.characterController = GetComponent<CharacterController>();
        }

        private void Start() {
            if (!this.photonView.IsMine) {
                this.virtualCamera.enabled = false;
                this.skinObject.SetActive(true);
            } else {
                this.virtualCamera.enabled = true;
                this.skinObject.SetActive(false);
            }
        }

        private void Update() {
            if (!this.photonView.IsMine && !GameManager.isDebugMode) {
                return;
            }

            // Manage horizontal rotation
            this.transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * this.sensitivityX);

            this.ManageMovement();
        }

        private void ManageMovement() {
            float moveDirectionY = this.moveDirection.y;
            
            this.moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            this.moveDirection = this.transform.TransformDirection(moveDirection);
            this.moveDirection *= this.moveSpeed;
            
            if (this.characterController.isGrounded) {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    this.moveDirection.y = this.jumpSpeed;
                }
            } else {
                this.moveDirection.y = moveDirectionY;
            }

            this.moveDirection.y -= this.gravity * Time.deltaTime;

            this.characterController.Move(this.moveDirection * Time.deltaTime);
        }
    }
}