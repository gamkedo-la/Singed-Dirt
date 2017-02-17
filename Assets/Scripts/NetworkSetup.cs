using System;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSetup : MonoBehaviour {
    NetworkManager netManager;

	/// <summary>
	/// Upon start, setup the game
	/// </summary>
    void Start() {
        netManager = NetworkManager.singleton;
        // setup the game
        SetupGame();
    }

	/// <summary>
	/// The actual logic to setup the overall game play, based on values passed in GameSetup object
	/// </summary>
	void SetupGame () {
        Debug.Log("NetworkSetup start");
		// grab game setup info
		var gameSetupGO = GameObject.Find("GameSetup");
        Debug.Log("gameSetupGO: " + gameSetupGO);

		// we were spawned from GameSetup scene...
		if (gameSetupGO != null) {

            var gameSetup = gameSetupGO.GetComponent<GameSetup>();
            // set network manager state
            netManager.networkPort = gameSetup.port;

            // setup server
            if (gameSetup.gameMode == GameSetup.GameMode.hostMultiPlayer) {
                if (gameSetup.host != "localhost") {
                    netManager.serverBindToIP = true;
                    netManager.serverBindAddress = gameSetup.host;
                }

                Debug.Log("starting server");
                netManager.StartHost();

            // setup client
            } else {
                netManager.networkAddress = gameSetup.host;
                // start the client
                Debug.Log(String.Format("starting client: {0}:{1}", netManager.networkAddress, netManager.networkPort));
                netManager.StartClient();
            }

		// debug mode
		} else {

            // add HUD GUI to network manager
            netManager.gameObject.AddComponent<NetworkManagerHUD>();
		}
	}
}
