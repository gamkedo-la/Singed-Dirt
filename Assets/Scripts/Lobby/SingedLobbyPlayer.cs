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
    //public Button readyButton;
    public Toggle readyToggle;
    public Button setupButton;
    public Button removeButton;
    public GameObject remoteIcon;
    public GameObject localIcon;

    [Header("Player Variables")]
    public TankBaseKind tankBaseKind = TankBaseKind.standard;
    public TankTurretBaseKind turretBaseKind = TankTurretBaseKind.standard;
    public TankTurretKind turretKind = TankTurretKind.standard;
    public TankHatKind hatKind = TankHatKind.sunBlue;

    [SyncVar(hook = "OnMyName")]
    public string playerName = "";

    public static Color hexToColor(string hex) {
        hex = hex.Replace ("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace ("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if(hex.Length == 8){
            a = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r,g,b,a);
    }

    // FIXME: evaluate
    public Color OddRowColor = hexToColor("562F00FF");
    public Color EvenRowColor = hexToColor("814C00FF");

    void Awake() {
        Debug.Log("SingedLobbyPlayer Awake: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
    }

    // ------------------------------------------------------
    // EVENT HANDLER METHODS
    /// <summary>
    /// Called when player enters the lobby
    /// </summary>
    public override void OnClientEnterLobby() {
        Debug.Log("OnClientEnterLobby: " + this);
        base.OnClientEnterLobby();

        // update lobby manager to indicate new player joined
        if (SingedLobbyManager.s_singleton != null) {
            SingedLobbyManager.s_singleton.OnPlayersNumberModified(1);
        }

        // update lobby panel
        LobbyPanelManager.singleton.AddPlayer(this);

        // setup local/other player
        if (isLocalPlayer) {
            SetupLocalPlayer();
        } else {
            SetupOtherPlayer();
        }

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
        // send ready or not ready message to lobby manager
        if (readyToggle.isOn) {
            SendReadyToBeginMessage();
        } else {
            SendNotReadyToBeginMessage();
        }
    }

    public void OnClickSetup() {
        var manager = SingedLobbyManager.s_singleton;
        if (manager != null) {
            // change to playerSetupPanel
            manager.ChangeTo(manager.playerSetupPanel.gameObject, null);
            // link player setup panel to current lobby player
            manager.playerSetupPanel.LinkPlayer(this);
        }
    }

    public override void OnStartAuthority() {
        Debug.Log("OnStartAuthority");
        base.OnStartAuthority();
        SetupLocalPlayer();
    }

    void SetupOtherPlayer() {
        Debug.Log("SetupOtherPlayer");
        // can't set name of other player
        playerNameInput.interactable = false;

        // server can boot client
        removeButton.interactable = NetworkServer.active;


        // disable ready button for other player
        readyToggle.interactable = false;

        OnClientReady(false);
    }

    void SetupLocalPlayer() {
        Debug.Log("SetupLocalPlayer");
        playerNameInput.interactable = true;
        remoteIcon.gameObject.SetActive(false);
        localIcon.gameObject.SetActive(true);

        CheckRemoveButton();
        readyToggle.interactable = true;

        //have to use child count of player prefab already setup as "this.slot" is not set yet
        if (playerName == "") {
            var name = "Player" + (LobbyPanelManager.singleton.playerListContentTransform.childCount-1).ToString();
            CmdNameChanged(name);
        } else {
            Debug.Log("not setting initial name, current player name: " + playerName);
        }

        //we switch from simple name display to name input
        setupButton.interactable = true;
        playerNameInput.interactable = true;

        playerNameInput.onEndEdit.RemoveAllListeners();
        playerNameInput.onEndEdit.AddListener(OnNameChanged);

        /*
        setupButton.onClick.RemoveAllListeners();
        setupButton.onClick.AddListener(OnColorClicked);
        */

        readyToggle.onValueChanged.RemoveAllListeners();
        readyToggle.onValueChanged.AddListener(OnClickReady);

        //when OnClientEnterLobby is called, the loval PlayerController is not yet created, so we need to redo that here to disable
        //the add button if we reach maxLocalPlayer. We pass 0, as it was already counted on OnClientEnterLobby
        if (SingedLobbyManager.s_singleton != null) SingedLobbyManager.s_singleton.OnPlayersNumberModified(0);
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
        if (isLocalPlayer) {
            RemovePlayer();
        } else if (isServer) {
            SingedLobbyManager.s_singleton.KickPlayer(connectionToClient);
        }
    }

    public void OnDestroy() {
        Debug.Log("OnDestroy: " + this);
        // remove player from lobby panel and update lobby manager
        LobbyPanelManager.singleton.RemovePlayer(this);
        SingedLobbyManager.s_singleton.OnPlayersNumberModified(-1);
    }

    // ------------------------------------------------------
    // CLIENT->SERVER METHODS
    [Command]
    public void CmdNameChanged(
        string newName
    ) {
        playerName = newName;
    }
}
