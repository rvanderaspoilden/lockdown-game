using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game {
    public class GameManager : MonoBehaviourPun {
        
        [Header("Fields to complete")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject[] playerSpawns;

        [Header("Only for debug")]
        public static new Camera camera;
        
        public static bool isDebugMode = false;

        private void Awake() {
            camera = Camera.main;
            this.playerSpawns = GameObject.FindGameObjectsWithTag("PlayerSpawn");
            
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start() {
            // Manage spawn            
            int spawnId = (int)PhotonNetwork.LocalPlayer.CustomProperties["spawnId"];
            PhotonNetwork.Instantiate("Prefabs/Game/" + this.playerPrefab.name, this.playerSpawns[spawnId].transform.position, Quaternion.identity);
            
            // Manage skin
            int skinId = (int)PhotonNetwork.LocalPlayer.CustomProperties["skinId"];
        }

        private void OnDestroy() {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }   
}
