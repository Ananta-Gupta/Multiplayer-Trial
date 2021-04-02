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
        if (!StaticDataManager.isGameLoaded && SceneManager.GetActiveScene().buildIndex == StaticDataManager.menuSceneIndex)
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

        /*Debug.Log("Player List :-");
        foreach (var item in StaticDataManager.playersNameList)
            Debug.Log(item);*/
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
        Debug.Log("Lobby joined successfully.");
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
        StaticDataManager.playerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
    }

    public override void OnLeftRoom()
    {
        Debug.Log("You left the room : " + StaticDataManager.randomRooomJoinedName);
        FindObjectOfType<MenuManager>().menuPanel.SetActive(true);
        FindObjectOfType<MenuManager>().seeyaWorldwidePanel.SetActive(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " has joined the room, Actor Number : " + newPlayer.ActorNumber);

        PlayerCounterUpdate();

        object[] sendData = new object[] { WaitForPlayerController.timerToStartGame };
        PhotonRaiseEventsSender_Other(StaticDataManager.GameStartTimerSynk, sendData, true);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " has left the room.");

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("here1.");
            if (StaticDataManager.isGameLoaded)
            {
                Debug.Log("here2.");
                object[] sendData = new object[] { otherPlayer.ActorNumber };
                PhotonRaiseEventsSender_All(StaticDataManager.MeLeftInRunningGame, sendData, true);
            }
        }

        PlayerCounterUpdate();
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
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            StaticDataManager.playersIDList.Add(PhotonNetwork.PlayerList[i].ActorNumber);

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
        PhotonNetwork.RaiseEvent(code, data, reops, SendOptions.SendUnreliable);
    }
    public void PhotonRaiseEventsSender_MasterClient(byte code, object[] data, bool tf)
    {
        RaiseEventOptions reops = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(code, data, reops, SendOptions.SendReliable);
    }
    public void PhotonRaiseEventsSender_Other(byte code, object[] data, bool tf)
    {
        RaiseEventOptions reops1 = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(code, data, reops1, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        //Debug.LogError("Event Code" + photonEvent.Code);
        object[] dataReceiver = (object[])photonEvent.CustomData;

        switch (photonEvent.Code)
        {
            case StaticDataManager.GameStartTimerSynk:
                WaitForPlayerController.timerToStartGame = (float)dataReceiver[0];
                WaitForPlayerController.notFullGameTimer = (float)dataReceiver[0];
                break;

            case StaticDataManager.OtherPlayerBlocker:
                if ((int)dataReceiver[0] == PhotonNetwork.LocalPlayer.ActorNumber)
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

            case StaticDataManager.GameEndState:
                StaticDataManager.isGameLoaded = false;
                break;

            //In Game Code Cases
            case StaticDataManager.DiceRollingSynk:
                FindObjectOfType<GameManager>().diceA.text = dataReceiver[0].ToString();
                FindObjectOfType<GameManager>().diceB.text = dataReceiver[1].ToString();
                break;

            case StaticDataManager.CurrentScroeSynk:
                StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScoreText.text = dataReceiver[0].ToString();
                FindObjectOfType<GameManager>().rankScoreText.text = dataReceiver[1].ToString();
                break;
            case StaticDataManager.OverAllScoreSynk:
                break;

            case StaticDataManager.ChangePlayer:
                FindObjectOfType<GameManager>().ChangePlayerCall();
                break;

            case StaticDataManager.PlayerWin:
                PhotonRaiseEventsSender_All(StaticDataManager.GameEndState, null, true);

                FindObjectOfType<GameManager>().winningPanel.SetActive(true);
                FindObjectOfType<GameManager>().winningPlayerNameText.text = dataReceiver[0].ToString() + " Win.";
                FindObjectOfType<GameManager>().winningPlayerScore.text = "Score : " + dataReceiver[1].ToString();
                break;

            //Game Status
            case StaticDataManager.MeLeftInRunningGame:
                StaticDataManager.playersIDList.Remove((int)dataReceiver[0]);
                for (int i = 0; i < FindObjectOfType<GameManager>().playerContainer.childCount; i++)
                {
                    if (FindObjectOfType<GameManager>().playerContainer.GetChild(i).GetChild(0).GetComponent<Text>().text == dataReceiver[0].ToString())
                        Destroy(FindObjectOfType<GameManager>().playerContainer.GetChild(i).gameObject);
                }

                Debug.Log("Player Count now : " + PhotonNetwork.PlayerList.Length);
                if (PhotonNetwork.PlayerList.Length == 1)
                {
                    FindObjectOfType<GameManager>().playerContainer.GetChild(0).GetComponent<PlayerInfo>().isWinning = true;
                    object[] sendData = new object[] {  FindObjectOfType<GameManager>().playerContainer.GetChild(0).GetComponent<PlayerInfo>().playerName.text,
                                                        FindObjectOfType<GameManager>().playerContainer.GetChild(0).GetComponent<PlayerInfo>().bankedScore };
                    ServerController.instance.PhotonRaiseEventsSender_All(StaticDataManager.PlayerWin, sendData, true);
                }
                break;
        }
    }
    #endregion
}