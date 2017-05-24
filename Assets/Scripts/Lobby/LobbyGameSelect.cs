using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyGameSelect : MonoBehaviour {

    //SingedLobbyManager lobbyManager;

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public InputField hostInput;
    public InputField portInput;
    public InputField matchNameInput;

    private AudioClip menuOKSound;
    private MenuSoundKind menuSoundKind = MenuSoundKind.menuSelect;
    private MusicKind musicKind = MusicKind.mainMenuMusic;

    void Start() {
        //lobbyManager = SingedLobbyManager.s_singleton;
        SoundManager.instance.PlayMenuMusic();
        GetAudioClipFile(MenuSoundKind.menuSelect);
    }

    public string host {
        get {
            if (hostInput != null) {
                return hostInput.text;
            } else {
                return null;
            }
        }
    }

    public int port {
        get {
            if (portInput != null) {
                return Int32.Parse(portInput.text);
            } else {
                return 0;
            }
        }
    }

    void GetAudioClipFile(MenuSoundKind sound) {
        menuOKSound = (AudioClip)Resources.Load("MenuSound/" + sound);
    }

    public void OnClickHost() {
        // Debug.Log("OnClickHost");

        // change to lobby
        //lobbyManager.ChangeTo(lobbyManager.lobbyPanel.gameObject);

        var lobbyManager = SingedLobbyManager.s_singleton;
        SoundManager.instance.PlayAudioClip(menuOKSound);

        // set hosting address/port
        lobbyManager.networkPort = port;
        lobbyManager.serverBindToIP = true;
        lobbyManager.serverBindAddress = host;

        // start the game as host (client/server)
        // NOTE: this will cause OnStartHost to be called in lobby manager
        lobbyManager.StartHost();
    }

    public void OnClickJoin() {
        // Debug.Log("OnClickJoin");

        var lobbyManager = SingedLobbyManager.s_singleton;
        SoundManager.instance.PlayAudioClip(menuOKSound);

        // set connect address/port
        lobbyManager.networkAddress = host;
        lobbyManager.networkPort = port;

        // start the game as client
        lobbyManager.StartClient();

        // display connecting info panel
        lobbyManager.infoPanel.Display("Connecting...", "Cancel", () => { lobbyManager.StopClientCallback(); });

        // set status
        lobbyManager.statusPanel.SetStatus("Connecting...", host, port);
    }

    public void OnClickCreateMatchmakingGame() {
        var lobbyManager = SingedLobbyManager.s_singleton;

        
        SoundManager.instance.PlayAudioClip(menuOKSound);

        lobbyManager.StartMatchMaker();
        // Debug.Log(String.Format("requesting match for name: {0}, maxPlayers: {1}", matchNameInput.text, lobbyManager.maxPlayers));
        lobbyManager.matchMaker.CreateMatch(
            matchNameInput.text,
            (uint)lobbyManager.maxPlayers,
            true,
            "", "", "", 0, 0,
            lobbyManager.OnMatchCreate);

        lobbyManager.statusPanel.SetBackEnabled(true, () => { lobbyManager.StopHostCallback(); });

        // display connecting info panel
        lobbyManager.infoPanel.Display("Connecting...", "Cancel", () => { lobbyManager.StopClientCallback(); });

        // set status
        lobbyManager.statusPanel.SetStatus("Matchmaker Connecting...", host, port);
    }

    public void OnClickOpenServerList() {
        var lobbyManager = SingedLobbyManager.s_singleton;
        GetAudioClipFile(MenuSoundKind.menuSelect);
        SoundManager.instance.PlayAudioClip(menuOKSound);
        lobbyManager.StartMatchMaker();
        // FIXME: validate this is the right callback
        lobbyManager.ChangeTo(lobbyManager.matchmakerServerPanel.gameObject,
                              () => { lobbyManager.StopClientCallback(); });
    }

    public void onEndEditGameName(
        string text
    ) {
        if (Input.GetKeyDown(KeyCode.Return)) {
            OnClickCreateMatchmakingGame();
        }
    }

}
