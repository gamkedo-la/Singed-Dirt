using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// Class representing the lobby player, and associated controls
/// </summary>
public class SingedLobbyPlayer : NetworkLobbyPlayer {

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text playerStatusInfo;
    public InputField playerNameInput;
    public Toggle readyToggle;
    public Button setupButton;
    public Button removeButton;
    public GameObject remoteIcon;
    public GameObject localIcon;
    private MenuSoundKind menuSoundKind = MenuSoundKind.menuSelect;
    private AudioClip menuOKSound;
    private AudioClip menuBadSound;
    SingedLobbyManager lobbyManager;

    [Header("Player Variables")]
    [SyncVar]
    TankBaseKind _tankBaseKind;
    public TankBaseKind tankBaseKind {
        get {
            return _tankBaseKind;
        }
        set {
            _tankBaseKind = value;
            if (!isServer) CmdTankBaseChanged(value);
        }
    }

    [SyncVar]
    TankTurretBaseKind _turretBaseKind;
    public TankTurretBaseKind turretBaseKind {
        get {
            return _turretBaseKind;
        }
        set {
            _turretBaseKind = value;
            if (!isServer) CmdTurretBaseChanged(value);
        }
    }

    [SyncVar]
    TankTurretKind _turretKind;
    public TankTurretKind turretKind {
        get {
            return _turretKind;
        }
        set {
            _turretKind = value;
            if (!isServer) CmdTurretChanged(value);
        }
    }

    [SyncVar]
    TankHatKind _hatKind;
    public TankHatKind hatKind {
        get {
            return _hatKind;
        }
        set {
            _hatKind = value;
            if (!isServer) CmdHatChanged(value);
        }
    }

    void GetAudioClipFile(MenuSoundKind sound, bool isBad) {
        if (isBad){
            menuBadSound = (AudioClip)Resources.Load("MenuSound/" + sound);
        } else {
            menuOKSound = (AudioClip)Resources.Load("MenuSound/" + sound);
        }
    }

    //public TankBaseKind tankBaseKind = TankBaseKind.standard;
    //public TankTurretBaseKind turretBaseKind = TankTurretBaseKind.standard;
    //public TankTurretKind turretKind = TankTurretKind.standard;
    //public TankHatKind hatKind = TankHatKind.sunBlue;

    [SyncVar(hook = "OnMyName")]
    public string playerName = "";

    // FIXME: evaluate
    public Color OddRowColor = ParseHex.ToColor("562F00FF");
    public Color EvenRowColor = ParseHex.ToColor("814C00FF");

    void Awake() {
        // Debug.Log("SingedLobbyPlayer Awake: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
        GetAudioClipFile(MenuSoundKind.menuSelect, false);
        GetAudioClipFile(MenuSoundKind.menuBack, true);
        lobbyManager = SingedLobbyManager.s_singleton;
    }

    // ------------------------------------------------------
    // EVENT HANDLER METHODS
    /// <summary>
    /// Called when player enters the lobby
    /// </summary>
    public override void OnClientEnterLobby() {
        Debug.Log("OnClientEnterLobby: " + this);
        base.OnClientEnterLobby();

        // setup local/other player
        if (isLocalPlayer) {
            SetupLocalPlayer();
        } else {
            SetupOtherPlayer();
        }

        // update lobby panel
        LobbyPanelManager.singleton.AddPlayer(this);

        //setup the player data on UI. The value are SyncVar so the player
        //will be created with the right value currently on server
        OnMyName(playerName);
    }

    public void OnPlayerListChanged(
        int index
    ) {
        GetComponent<Image>().color = (index % 2 == 0) ? EvenRowColor : OddRowColor;
    }


    /// <summary>
    /// Callback executed when client ready state changes
    /// </summary>
    public override void OnClientReady(
        bool readyState
    ) {
        if (!isLocalPlayer) {
            readyToggle.isOn = readyState;
        }
        // disable controls if ready is pressed (NOTE: still allow player to unselect ready state)
        if (readyState) {
            setupButton.interactable = false;
            playerNameInput.interactable = false;

        // otherwise... re-enable controls
        } else {
            readyToggle.interactable = isLocalPlayer;
            setupButton.interactable = isLocalPlayer;
            playerNameInput.interactable = isLocalPlayer;
        }
    }

