using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game {
    public class GameManager : MonoBehaviourPun {
        [SerializeField] private GameObject playerPrefab;
        
        private void Awake() {
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
