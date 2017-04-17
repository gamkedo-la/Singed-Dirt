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

    void Start() {
        //lobbyManager = SingedLobbyManager.s_singleton;
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

    public void OnClickHost() {
        // Debug.Log("OnClickHost");

        // change to lobby
        //lobbyManager.ChangeTo(lobbyManager.lobbyPanel.gameObject);

        // set hosting address/port
        var lobbyManager = SingedLobbyManager.s_singleton;
        lobbyManager.networkPort = port;
        lobbyManager.serverBindToIP = true;
        lobbyManager.serverBindAddress = host;

        // start the game as host (client/server)
        // NOTE: this will cause OnStartHost to be called in lobby manager
        lobbyManager.StartHost();
    }

    public void OnClickJoin() {
        // Debug.Log("OnClickJoin");

        // set connect address/port
        var lobbyManager = SingedLobbyManager.s_singleton;
        lobbyManager.networkAddress = host;
        lobbyManager.networkPort = port;

        // start the game as client
        lobbyManager.StartClient();

        // display connecting info panel
        //var cancelCallback = lobbyManager.StopClientCallback;
        lobbyManager.infoPanel.Display("Connecting...", "Cancel", () => { lobbyManager.StopClientCallback(); });

        // set status
        lobbyManager.statusPanel.SetStatus("Connecting...", host, port);
    }

}
