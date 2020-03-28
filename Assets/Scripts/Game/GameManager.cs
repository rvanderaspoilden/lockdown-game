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

        [Header("Only for debug")]
        public static new Camera camera;
        
        public static bool isDebugMode = false;

        private void Awake() {
            camera = Camera.main;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start() {
            PhotonNetwork.Instantiate("Prefabs/Game/" + this.playerPrefab.name, new Vector3(Random.Range(-10f, 10f), 1, Random.Range(-10f, 10f)), Quaternion.identity);
        }

        private void OnDestroy() {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }   
}
