using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using System.Collections;

public class LobbyMatchEntry : MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text serverNameText;
    public Text slotInfoText;
    public Button joinButton;

	public void Populate(
        MatchInfoSnapshot match,
        Color c
    ) {
        // fill out info in row
        serverNameText.text = match.name;
        slotInfoText.text = match.currentSize.ToString() + "/" + match.maxSize.ToString(); ;

        // set join button action
        var networkID = match.networkId;
        var lobbyManager = SingedLobbyManager.s_singleton;
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => { lobbyManager.JoinMatch(networkID); });

        GetComponent<Image>().color = c;
    }

}
