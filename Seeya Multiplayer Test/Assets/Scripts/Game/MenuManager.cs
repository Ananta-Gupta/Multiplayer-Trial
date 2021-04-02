using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menuPanel;
    public InputField playerNameInputField;
    public Button seeyaWorldwideBtn;
    public Transform playerListingContainer;
    public GameObject playerListingPrefab;

    [Header("Seeya Worldwide")]
    public GameObject seeyaWorldwidePanel;
    public Text playerCounterText;
    public Text seeyaWorldwideCountDownTimer;

    [Header("Seeya With Friend")]
    public GameObject seeyaWithFriendPanel;

    public void OnClick_SeeyaWorldwide()
    {
        //Debug.Log("Seeya Worldwide Btn Pressed.");
        ServerController.instance.JoinRoomCall();
    }

    public void OnClick_SeeyaWorldwide_BackBtn()
    {
        Debug.Log("Seeya Worldwide Back Btn Pressed.");
        ServerController.instance.LeaveRoomCall();
    }

    public void PlayerChangedName(string name)
    {
        ServerController.instance.PlayerNameUpdate(name);
    }

    public void OnClick_SeeyaWithFriend()
    {
        seeyaWithFriendPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void OnClick_JoinGame(InputField inputField)
    {
        ServerController.instance.JoinRoomCall(inputField.text);
    }
}