using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// ========================================================================
/// <summary>
/// Class to manage the list of players in the lobby
/// </summary>
public class LobbyPanelManager : MonoBehaviour {

    // ------------------------------------------------------
    // STATIC VARIABLES
    public static LobbyPanelManager singleton = null;

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text minPlayersLabel;
    public Dropdown minPlayersDropdown;
    public Toggle autoJoinToggle;
    public Button startGameButton;
    public RectTransform playerListContentTransform;
    public GameObject addPlayerRow;

    // ------------------------------------------------------
    // INSTANCE VARIABLES
    protected VerticalLayoutGroup layoutGroup;
    protected List<SingedLobbyPlayer> players = new List<SingedLobbyPlayer>();

    public void OnEnable() {
        var lobbyManager = SingedLobbyManager.s_singleton;
        singleton = this;
        layoutGroup = playerListContentTransform.GetComponent<VerticalLayoutGroup>();

        // update lobby manager w/ starting data
        var index = minPlayersDropdown.value;
        lobbyManager.minPlayers = int.Parse(minPlayersDropdown.options[index].text);

        // disable minPlayers, autoJoin, and startGame if not on server
        minPlayersLabel.gameObject.SetActive(lobbyManager.isHosting);
        minPlayersDropdown.gameObject.SetActive(lobbyManager.isHosting);
        autoJoinToggle.gameObject.SetActive(lobbyManager.isHosting);
        startGameButton.gameObject.SetActive(lobbyManager.isHosting);
        startGameButton.interactable = false;
    }

    public void OnMinPlayersValueChanged() {
        var lobbyManager = SingedLobbyManager.s_singleton;
        var index = minPlayersDropdown.value;
        // lobby manager minPlayers determines the minimal number of players before a game can start
        lobbyManager.minPlayers = int.Parse(minPlayersDropdown.options[index].text);

        // recheck if we are ready to begin
        lobbyManager.CheckReadyToBegin();
    }

    public void OnAutoStartValueChanged() {
        var lobbyManager = SingedLobbyManager.s_singleton;
        lobbyManager.doAutoStart = autoJoinToggle.isOn;
        if (lobbyManager.doAutoStart) {
            lobbyManager.CheckReadyToBegin();
        }
    }

    public void OnClickStartGame() {
        var lobbyManager = SingedLobbyManager.s_singleton;
        lobbyManager.StartGame();
    }

    void Update() {
        // FIXME: see if this can be disabled
        //this dirty the layout to force it to recompute evryframe (a sync problem between client/server
        //sometime to child being assigned before layout was enabled/init, leading to broken layouting)

        if (layoutGroup != null) {
            layoutGroup.childAlignment = Time.frameCount%2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
        }
    }

    public void AddPlayer(SingedLobbyPlayer player) {
        if (players.Contains(player)) return;

        // add player
        players.Add(player);

        // update parent and adjust add button row
        player.transform.SetParent(playerListContentTransform, false);
        addPlayerRow.transform.SetAsLastSibling();

        // notify players of list modification
        PlayerListModified();
    }

    public void RemovePlayer(SingedLobbyPlayer player) {
        players.Remove(player);
        PlayerListModified();
    }

    public void PlayerListModified() {
        int i = 0;
        foreach (SingedLobbyPlayer p in players) {
            p.OnPlayerListChanged(i);
            ++i;
        }
    }
}
