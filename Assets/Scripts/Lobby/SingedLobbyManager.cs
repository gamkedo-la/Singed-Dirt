using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Class representing the lobby overal lobby manager, derived from NetworkLobbyManager
/// </summary>
public class SingedLobbyManager : NetworkLobbyManager {
    // ------------------------------------------------------
    // STATIC VARIABLES
    public static SingedLobbyManager s_singleton;
    static short MsgKicked = MsgType.Highest + 1;

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public LobbyStatus statusPanel;
    public LobbyGameSelect gameSelectPanel;
    public LobbyPanelManager lobbyPanel;
    public LobbyInfoPanelController infoPanel;
    public TankModelPanel playerSetupPanel;

    GameObject currentPanel;

    //Client numPlayers from NetworkManager is always 0, so we count (throught connect/destroy in LobbyPlayer) the number
    //of players, so that even client know how many player there is.
    [HideInInspector]
    public int playerCount = 0;

    void Awake() {
        s_singleton = this;
    }

    void Start() {
        // mark lobby as persistent object between scenes
        DontDestroyOnLoad(gameObject);

        // set initial status
        statusPanel.SetStatus("Offline", gameSelectPanel.host, gameSelectPanel.port);
        // disable back button
        statusPanel.SetBackEnabled(false, null);

        // set initial panel to main game select
        ChangeTo(gameSelectPanel.gameObject, null);
    }

    /// <summary>
    /// Change the current view to selected panel
    /// </summary>
    public void ChangeTo(
        GameObject newPanel,
        UnityEngine.Events.UnityAction backCallback
    ) {
        Debug.Log("ChangeTo");
        if (currentPanel != null) {
            currentPanel.SetActive(false);
        }

        if (newPanel != null) {
            newPanel.SetActive(true);
        }

        currentPanel = newPanel;

        if (currentPanel != gameSelectPanel.gameObject) {
            Debug.Log("setting back enabled true");
            statusPanel.SetBackEnabled(true, backCallback);
        } else {
            statusPanel.SetBackEnabled(false, null);
            statusPanel.SetStatus("Offline", gameSelectPanel.host, gameSelectPanel.port);
            //_isMatchmaking = false;
        }
    }

    public void AddLocalPlayer() {
        // attempt to set new local player
        // Debug.Log("AddLocalPlayer");
        TryToAddPlayer();
    }

    // ------------------------------------------------------
    // EVENT HANDLER METHODS

