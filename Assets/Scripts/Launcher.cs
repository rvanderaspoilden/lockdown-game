using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Utils;

public class Launcher : MonoBehaviourPunCallbacks {
    [Header("Settings")]
    [SerializeField] private byte maxPlayers;
    
    [Header("UI fields")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField roomIdInputField;
    
    [Header("Only for debug")]
    [SerializeField] private string username;
    [SerializeField] private string roomId;

    // Start is called before the first frame update
    void Start()
    {
        this.usernameInputField.onValueChanged.AddListener((string value) => this.SetUsername(value));
        this.roomIdInputField.onValueChanged.AddListener((string value) => this.SetRoomId(value));
        PhotonNetwork.AutomaticallySyncScene = true;
        this.Connect();
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
        
        RoomOptions roomOptions = new RoomOptions() {
            MaxPlayers = this.maxPlayers,
            IsVisible = true,
            IsOpen = true
        };
        
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

    #region ConnectionCallbacks

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
    }

    #endregion
}
