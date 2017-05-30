using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections;

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
    public MatchmakerServerController matchmakerServerPanel;

    GameObject currentPanel;
    protected ulong _currentMatchID;
    protected bool _disconnectServer = false;
    public bool loadMainMenu = false;

    //used to disconnect a client properly when exiting the matchmaker
    [HideInInspector]
    public bool _isMatchmaking = false;
    [HideInInspector]
    public bool isHosting = false;
    [HideInInspector]
    public bool doAutoStart = true;
    private MenuSoundKind menuSoundKind = MenuSoundKind.menuSelect;
    public AudioClip menuOKSound;

    void Awake() {
        s_singleton = this;
        isHosting = false;
        doAutoStart = true;
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
        GetAudioClipFile(menuSoundKind);
        SoundManager.instance.SetVolumeSliders();
    }
    void GetAudioClipFile(MenuSoundKind sound) {
        menuOKSound = (AudioClip)Resources.Load("MenuSound/" + sound);
    }

    /// <summary>
    /// Change the current view to selected panel
    /// </summary>
    public void ChangeTo(
        GameObject newPanel,
        UnityEngine.Events.UnityAction backCallback
    ) {

        // disable current panel (if any)
        if (currentPanel != null) {
            currentPanel.SetActive(false);
        }

        // enable new panel (if any)
        if (newPanel != null) {
            newPanel.SetActive(true);
        }
        currentPanel = newPanel;

        // ensure status panel is enabled
        statusPanel.ToggleVisibility(true);

        if (currentPanel != gameSelectPanel.gameObject) {
            statusPanel.SetBackEnabled(true, backCallback);
        } else {
            statusPanel.SetBackEnabled(false, null);
            statusPanel.SetStatus("Offline", gameSelectPanel.host, gameSelectPanel.port);
        }
    }

    public void AddLocalPlayer() {
        // attempt to set new local player
        // Debug.Log("AddLocalPlayer");
        SoundManager.instance.PlayAudioClip(menuOKSound);
        TryToAddPlayer();
    }

    public void JoinMatch(
        NetworkID networkID
    ) {
        // join the match
        matchMaker.JoinMatch(networkID, "", "", "", 0, 0, OnMatchJoined);

        // set the back button to stop the client
        statusPanel.SetBackEnabled(true, () => { StopClientCallback(); });

        // Display
        infoPanel.Display("Connecting...", "Cancel", () => { StopClientCallback(); });
    }

    // ------------------------------------------------------
    // EVENT HANDLER METHODS

    // hook into NetworkManager client setup process
    public override void OnStartClient(NetworkClient mClient) {
        base.OnStartClient(mClient); // base implementation is currently empty
        SingedMessages.ClientRegisterMessageHandlers(mClient);
    }

    // hook into NetManagers server setup process
    public override void OnStartServer() {
        base.OnStartServer(); //base is empty
        SingedMessages.ServerRegisterMessageHandlers();
    }

    public override void OnMatchCreate(
        bool success,
        string extendedInfo,
        MatchInfo matchInfo
    ) {
        //Debug.Log("OnMatchCreate, success: " + success);
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        _currentMatchID = (System.UInt64) matchInfo.networkId;
    }

    public override void OnDestroyMatch(
        bool success,
        string extendedInfo
    ) {
        base.OnDestroyMatch(success, extendedInfo);
        if (_disconnectServer) {
            StopMatchMaker();
            StopHost();
        }
    }

    public override void OnLobbyServerDisconnect(
        NetworkConnection    connection
    ) {
        // Debug.Log("OnLobbyServerDisconnect");
    }

    public override void OnLobbyServerSceneChanged(string sceneName) {
        // if we are changing to the play scene... copy state into the turn manager
        if (sceneName == playScene) {
            var turnManager = TurnManager.singleton;
            turnManager.expectedPlayers = lobbyPanel.playerCount;
        }
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
            SoundManager.instance.PlayMenuMusic();
            //Debug.Log("changing to lobby scene");
            statusPanel.ToggleVisibility(true);

            if (statusPanel.isInGame) {
                if (isHosting) {
                    ChangeTo(lobbyPanel.gameObject, () => { StopHostCallback(); });
                } else {
                    ChangeTo(lobbyPanel.gameObject, () => { StopClientCallback(); });
                }

            } else {
                ChangeTo(gameSelectPanel.gameObject, null);
            }

        // otherwise ... game is starting
        } else {
            SoundManager.instance.PlayBattleMusic();
            //Debug.Log("changing to game scene");
            // disable lower panels, set back callback to stop game
            ChangeTo(null, () => { StopGameCallback(); });

            // hide status panel
            statusPanel.ToggleVisibility(false);
            statusPanel.isInGame = true;
        }
    }

    public override void OnStartHost() {
        isHosting = true;
        base.OnStartHost();

        // change to lobby view
        ChangeTo(lobbyPanel.gameObject, () => { StopHostCallback(); });

        // update main status
        statusPanel.SetStatus("Hosting", gameSelectPanel.host, gameSelectPanel.port);
    }

    public override void OnStopHost() {
        isHosting = false;
        base.OnStopHost();
    }

    public override void OnLobbyServerPlayersReady() {
        if (doAutoStart) {
            base.OnLobbyServerPlayersReady();
        } else {
            lobbyPanel.startGameButton.interactable = true;
        }
    }

    public void StartGame() {
        base.OnLobbyServerPlayersReady();
    }

    // ------------------------------------------------------
    // CALLBACK METHODS

    public void StopClientCallback() {
        //Debug.Log("pressed back!");
        // stop the client session
        StopClient();

        // if using matchMaker, stop the matchmaker service
        if (matchMaker != null) {
            StopMatchMaker();
        }

        // switch view back to main game select panel
        ChangeTo(gameSelectPanel.gameObject, null);
    }

    public void StopHostCallback() {
        //Debug.Log("being called when clicking back!");
        // if we are hosting a matchmaker game, handle teardown of match
        if (matchMaker != null) {
			matchMaker.DestroyMatch((NetworkID)_currentMatchID, 0, OnDestroyMatch);
			_disconnectServer = true;

        // otherwise, just stop the host
        } else {
            StopHost();
        }

        // switch view back to main game select panel
        ChangeTo(gameSelectPanel.gameObject, null);
    }

    public void StopGameCallback() {
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