    public override void OnLobbyServerDisconnect(
        NetworkConnection    connection
    ) {
        // Debug.Log("OnLobbyServerDisconnect");
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(
        GameObject lobbyPlayer,
        GameObject gamePlayer
    ) {
        // Debug.Log("OnLobbyServerSceneLoadedForPlayer");

        var playerSetupHook = GetComponent<TankModelHook>();
        if (playerSetupHook != null) {
            playerSetupHook.SetupPlayer(this, lobbyPlayer, gamePlayer);
        }
        return true;
    }

    public override void OnLobbyClientSceneChanged(
        NetworkConnection connection
    ) {

        // scene changed back to lobby
        if (SceneManager.GetSceneAt(0).name == lobbyScene) {
            // Debug.Log("changing to lobby scene");
            ChangeTo(gameSelectPanel.gameObject, null);
            statusPanel.ToggleVisibility(true);
            statusPanel.isInGame = false;
            /*
            if (statusPanel.isInGame) {
                if (_isMatchmaking) {
                    if (connection.playerControllers[0].unetView.isServer) {
                        backDelegate = StopHostClbk;
                    } else {
                        backDelegate = StopClientClbk;
                    }
                } else {
                    if (connection.playerControllers[0].unetView.isClient) {
                        backDelegate = StopHostClbk;
                    } else {
                        backDelegate = StopClientClbk;
                    }
                }
            } else {
                ChangeTo(mainMenuPanel);
            }
            */

        // otherwise ... game is starting
        } else {
            Debug.Log("changing to game scene");
            // disable lower panels, set back callback to stop game
            ChangeTo(null, () => { StopGameCallback(); });

            // hide status panel
            statusPanel.ToggleVisibility(false);
            statusPanel.isInGame = true;
        }
    }

    /// <summary>
    /// Update player count
    /// </summary>
    public void OnPlayersNumberModified(
        int count
    ) {
        Debug.Log("OnPlayersNumberModified");
        playerCount += count;

        // count number of local players in scene
        int localPlayerCount = 0;
        foreach (var player in ClientScene.localPlayers) {
            localPlayerCount += (player == null || player.playerControllerId == -1) ? 0 : 1;
        }

        // enable/disable lobby's add player button based on max number of players and max # of local players
        lobbyPanel.addPlayerRow.SetActive(localPlayerCount < maxPlayersPerConnection && playerCount < maxPlayers);
    }

    public override void OnStartHost() {
        base.OnStartHost();

        // change to lobby view
        ChangeTo(lobbyPanel.gameObject, () => { StopHostCallback(); });

        // update main status
        statusPanel.SetStatus("Hosting", gameSelectPanel.host, gameSelectPanel.port);
    }

    // ------------------------------------------------------
    // CALLBACK METHODS

    public void StopClientCallback() {
        // stop the client session
        StopClient();

        /*
        // FIXME: matchmaking not supported yet
        if (_isMatchmaking) {
            StopMatchMaker();
        }
        */

        // switch view back to main game select panel
        ChangeTo(gameSelectPanel.gameObject, null);
    }

    public void OnDestroy() {
        Debug.Log("OnDestroy: " + this);
    }

    public void StopHostCallback() {
        Debug.Log("StopHostCallback");
        //if (_isMatchmaking) {
			//matchMaker.DestroyMatch((NetworkID)_currentMatchID, 0, OnDestroyMatch);
			//_disconnectServer = true;
        //} else {
            StopHost();
        //}
        // switch view back to main game select panel
        ChangeTo(gameSelectPanel.gameObject, null);
        //Debug.Log("trying to destroy self: " + this);
        //Destroy(gameObject);
    }

    public void StopGameCallback() {
        Debug.Log("StopGameCallback");
        // FIXME:
        StopHost();
        StopClient();
        // switch view back to main game select panel
        ChangeTo(gameSelectPanel.gameObject, null);
    }

    class KickMsg : MessageBase { }

    public void KickPlayer(NetworkConnection connection) {
        connection.Send(MsgKicked, new KickMsg());
    }

    // ----------------- Client callbacks ------------------

    /// <summary>
    /// Client event handler, called when client successfully connects to server
    /// </summary>
    public override void OnClientConnect(
        NetworkConnection connection
    ) {
        Debug.Log("OnClientConnect");
        base.OnClientConnect(connection);

        // disable info panel
        infoPanel.gameObject.SetActive(false);

        // setup kicked message handler
        connection.RegisterHandler(MsgKicked, KickedMessageHandler);

        // for network client, activate lobby panel, update status
        if (!NetworkServer.active) {
            ChangeTo(lobbyPanel.gameObject, () => { StopClientCallback(); });
            statusPanel.SetStatus("Client", gameSelectPanel.host, gameSelectPanel.port);
        }
    }

    public override void OnClientDisconnect(
        NetworkConnection connection
    ) {
        //base.OnClientDisconnect(connection);
        ChangeTo(gameSelectPanel.gameObject, null);
        infoPanel.Display(
            "Client connection failed",
            "Close",
            null
        );
        StopClient();
    }

    public override void OnClientError(
        NetworkConnection connection,
        int errorCode
    ) {
        ChangeTo(gameSelectPanel.gameObject, null);

        // popup warning
        infoPanel.Display(
            "Cient error : " + (errorCode == 6 ? "timeout" : errorCode.ToString()),
            "Close",
            null
        );

        StopClient();
    }

    public void KickedMessageHandler(
        NetworkMessage netMsg
    ) {
        infoPanel.Display(
            "Kicked by Server",
            "Close",
            null
        );
        netMsg.conn.Disconnect();
    }

}
