using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class ServerController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region Instance
    public static ServerController instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            if (instance != this)
                Destroy(gameObject);
            else
                instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Master Settigns
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnFinishLoding;
    }

    private void Start()
    {

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnFinishLoding;
    }
    #endregion

    #region Custom Methods
    public void JoinRoomCall()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void JoinRoomCall(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void LeaveRoomCall()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        StartCoroutine(RejoinLobby());
    }

    private IEnumerator RejoinLobby()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinLobby();
    }

    public void PlayerNameUpdate(string name)
    {
        PhotonNetwork.NickName = name;
        PlayerPrefs.SetString("NickName", name);
    }

    private void PlayerCounterUpdate()
    {
        if (!StaticDataManager.isGameLoaded)
        {
            StaticDataManager.currentPlayersInRoomCount = PhotonNetwork.PlayerList.Length;
            StaticDataManager.maxPlayersInRoomCount = PhotonNetwork.CurrentRoom.MaxPlayers;
            FindObjectOfType<MenuManager>().playerCounterText.text = "Player Count : " + StaticDataManager.currentPlayersInRoomCount + "/" + StaticDataManager.maxPlayersInRoomCount;

            if (StaticDataManager.currentPlayersInRoomCount >= StaticDataManager.minPlayerToStartGame)
                WaitForPlayerController.readyToCount = true;
            else
                WaitForPlayerController.readyToCount = false;

            ClearPlayerList();
        }
    }

    private void ClearPlayerList()
    {
        for (int i = FindObjectOfType<MenuManager>().playerListingContainer.childCount - 1; i >= 0; i--)
            Destroy(FindObjectOfType<MenuManager>().playerListingContainer.GetChild(i).gameObject);

        ListPlayer();
    }

    private void ListPlayer()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject tempListing = Instantiate(FindObjectOfType<MenuManager>().playerListingPrefab, FindObjectOfType<MenuManager>().playerListingContainer);
            Text tempText = tempListing.transform.GetChild(0).GetComponent<Text>();
            tempText.text = player.NickName;
        }
    }

    public void StartGame()
    {
        WaitForPlayerController.startGame = true;
        if (!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(StaticDataManager.gameSceneIndex);
    }

    private void PlayersInstantiate()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject tempListing = Instantiate(FindObjectOfType<GameManager>().playerPrefab, FindObjectOfType<GameManager>().playerContainer);
            Text tempText = tempListing.transform.GetChild(0).GetComponent<Text>();
            tempText.text = player.NickName;
        }

        foreach (var item in StaticDataManager.playerIdInfo)
            Debug.Log(item.Key + "," + item.Value);
    }
    #endregion

    #region Creat/Join Room
    private void CreateRandomRoom()
    {
        Debug.Log("Creating new random room");
        int randRoomName = Random.Range(1000, 10000);
        RoomOptions roomOps = new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = (byte)StaticDataManager.maxPlayerRoomCanHold };
        roomOps.PublishUserId = true;
        PhotonNetwork.CreateRoom(randRoomName.ToString(), roomOps);
    }
    #endregion

    #region Photon Seccess Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connect to master");
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.JoinLobby();

        if (PlayerPrefs.HasKey("NickName"))
        {
            if (PlayerPrefs.GetString("NickName") == "")
                PhotonNetwork.NickName = "Player" + Random.Range(0, 10000);
            else
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
        }
        else
            PhotonNetwork.NickName = "Player" + Random.Range(0, 10000);

        FindObjectOfType<MenuManager>().playerNameInputField.text = PhotonNetwork.NickName;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        Debug.Log("Disconnected : " + cause.ToString());
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Jobby joined successfully.");
    }

    public override void OnLeftLobby()
    {
        Debug.Log("You left the lobby");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created");
    }

    public override void OnJoinedRoom()
    {
        StaticDataManager.randomRooomJoinedName = PhotonNetwork.CurrentRoom.Name;
        Debug.Log("Room : " + StaticDataManager.randomRooomJoinedName + " joined.");
        FindObjectOfType<MenuManager>().seeyaWorldwidePanel.SetActive(true);
        FindObjectOfType<MenuManager>().seeyaWithFriendPanel.SetActive(false);
        FindObjectOfType<MenuManager>().menuPanel.SetActive(false);

        PlayerCounterUpdate();
        StaticDataManager.playerUserId = PhotonNetwork.LocalPlayer.UserId;
        //Debug.Log("Player User Id: " + StaticDataManager.playerUserId);

        if (!PhotonNetwork.IsMasterClient)
            return;
        StaticDataManager.playerIdInfo.Add(PhotonNetwork.LocalPlayer.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("You left the room : " + StaticDataManager.randomRooomJoinedName);
        FindObjectOfType<MenuManager>().menuPanel.SetActive(true);
        FindObjectOfType<MenuManager>().seeyaWorldwidePanel.SetActive(false);


        if (!PhotonNetwork.IsMasterClient)
            return;
        StaticDataManager.playerIdInfo.Remove(PhotonNetwork.LocalPlayer.NickName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " has joined the room, Actor Number : " + newPlayer.ActorNumber);

        PlayerCounterUpdate();

        if (!PhotonNetwork.IsMasterClient)
            return;

        StaticDataManager.playerIdInfo.Add(newPlayer.NickName, newPlayer.ActorNumber);
        object[] sendData = new object[] { WaitForPlayerController.timerToStartGame };
        PhotonRaiseEventsSender_Other(StaticDataManager.GameStartTimerSynk, sendData, true);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " has left the room.");

        PlayerCounterUpdate();

        if (!PhotonNetwork.IsMasterClient)
            return;

        StaticDataManager.playerIdInfo.Remove(otherPlayer.NickName);
    }
    #endregion

    #region Photon Failed Callbacks
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join random room.");
        CreateRandomRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room.");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to creat random room.");
        CreateRandomRoom();
    }
    #endregion

    #region Photon Custom Callbacks
    private void OnFinishLoding(Scene scene, LoadSceneMode mode)
    {
        int currentScene = scene.buildIndex;

        switch (currentScene)
        {
            case StaticDataManager.gameSceneIndex:
                StaticDataManager.isGameLoaded = true;
                PlayersInstantiate();
                break;
        }
    }
    #endregion

    #region Raise Events
    public void PhotonRaiseEventsSender_All(byte code, object[] data, bool tf)
    {
        RaiseEventOptions reops = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(code, data, reops, SendOptions.SendReliable);
    }
    public void PhotonRaiseEventsSender_MasterClient(byte code, object[] data, bool tf)
    {
        RaiseEventOptions reops = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(code, data, reops, SendOptions.SendReliable);
    }
    public void PhotonRaiseEventsSender_Other(byte code, object[] data, bool tf)
    {
        RaiseEventOptions reops = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(code, data, reops, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        object[] dataReceiver = (object[])photonEvent.CustomData;

        switch (photonEvent.Code)
        {
            case StaticDataManager.GameStartTimerSynk:
                WaitForPlayerController.timerToStartGame = (float)dataReceiver[0];
                WaitForPlayerController.notFullGameTimer = (float)dataReceiver[0];
                break;

            case StaticDataManager.DiceRollingSynk:
                FindObjectOfType<GameManager>().diceA.text = dataReceiver[0].ToString();
                FindObjectOfType<GameManager>().diceB.text = dataReceiver[1].ToString();
                break;

            case StaticDataManager.OtherPlayerBlocker:
                //Debug.Log("Data1 : " + dataReceiver[0] + " , Data 2 : " + PhotonNetwork.LocalPlayer.ActorNumber + "Time : " + Time.time);
                if (int.Parse(dataReceiver[0].ToString()) == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    FindObjectOfType<GameManager>().rollBtn.SetActive(true);
                    FindObjectOfType<GameManager>().bankBtn.SetActive(true);
                    FindObjectOfType<GameManager>().rayCastBlockPanel.SetActive(false);
                }
                else
                {
                    FindObjectOfType<GameManager>().rollBtn.SetActive(false);
                    FindObjectOfType<GameManager>().bankBtn.SetActive(false);
                    FindObjectOfType<GameManager>().rayCastBlockPanel.SetActive(true);
                }
                break;

            case StaticDataManager.CurrentScroeSynk:
                break;
            case StaticDataManager.OverAllScoreSynk:
                break;

            case StaticDataManager.ChangePlayer:
                FindObjectOfType<GameManager>().ChangePlayerCall();
                break;

            case StaticDataManager.PlayerWin:
                FindObjectOfType<GameManager>().winningPanel.SetActive(true);
                FindObjectOfType<GameManager>().winningPlayerNameText.text = dataReceiver[0].ToString() + "Win.";
                FindObjectOfType<GameManager>().winningPlayerScore.text = "Score : " + dataReceiver[1].ToString();
                break;
        }
    }
    #endregion
}