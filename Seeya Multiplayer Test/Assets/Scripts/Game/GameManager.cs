using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public RectTransform playerContainer;
    public GameObject playerPrefab;

    public Text diceA, diceB;
    public GameObject rollBtn;
    public GameObject bankBtn;

    [Header("Footer")]
    public Text potScore;
    public Text rankText;
    public Text rankScoreText;

    [Header("Winning Panel")]
    public GameObject winningPanel;
    public Button backBtn;
    public Text winningPlayerNameText;
    public Text winningPlayerScore;

    [SerializeField] private PlayerInfo currentPlayer;

    private bool rolling = false;
    private float time = 2f;

    public GameObject rayCastBlockPanel;

    private void Start()
    {
        winningPanel.SetActive(false);
        CreatePlayerList();
    }

    private void Update()
    {
        if (rolling)
        {
            rollBtn.GetComponent<Button>().interactable = false;
            bankBtn.GetComponent<Button>().interactable = false;

            time -= Time.deltaTime;
            StaticDataManager.diceAValue = Random.Range(1, 7);
            StaticDataManager.diceBValue = Random.Range(1, 7);

            diceA.text = StaticDataManager.diceAValue.ToString();
            diceB.text = StaticDataManager.diceBValue.ToString();

            object[] sendData = new object[] { StaticDataManager.diceAValue, StaticDataManager.diceBValue };
            ServerController.instance.PhotonRaiseEventsSender_Other(StaticDataManager.DiceRollingSynk, sendData, true);

            if (time <= 0f)
            {
                rolling = false;
                rollBtn.GetComponent<Button>().interactable = true;
                StaticDataManager.totalDiceValue = StaticDataManager.diceAValue + StaticDataManager.diceBValue;
                CheckScore();
            }
        }
    }

    private void CreatePlayerList()
    {
        for (int i = 0; i < playerContainer.childCount; i++)
            StaticDataManager.playersList.Add(playerContainer.GetChild(i).GetComponent<PlayerInfo>());

        if (PhotonNetwork.IsMasterClient)
            FirstPlayerSetup();

        currentPlayer = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer];
    }

    private void FirstPlayerSetup()
    {
        StaticDataManager.currentPlayingPlayer = 0;
        currentPlayer = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer];
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].seeyaText.text = "";
        bankBtn.GetComponent<Button>().interactable = false;
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankScroeText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankedScore.ToString();

        object[] sendData = new object[] { PhotonNetwork.LocalPlayer.ActorNumber };
        ServerController.instance.PhotonRaiseEventsSender_All(StaticDataManager.OtherPlayerBlocker, sendData, true);
    }

    public void OnClickRollBtn()
    {
        //Debug.Log("OnClickRollBtn");
        time = 2f;
        rolling = true;
    }

    private void CheckScore()
    {
        rollBtn.GetComponent<Button>().interactable = false;
        bankBtn.GetComponent<Button>().interactable = false;

        if (StaticDataManager.diceAValue == 1 || StaticDataManager.diceBValue == 1)
            GetSingleSeeya();
        else if (StaticDataManager.diceAValue == 1 && StaticDataManager.diceBValue == 1)
            GetDoubleSeeya();
        else
            GetScore();
    }

    #region CheckScore
    private void GetSingleSeeya()
    {
        if (StaticDataManager.diceAValue == 1)
            diceA.transform.parent.GetComponent<Image>().color = Color.red;
        else if (StaticDataManager.diceBValue == 1)
            diceB.transform.parent.GetComponent<Image>().color = Color.red;

        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].seeyaText.text = "Single Seeya";
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankedScore;
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScoreText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankedScore.ToString();
        rankScoreText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore.ToString();

        object[] sendData = new object[] {
            StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore,
            rankScoreText.text,
        };
        ServerController.instance.PhotonRaiseEventsSender_Other(StaticDataManager.CurrentScroeSynk, sendData, true);
        NextPlayer();
    }

    private void GetDoubleSeeya()
    {
        diceA.transform.parent.GetComponent<Image>().color = Color.red;
        diceB.transform.parent.GetComponent<Image>().color = Color.red;

        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].seeyaText.text = "Double Seeya";
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore = 0;
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankedScore = 0;
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScoreText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankedScore.ToString();
        rankScoreText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore.ToString();
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankScroeText.text = 0.ToString();

        currentPlayer.isWinning = false;

        object[] sendData = new object[] {
            StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore,
            rankScoreText.text,
        };
        ServerController.instance.PhotonRaiseEventsSender_Other(StaticDataManager.CurrentScroeSynk, sendData, true);
        NextPlayer();
    }

    private void GetScore()
    {
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].seeyaText.text = "";
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore += StaticDataManager.totalDiceValue;
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScoreText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore.ToString();
        rankScoreText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore.ToString();
        rollBtn.GetComponent<Button>().interactable = true;
        bankBtn.GetComponent<Button>().interactable = true;

        object[] sendData = new object[] {
            StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore,
            rankScoreText.text,
        };
        ServerController.instance.PhotonRaiseEventsSender_Other(StaticDataManager.CurrentScroeSynk, sendData, true);
    }
    #endregion

    #region Change Player
    private int localPlayerActorNo;

    private void NextPlayer()
    {
        for (int i = 0; i < StaticDataManager.playersIDList.Count; i++)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == StaticDataManager.playersIDList[i])
            {
                if (i == StaticDataManager.playersIDList.Count - 1)
                    localPlayerActorNo = 0;
                else
                    localPlayerActorNo = i + 1;
            }
        }

        Debug.Log(StaticDataManager.playersIDList[localPlayerActorNo] + "," + localPlayerActorNo);
        object[] sendData = new object[] { StaticDataManager.playersIDList[localPlayerActorNo] };
        ServerController.instance.PhotonRaiseEventsSender_All(StaticDataManager.OtherPlayerBlocker, sendData, true);

        ServerController.instance.PhotonRaiseEventsSender_All(StaticDataManager.ChangePlayer, null, true);
    }

    public void ChangePlayerCall()
    {
        //Invoke(nameof(ChangePlayer), 2f);

        ChangePlayer();
    }

    int newX = 0;
    private void ChangePlayer()
    {
        if (StaticDataManager.currentPlayingPlayer == (StaticDataManager.playersList.Count - 1))
        {
            playerContainer.anchoredPosition = new Vector2(0, 0);
            newX = 0;
            StaticDataManager.currentPlayingPlayer = 0;
        }
        else
        {
            newX -= 400;
            playerContainer.anchoredPosition = new Vector3(newX, 0, 0);
            StaticDataManager.currentPlayingPlayer++;
        }

        rollBtn.GetComponent<Button>().interactable = true;
        bankBtn.GetComponent<Button>().interactable = false;

        diceA.transform.parent.GetComponent<Image>().color = Color.white;
        diceB.transform.parent.GetComponent<Image>().color = Color.white;

        currentPlayer = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer];
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].seeyaText.text = "";

        rankScoreText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore.ToString();
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankScroeText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankedScore.ToString();
    }
    #endregion

    public void OnClickBankBtn()
    {
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankedScore = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore;
        StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankScroeText.text = StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].bankedScore.ToString();

        CheckForWinner();

        NextPlayer();
    }

    private void CheckForWinner()
    {
        if (StaticDataManager.playersList[StaticDataManager.currentPlayingPlayer].currentScore >= 10)
        {
            currentPlayer.isWinning = true;
            object[] sendData = new object[] { currentPlayer.playerName.text, currentPlayer.bankedScore };
            ServerController.instance.PhotonRaiseEventsSender_All(StaticDataManager.PlayerWin, sendData, true);
        }
    }
    
    public void BackBtnCall()
    {
        StaticDataManager.isGameLoaded = false;
        ServerController.instance.LeaveRoomCall();
        SceneManager.LoadScene(StaticDataManager.menuSceneIndex);
    }
}

//asynk scene