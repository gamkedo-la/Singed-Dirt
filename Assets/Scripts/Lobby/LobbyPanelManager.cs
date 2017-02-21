using UnityEngine;
using UnityEngine.UI;
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
    public RectTransform playerListContentTransform;
    public GameObject addPlayerRow;

    // ------------------------------------------------------
    // INSTANCE VARIABLES
    protected VerticalLayoutGroup layoutGroup;
    protected List<SingedLobbyPlayer> players = new List<SingedLobbyPlayer>();

    public void OnEnable() {
        singleton = this;
        layoutGroup = playerListContentTransform.GetComponent<VerticalLayoutGroup>();
    }

    void Update() {
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
        Debug.Log("addPlayerRow: " + addPlayerRow);
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