    public void OnNameChanged(
        string newName
    ) {
        CmdNameChanged(newName);
    }

    public void OnClickReady(bool value) {
        SoundManager.instance.PlayAudioClip(menuOKSound);
        // send ready or not ready message to lobby manager
        if (readyToggle.isOn) {
            SendReadyToBeginMessage();
        } else {
            SendNotReadyToBeginMessage();
        }
    }

    public void OnClickSetup() {
        SoundManager.instance.PlayAudioClip(menuOKSound);
        var manager = SingedLobbyManager.s_singleton;
        if (manager != null) {
            // change to playerSetupPanel
            manager.ChangeTo(manager.playerSetupPanel.gameObject, null);
            // link player setup panel to current lobby player
            manager.playerSetupPanel.LinkPlayer(this);
        }
    }

    public override void OnStartAuthority() {
        // Debug.Log("OnStartAuthority");
        base.OnStartAuthority();
        SetupLocalPlayer();
    }

    void SetupOtherPlayer() {
        // Debug.Log("SetupOtherPlayer");
        // can't set name of other player
        playerNameInput.interactable = false;

        // server can boot client
        removeButton.interactable = NetworkServer.active;


        // disable ready button for other player
        readyToggle.interactable = false;
        readyToggle.isOn = false;

        OnClientReady(false);
    }

    void SetupLocalPlayer() {
        // Debug.Log("SetupLocalPlayer");
        playerNameInput.interactable = true;
        remoteIcon.gameObject.SetActive(false);
        localIcon.gameObject.SetActive(true);

        CheckRemoveButton();
        readyToggle.interactable = true;
        readyToggle.isOn = false;

        //have to use child count of player prefab already setup as "this.slot" is not set yet
        if (playerName == "") {
            var name = "Player" + LobbyPanelManager.singleton.playerCount.ToString();
            CmdNameChanged(name);
        }

        //we switch from simple name display to name input
        setupButton.interactable = true;
        playerNameInput.interactable = true;

        playerNameInput.onEndEdit.RemoveAllListeners();
        playerNameInput.onEndEdit.AddListener(OnNameChanged);

        readyToggle.onValueChanged.RemoveAllListeners();
        readyToggle.onValueChanged.AddListener(OnClickReady);

    }

    /// <summary>
    /// check if remove button should be enabled, based on # of local players
    /// </summary>
    public void CheckRemoveButton() {
        if (!isLocalPlayer) return;

        int localPlayerCount = 0;
        foreach (var player in ClientScene.localPlayers) {
            localPlayerCount += (player == null || player.playerControllerId == -1) ? 0 : 1;
        }

        removeButton.interactable = localPlayerCount > 1;
    }

    // ------------------------------------------------------
    // CALLBACK METHODS
    public void OnMyName(
        string newName
    ) {
        playerName = newName;
        playerNameInput.text = newName;
    }

    public void OnClickRemovePlayer() {
        SoundManager.instance.PlayAudioClip(menuBadSound);
        if (isLocalPlayer) {
            RemovePlayer();
        } else if (isServer) {
            SingedLobbyManager.s_singleton.KickPlayer(connectionToClient);
        }
    }

    public void OnDestroy() {
        // Debug.Log("OnDestroy: " + this);
        // remove player from lobby panel and update lobby manager
        if (LobbyPanelManager.singleton != null) {
            LobbyPanelManager.singleton.RemovePlayer(this);
        }
    }

    // ------------------------------------------------------
    // CLIENT->SERVER METHODS
    [Command]
    public void CmdNameChanged(
        string newName
    ) {
        playerName = newName;
    }

    [Command]
    public void CmdTankBaseChanged(
        TankBaseKind newKind
    ) {
        tankBaseKind = newKind;
    }

    [Command]
    public void CmdTurretBaseChanged(
        TankTurretBaseKind newKind
    ) {
        turretBaseKind = newKind;
    }

    [Command]
    public void CmdTurretChanged(
        TankTurretKind newKind
    ) {
        turretKind = newKind;
    }

    [Command]
    public void CmdHatChanged(
        TankHatKind newKind
    ) {
        hatKind = newKind;
    }

}
