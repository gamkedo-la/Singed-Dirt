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
    public Button readyButton;
    public Button setupButton;
    public Button removeButton;
    public GameObject remoteIcon;
    public GameObject localIcon;

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
    static Color JoinColor = new Color(255.0f/255.0f, 0.0f, 101.0f/255.0f,1.0f);
    static Color NotReadyColor = new Color(34.0f / 255.0f, 44 / 255.0f, 55.0f / 255.0f, 1.0f);
    static Color ReadyColor = new Color(0.0f, 204.0f / 255.0f, 204.0f / 255.0f, 1.0f);
    static Color TransparentColor = new Color(0, 0, 0, 0);

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
    public override void OnClientReady(bool readyState) {
        if (readyState) {
            //ChangeReadyButtonColor(TransparentColor);

            Text textComponent = readyButton.transform.GetChild(0).GetComponent<Text>();
            textComponent.text = "READY";
            textComponent.color = ReadyColor;
            readyButton.interactable = false;
            setupButton.interactable = false;
            playerNameInput.interactable = false;
        } else {
            //ChangeReadyButtonColor(isLocalPlayer ? JoinColor : NotReadyColor);

            Text textComponent = readyButton.transform.GetChild(0).GetComponent<Text>();
            textComponent.text = isLocalPlayer ? "JOIN" : "...";
            textComponent.color = Color.white;
            readyButton.interactable = isLocalPlayer;
            setupButton.interactable = isLocalPlayer;
            playerNameInput.interactable = isLocalPlayer;
        }
    }

    public void OnNameChanged(
        string newName
    ) {
        CmdNameChanged(newName);
    }

    public void OnReadyClicked() {
        // send ready message to lobby manager
        SendReadyToBeginMessage();
    }

    public override void OnStartAuthority() {
        Debug.Log("OnStartAuthority");
        base.OnStartAuthority();

        //if we return from a game, color of text can still be the one for "Ready"
        //readyButton.transform.GetChild(0).GetComponent<Text>().color = Color.white;

        SetupLocalPlayer();
    }

    void SetupOtherPlayer() {
        Debug.Log("SetupOtherPlayer");
        // can't set name of other player
        playerNameInput.interactable = false;

        // server can boot client
        removeButton.interactable = NetworkServer.active;

        //ChangeReadyButtonColor(NotReadyColor);

        // disable ready button for other player
        readyButton.transform.GetChild(0).GetComponent<Text>().text = "...";
        readyButton.interactable = false;

        OnClientReady(false);
    }

    void SetupLocalPlayer() {
        Debug.Log("SetupLocalPlayer");
        playerNameInput.interactable = true;
        remoteIcon.gameObject.SetActive(false);
        localIcon.gameObject.SetActive(true);

        CheckRemoveButton();

        //ChangeReadyButtonColor(JoinColor);

        readyButton.transform.GetChild(0).GetComponent<Text>().text = "JOIN";
        readyButton.interactable = true;

        //have to use child count of player prefab already setup as "this.slot" is not set yet
        if (playerName == "") {
            Debug.Log("setting player name: numplayers: " + SingedLobbyManager.s_singleton.playerCount);
            Debug.Log("transform count: " + (LobbyPanelManager.singleton.playerListContentTransform.childCount-1).ToString());
            var name = "Player" + (LobbyPanelManager.singleton.playerListContentTransform.childCount-1).ToString();
            Debug.Log("name: " + name);
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

        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(OnReadyClicked);

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
        Debug.Log("OnMyName setting playerName: " + newName);
        playerName = newName;
        playerNameInput.text = newName;
        //playerNameInput.transform.GetChild(0).GetComponent<Text>().text = newName;
        Debug.Log("playerNameInput: " + playerNameInput.text);
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
        Debug.Log("CmdNameChanged setting playerName: " + newName);
        Debug.Log("setting playerName: " + playerName);
        playerName = newName;
    }
}
