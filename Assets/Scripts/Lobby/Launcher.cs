using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Lobby {
    public class Launcher : MonoBehaviourPunCallbacks {
        [Header("Settings")]
        [SerializeField] private byte maxPlayers;

        [Header("UI fields for authentification")]
        [SerializeField] private GameObject authentificationPanel;

        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private TMP_InputField roomIdInputField;

        [Header("UI fields for room")]
        [SerializeField] private GameObject roomPanel;

        [SerializeField] private TextMeshProUGUI roomIdText;
        [SerializeField] private GameObject scrollviewContent;
        [SerializeField] private LobbyPlayer lobbyPlayerPrefab;
        [SerializeField] private Button startButton;

        [Header("Only for debug")]
        [SerializeField] private string username;

        [SerializeField] private string roomId;

        private Dictionary<int, LobbyPlayer> lobbyPlayers;

        private void Awake() {
            this.DisplayAuthentificationPanel();

            this.lobbyPlayers = new Dictionary<int, LobbyPlayer>(this.maxPlayers);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Start is called before the first frame update
        void Start() {
            // Listener on inputs
            this.usernameInputField.onValueChanged.AddListener((string value) => this.SetUsername(value));
            this.roomIdInputField.onValueChanged.AddListener((string value) => this.SetRoomId(value));

            // Photon connection
            PhotonNetwork.AutomaticallySyncScene = true;

            if (PhotonNetwork.IsConnected) {
                if (PhotonNetwork.CurrentRoom != null) {
                    this.DisplayRoomPanel();
                }
            } else {
                this.Connect();
            }
        }

        private void OnDestroy() {
            this.usernameInputField.onValueChanged.RemoveAllListeners();
            this.roomIdInputField.onValueChanged.RemoveAllListeners();
        }

        public void SetUsername(string value) {
            this.username = value;
            PhotonNetwork.NickName = this.username;
        }

        public void Connect() {
            PhotonNetwork.ConnectUsingSettings();
        }

        public void SetRoomId(string roomId) {
            this.roomId = roomId;
        }

        public void CreateRoom() {
            if (PhotonNetwork.NickName == "") {
                Debug.Log("Nickname need to be filled");
                return;
            }

            RoomOptions roomOptions = new RoomOptions() {MaxPlayers = this.maxPlayers, IsVisible = true, IsOpen = true};

            // Todo check roomId uniq
            PhotonNetwork.CreateRoom(RoomUtils.CreateRoomId(), roomOptions, TypedLobby.Default);
        }

        public void JoinRandomRoom() {
            if (PhotonNetwork.NickName == "") {
                Debug.Log("Nickname need to be filled");
                return;
            }

            PhotonNetwork.JoinRandomRoom();
        }

        public void JoinRoom() {
            if (PhotonNetwork.NickName == "" || this.roomId == "") {
                Debug.Log("Nickname need to be filled and Room ID");
                return;
            }

            PhotonNetwork.JoinRoom(this.roomId);
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        public void StartGame() {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            List<int> randomNumbers = RoomUtils.FillListAndShuffle(this.maxPlayers);

            int counter = 0;
            foreach (KeyValuePair<int, Player> entry in PhotonNetwork.CurrentRoom.Players) {
                Hashtable playerProperties = new Hashtable();
                playerProperties["spawnId"] = randomNumbers[counter];
                playerProperties["skinId"] = randomNumbers[counter];
                entry.Value.SetCustomProperties(playerProperties);
                counter++;
            }

            StartCoroutine(this.LoadLevelForAll());
        }

        private IEnumerator LoadLevelForAll() {
            Debug.Log("Start in 3s");
            yield return new WaitForSeconds(1);

            Debug.Log("Start in 2s");
            yield return new WaitForSeconds(1);

            Debug.Log("Start in 1s");
            yield return new WaitForSeconds(1);

            PhotonNetwork.LoadLevel("Game");
        }

        private void DisplayAuthentificationPanel() {
            this.authentificationPanel.SetActive(true);
            this.roomPanel.SetActive(false);
        }

        private void DisplayRoomPanel() {
            this.authentificationPanel.SetActive(false);

            this.roomIdText.text = PhotonNetwork.CurrentRoom.Name;
            this.startButton.interactable = PhotonNetwork.IsMasterClient;

            // Clear UI
            foreach (KeyValuePair<int, LobbyPlayer> keyValuePair in this.lobbyPlayers) {
                Destroy(keyValuePair.Value.gameObject);
            }

            this.lobbyPlayers.Clear();

            // Fill players in UI
            foreach (KeyValuePair<int, Player> entry in PhotonNetwork.CurrentRoom.Players) {
                this.CreateLobbyPlayer(entry.Value);
            }

            this.roomPanel.SetActive(true);
        }

        private void CreateLobbyPlayer(Player photonPlayer) {
            LobbyPlayer player = Instantiate(this.lobbyPlayerPrefab, this.scrollviewContent.transform);
            player.Setup(photonPlayer.NickName, photonPlayer.IsMasterClient);
            this.lobbyPlayers.Add(photonPlayer.ActorNumber, player);
        }

        #region ConnectionCallbacks

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
            Debug.Log("Properties changed");
        }

        public override void OnConnectedToMaster() {
            Debug.Log("I'm now connect to master server");
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby() {
            Debug.Log("Lobby joined");
        }

        public override void OnCreatedRoom() {
            Debug.Log("Room created with id : " + PhotonNetwork.CurrentRoom.Name);
        }

        public override void OnCreateRoomFailed(short returnCode, string message) {
            Debug.LogErrorFormat("An error occured during room creation, code : {0}, message : {1}", returnCode, message);
        }

        public override void OnJoinRandomFailed(short returnCode, string message) {
            Debug.LogErrorFormat("Join random room failed cause {0}", message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message) {
            Debug.LogErrorFormat("Join room with id {0} failed cause {1}", this.roomId, message);
        }

        public override void OnJoinedRoom() {
            Debug.Log("Room joined with id : " + PhotonNetwork.CurrentRoom.Name);

            this.DisplayRoomPanel();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer) {
            Debug.LogFormat("{0} joined room !", newPlayer.NickName);

            this.CreateLobbyPlayer(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer) {
            Debug.LogFormat("{0} left room !", otherPlayer.NickName);

            Destroy(this.lobbyPlayers[otherPlayer.ActorNumber].gameObject);
            this.lobbyPlayers.Remove(otherPlayer.ActorNumber);
        }

        public override void OnLeftRoom() {
            Debug.Log("I left the room");
            this.DisplayAuthentificationPanel();
            StopAllCoroutines();
        }

        public override void OnMasterClientSwitched(Player newMasterClient) {
            this.lobbyPlayers[newMasterClient.ActorNumber].Setup(newMasterClient.NickName, true);
        }

        #endregion
    }
}