using UnityEngine;
using Photon.Pun;

public class WaitForPlayerController : MonoBehaviour
{
    public static WaitForPlayerController instance;

    public MenuManager menuManager;
    private PhotonView photonView;

    public static bool readyToCount = false;
    public static bool startGame = false;
    public static float timerToStartGame = StaticDataManager.gameStartTimer;
    public static float notFullGameTimer = StaticDataManager.gameStartTimer;

    private void Awake() => instance = this;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        WaitingForMorePlayers();
    }

    private void WaitingForMorePlayers()
    {
        if (StaticDataManager.currentPlayersInRoomCount < StaticDataManager.minPlayerToStartGame)
            ResetTimer();

        if (readyToCount)
        {
            notFullGameTimer -= Time.deltaTime;
            timerToStartGame = notFullGameTimer;
        }

        string tempTimer = "Match Starting in " + (int)timerToStartGame + " sec.";
        menuManager.seeyaWorldwideCountDownTimer.text = tempTimer;

        if (timerToStartGame <= 0f)
        {
            if (startGame)
                return;
            StartingGame();
        }
    }

    private void ResetTimer()
    {
        notFullGameTimer = StaticDataManager.gameStartTimer;
        timerToStartGame = StaticDataManager.gameStartTimer;
    }

    private void StartingGame()
    {
        ServerController.instance.StartGame();
    }
}