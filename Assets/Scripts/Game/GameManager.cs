using System;
using System.Collections;
using System.Collections.Generic;
using Game.Player;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game {
    public class GameManager : MonoBehaviourPun {
        
        [Header("Fields to complete")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject[] playerSpawns;
        [SerializeField] private Material[] skinMaterials;

        [Header("Only for debug")]
        public static new Camera camera;
        public static PlayerEntity localPlayer;
        
        public static bool isDebugMode = false;

        public static GameManager instance;

        private void Awake() {
            instance = this;
            camera = Camera.main;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start() {
            // Manage spawn            
            int spawnId = (int)PhotonNetwork.LocalPlayer.CustomProperties["spawnId"];
            GameObject playerObj = PhotonNetwork.Instantiate("Prefabs/Game/" + this.playerPrefab.name, this.playerSpawns[spawnId].transform.position, Quaternion.identity);
            localPlayer = playerObj.GetComponent<PlayerEntity>();
        }

        private void OnDestroy() {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public Material GetSkinMaterialAt(int idx) {
            return this.skinMaterials[idx];
        }
    }   
}
