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
        [SerializeField] private CinemachineVirtualCamera fpsCamera;
        [SerializeField] private CinemachineFreeLook tpsCamera;
        [SerializeField] private new Camera camera;
        [SerializeField] private GameObject skinObject;
        [SerializeField] private float sensitivityX;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float jumpSpeed = 10;
        [SerializeField] private bool isFrozen = false;
        [SerializeField] private float maxLife = 100f;

        [Header("Only for debug")]
        [SerializeField] private PhotonView photonView;
        [SerializeField] private PlayerHands playerHands;
        [SerializeField] private Animator animator;

        [SerializeField] private CharacterController characterController;
        [SerializeField] private Vector3 moveDirection = Vector3.zero;
        [SerializeField] private float currentLife;

        private void Awake() {
            this.photonView = GetComponent<PhotonView>();
            this.animator = GetComponent<Animator>();
            this.camera = GetComponentInChildren<Camera>();
            this.playerHands = GetComponent<PlayerHands>();
            this.characterController = GetComponent<CharacterController>();
        }

        private void Start() {
            // Manage life
            this.currentLife = this.maxLife;
            
            // Freeze player until warmup begin
            if (!GameManager.isDebugMode) {
                this.Freeze();
            }

            // Manage skin
            int skinId = (int) this.photonView.Owner.CustomProperties["skinId"];
            foreach (SkinnedMeshRenderer renderer in this.skinObject.GetComponentsInChildren<SkinnedMeshRenderer>()) {
                renderer.material = GameManager.instance.GetSkinMaterialAt(skinId);
            }

            if (!this.photonView.IsMine) {
                this.tpsCamera.enabled = false;
                this.fpsCamera.enabled = false;
                this.camera.gameObject.SetActive(false);
                this.skinObject.SetActive(true);
            } else {
                this.camera.gameObject.SetActive(true);
                this.Freeze();
            }
        }

        private void Update() {
            if (!this.photonView.IsMine) {
                return;
            }

            if (this.isFrozen) {
                return;
            }
            
            // Manage horizontal rotation
            this.transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * this.sensitivityX);

            this.ManageMovement();
        }

        public PhotonView GetPhotonView() {
            return this.photonView;
        }

        public void TakeDamage(float damage) {
            photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
        }

        [PunRPC]
        public void RPC_TakeDamage(float damage) {
            Debug.Log("RPC take damage");
            if (!photonView.IsMine) {
                return;
            }
            
            this.currentLife -= damage;

            if (this.currentLife <= 0) {
                this.currentLife = 0;
                this.animator.SetBool("Death_b", true);
                this.playerHands.ResetAll();
                this.Freeze();
            }

            HUDManager.instance.RefreshLifeUI(this.currentLife);

            photonView.RPC("RPC_UpdateLife", RpcTarget.Others, this.currentLife);
        }

        public void Heal(float value) {
            photonView.RPC("RPC_Heal", RpcTarget.All, value);
        }
        
        [PunRPC]
        public void RPC_Heal(float value) {
            if (!photonView.IsMine) {
                return;
            }

            this.currentLife += value;

            if (this.currentLife >= this.maxLife) {
                this.currentLife = maxLife;
            }

            HUDManager.instance.RefreshLifeUI(this.currentLife);

            photonView.RPC("RPC_UpdateLife", RpcTarget.Others, this.currentLife);
        }

        [PunRPC]
        public void RPC_UpdateLife(float currentLife) {
            this.currentLife = currentLife;
        }

        public void Freeze() {
            this.isFrozen = true;
            this.skinObject.SetActive(true);
            this.tpsCamera.enabled = true;
            this.fpsCamera.enabled = false;
        }

        public void UnFreeze() {
            this.isFrozen = false;
            this.skinObject.SetActive(false);
            this.tpsCamera.enabled = false;
            this.fpsCamera.enabled = true;
        }

        private void ManageMovement() {
            this.animator.SetBool("Jump_b", false); // todo

            float moveDirectionY = this.moveDirection.y;

            this.moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            this.moveDirection = this.transform.TransformDirection(moveDirection);
            this.moveDirection *= this.moveSpeed;

            if (this.characterController.isGrounded) {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    this.moveDirection.y = this.jumpSpeed;
                    this.animator.SetBool("Jump_b", true);
                }
            } else {
                this.moveDirection.y = moveDirectionY;
            }

            this.moveDirection.y -= this.gravity * Time.deltaTime;

            Vector3 animMoveSpeed = new Vector3(this.moveDirection.x, 0, this.moveDirection.z);
            this.animator.SetFloat("Speed_f", (animMoveSpeed.magnitude / this.moveSpeed));

            this.characterController.Move(this.moveDirection * Time.deltaTime);
        }
    }
}