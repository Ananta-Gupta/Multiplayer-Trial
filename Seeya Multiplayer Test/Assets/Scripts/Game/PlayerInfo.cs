using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerInfo : MonoBehaviour
{
    public Text playerName;
    public Text bankScroeText;
    public Text seeyaText;
    public Text currentScoreText;

    [HideInInspector] public int bankedScore;
    [HideInInspector] public int diceAValue, diceBValue;
    [HideInInspector] public int currentScore;
    [HideInInspector] public bool isWinning = false;
}