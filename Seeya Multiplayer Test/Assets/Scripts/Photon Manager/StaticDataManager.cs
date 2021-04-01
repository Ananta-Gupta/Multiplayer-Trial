using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDataManager : MonoBehaviour
{
    public static StaticDataManager instance;

    [Header("Scene Indexes")]
    public const int menuSceneIndex = 0;
    public const int gameSceneIndex = 1;
    
    [Header("Game Version")]
    public static string GAMEVERSION = "0.1";

    [Header("Raise Events Code")]
    public const byte GameStartTimerSynk = 1;
    public const byte SetPlayerActorNumber = 2;
    public const byte OtherPlayerBlocker = 3;

    //In Game Codes
    public const byte DiceRollingSynk = 11;
    public const byte CurrentScroeSynk = 12;
    public const byte OverAllScoreSynk = 13;
    public const byte ChangePlayer = 14;
    public const byte PlayerWin = 15;

    public static string playerUserId;
    public static List<PlayerInfo> playersList = new List<PlayerInfo>();
    public static Dictionary<string, int> playerIdInfo = new Dictionary<string, int>();

    [Header("Room Controll Variables")]
    public static int     maxPlayerRoomCanHold = 20;
    public static int     minPlayerToStartGame = 2;
    public static string  randomRooomJoinedName;
    public static int     currentPlayersInRoomCount;
    public static int     maxPlayersInRoomCount;
    public static float   gameStartTimer = 5f; //30

    [Header("Game Controll Variables")]
    public static int currentPlayingPlayer = 0;
    public static int diceAValue;
    public static int diceBValue;
    public static int totalDiceValue;
    public static bool isGameLoaded = false;

    private void Awake() => instance = this;
}