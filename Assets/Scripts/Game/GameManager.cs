using System;
using System.Collections;
using Game.AI;
using Game.Player;
using Game.Weapons;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace Game {
    public class GameManager : MonoBehaviourPunCallbacks {
        [Header("Fields to complete")]
        [SerializeField] private GameObject playerPrefab;

        [SerializeField] private Transform[] playerSpawns;
        [SerializeField] private Transform[] weaponSpawns;
        [SerializeField] private Material[] skinMaterials;
        [SerializeField] private Weapon[] weaponPrefabs;
        [SerializeField] private Transform[] destinationForAI;
        [SerializeField] private int warmupDuration;
        [SerializeField] private int escapeDuration;
        [SerializeField] private float covidDamage;

        [Header("Only for debug")]
        [SerializeField] private Clock clock;

        [SerializeField] private AudioSource audioSource;
        
        public static new Camera camera;

        public static PlayerEntity localPlayer;

        public static bool gameEnded = false;

        public static bool isWarmup = false;

        public static GameManager instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else {
                Destroy(instance);
                instance = this;
            }

            PhotonNetwork.AddCallbackTarget(this);

            this.clock = GameObject.FindObjectOfType<Clock>();
            this.audioSource = GetComponent<AudioSource>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start() {
            // Manage preferences
            if (!PlayerPrefs.HasKey("mute")) {
                PlayerPrefs.SetInt("mute", 0);
                this.SetMuteMusic(false);
            } else {
                this.SetMuteMusic(PlayerPrefs.GetInt("mute") == 1);
            }
            
            // Manage spawn            
            int spawnId = (int) PhotonNetwork.LocalPlayer.CustomProperties["spawnId"];
            Transform spawnToUse = this.playerSpawns[spawnId].transform;
            GameObject playerObj = PhotonNetwork.Instantiate("Prefabs/Game/" + this.playerPrefab.name, spawnToUse.position, spawnToUse.rotation);
            localPlayer = playerObj.GetComponent<PlayerEntity>();
            camera = localPlayer.GetComponentInChildren<Camera>();

            if (PhotonNetwork.IsMasterClient) {
                StartCoroutine(this.ManageTimer());
            }
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (HUDManager.isHudOpened) {
                    HUDManager.instance.CloseOptions();
                } else {
                    HUDManager.instance.OpenOptions();
                }
            }
        }

        private IEnumerator ManageTimer() {
            bool allPlayersReady = false;
            GameObject[] players;

            // Check all players are instantiated
            do {
                Debug.Log("Check players....");
                players = GameObject.FindGameObjectsWithTag("Player");
                allPlayersReady = players.Length == PhotonNetwork.CurrentRoom.PlayerCount;
                yield return new WaitForSeconds(1);
            } while (!allPlayersReady);
            
            // Hide loading canvas
            photonView.RPC("RPC_HideLoader", RpcTarget.All);

            // Change AI skins
            AIController[] aiControllers = FindObjectsOfType<AIController>();

            foreach (AIController aiController in aiControllers) {
                aiController.SetSkinMaterial(Random.Range(0, this.skinMaterials.Length));
            }
            
            // Open random doors
            this.OpenRandomDoors();
            
            // Start Music
            photonView.RPC("RPC_StartMusic", RpcTarget.All);

            // Unfreeze all players
            photonView.RPC("RPC_UnFreezePlayer", RpcTarget.All);

            // Instantiate all weapons
            this.InstantiateWeapons();
            
            // Start Warm-up
            isWarmup = true;
            int counter = this.warmupDuration;
            
            AlertManager.instance.Alert("Warm-up", AlertType.GENERAL, RpcTarget.All);
            
            while (counter > 0) {
                yield return new WaitForSeconds(1);
                counter--;
            }
            isWarmup = false;
            
            // Start clock
            photonView.RPC("RPC_StartClock", RpcTarget.All);

            // Choose patient zero
            players[Random.Range(0, players.Length)].GetComponent<PlayerEntity>().SetAsPatientZero();

            // Start Escape Timer
            yield return new WaitForSeconds(this.escapeDuration);
            
            // Stop clock
            photonView.RPC("RPC_StopClock", RpcTarget.All);

            // Time is up
            this.EndGame();
        }

        private void OpenRandomDoors() {
            Door[] doors = FindObjectsOfType<Door>();

            for (int i = 0; i < 6; i++) {
                doors[Random.Range(0, doors.Length)].Interact();
            }
        }

        [PunRPC]
        public void RPC_StartMusic() {
            this.audioSource.Play();
        }

        [PunRPC]
        public void RPC_HideLoader() {
            LoadingManager.instance.Hide();
        }

        [PunRPC]
        public void RPC_StartClock() {
            this.clock.StartClock();
        }
        
        [PunRPC]
        public void RPC_StopClock() {
            this.clock.StopClock();
        }

        public void SetMuteMusic(bool isMute) {
            this.audioSource.mute = isMute;
        }

        public void CheckContaminedNumber() {
            PlayerEntity[] players = FindObjectsOfType<PlayerEntity>();

            Debug.Log("Check contamined number");
            int counter = 0;
            foreach (PlayerEntity player in players) {
                if (player.IsContaminated() || player.IsDied()) {
                    counter++;
                }
            }

            // If all players are contaminated or there is no one
            if (counter == players.Length) {
                AlertManager.instance.Alert("Covid19 won this game", AlertType.CONTAMINED, RpcTarget.All);
                this.EndGame();
            } else if (counter == 0) {
                AlertManager.instance.Alert("Confined won this game", AlertType.CONFINED, RpcTarget.All);
                this.EndGame();
            }
        }

        public void LeaveGame() {
            PhotonNetwork.LeaveRoom();
        }

        public Transform GetRandomAIDestination() {
            return this.destinationForAI[Random.Range(0, this.destinationForAI.Length)];
        }

        public float GetCovidDamage() {
            return this.covidDamage;
        }

        private void EndGame() {
            photonView.RPC("RPC_FreezePlayer", RpcTarget.All);

            gameEnded = true;

            StopAllCoroutines();

            this.SaveScoring();

            StartCoroutine(this.BackToLobby());
        }

        private IEnumerator BackToLobby() {
            yield return new WaitForSeconds(5);
            PhotonNetwork.LoadLevel("Lobby");
        }

        private void SaveScoring() {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in players) {
                PlayerEntity playerEntity = player.GetComponent<PlayerEntity>();
                PlayerScore playerScore = player.GetComponent<PlayerScore>();

                float sum = 0;

                if (playerEntity.IsPatientZero()) {
                    sum += (playerScore.GetContaminedPlayer() * 2) + playerScore.GetContaminedAI();
                } else if (playerEntity.IsContaminated()) {
                    sum -= 2;
                    sum += (playerScore.GetContaminedPlayer() * .5f);

                    if (playerEntity.IsDied()) {
                        sum -= 1;
                    }
                } else {
                    sum += 2;
                }

                sum += playerScore.GetContaminedKilled();

                Photon.Realtime.Player photonPlayer = playerEntity.GetPhotonView().Owner;
                Hashtable data = photonPlayer.CustomProperties;

                if (data.ContainsKey("score")) {
                    data["score"] = (float) data["score"] + sum;
                } else {
                    data["score"] = sum;
                }

                photonPlayer.SetCustomProperties(data);
            }
        }

        [PunRPC]
        private void RPC_UnFreezePlayer() {
            localPlayer.UnFreeze();
        }

        [PunRPC]
        private void RPC_FreezePlayer() {
            HUDManager.instance.CloseOptions();
            
            localPlayer.Freeze();
        }

        private void InstantiateWeapons() {
            foreach (Transform spawn in this.weaponSpawns) {
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

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
            if (PhotonNetwork.IsMasterClient) {
                Debug.Log("Player left game");
                if (isWarmup) { // if warmup and player left the game => go back to lobby
                    PhotonNetwork.LoadLevel("Lobby");
                } else { // else check contamined number and finish the game so
                    StartCoroutine(this.CheckContaminedPlayerCoroutine());
                }
            }
        }

        private IEnumerator CheckContaminedPlayerCoroutine() {
            yield return new WaitForSeconds(2);
            CheckContaminedNumber();
        }

        public override void OnLeftRoom() {
            Debug.Log("I leave game");
            SceneManager.LoadScene("Lobby");
        }
    }
}