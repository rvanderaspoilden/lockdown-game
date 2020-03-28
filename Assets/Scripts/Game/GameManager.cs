using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Player;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

namespace Game {
    public class GameManager : MonoBehaviourPun {
        [Header("Fields to complete")]
        [SerializeField] private GameObject playerPrefab;

        [SerializeField] private Transform[] playerSpawns;
        [SerializeField] private Transform[] weaponSpawns;
        [SerializeField] private Material[] skinMaterials;
        [SerializeField] private Weapon[] weaponPrefabs;

        [Header("Only for debug")]
        public static new Camera camera;

        public static PlayerEntity localPlayer;

        public static bool isDebugMode = false;

        public static GameManager instance;

        private void Awake() {
            instance = this;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start() {
            // Manage spawn            
            int spawnId = (int) PhotonNetwork.LocalPlayer.CustomProperties["spawnId"];
            GameObject playerObj = PhotonNetwork.Instantiate("Prefabs/Game/" + this.playerPrefab.name, this.playerSpawns[spawnId].transform.position, Quaternion.identity);
            localPlayer = playerObj.GetComponent<PlayerEntity>();
            camera = localPlayer.GetComponentInChildren<Camera>();

            if (PhotonNetwork.IsMasterClient) {
                this.InstantiateWeapons();
                StartCoroutine(this.CheckToStartWarmup());
            }
        }

        private IEnumerator CheckToStartWarmup() {
            bool allPlayersReady = false;

            do {
                Debug.Log("Check players....");
                allPlayersReady = GameObject.FindGameObjectsWithTag("Player").Length == PhotonNetwork.CurrentRoom.PlayerCount;
                yield return new WaitForSeconds(1);
            } while (!allPlayersReady);

            Debug.Log("Start WARMUP");
            photonView.RPC("UnFreezePlayer", RpcTarget.All);
        }

        [PunRPC]
        private void UnFreezePlayer() {
            GameManager.localPlayer.UnFreeze();
            Debug.Log("I'm unfrozen");
        }

        private void InstantiateWeapons() {
            foreach (Transform spawn in this.playerSpawns) {
                PhotonNetwork.Instantiate("Prefabs/Game/" + this.weaponPrefabs[Random.Range(0, this.weaponPrefabs.Length)].name, spawn.position, Quaternion.identity);
            }
        }

        private void OnDestroy() {
            PhotonNetwork.RemoveCallbackTarget(this);
            StopAllCoroutines();
        }

        public Material GetSkinMaterialAt(int idx) {
            return this.skinMaterials[idx];
        }
    }
}