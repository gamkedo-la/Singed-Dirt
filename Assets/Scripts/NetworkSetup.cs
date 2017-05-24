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
        // Debug.Log("NetworkSetup start");
		// grab game setup info
		var gameSetupGO = GameObject.Find("GameSetup");
        // Debug.Log("gameSetupGO: " + gameSetupGO);

		// we were spawned without network manager
		if (netManager == null) {
            // FIXME... fill out netManager
            netManager = gameObject.AddComponent<NetworkManager>();
            // add HUD GUI to network manager
            netManager.gameObject.AddComponent<NetworkManagerHUD>();
        }
	}
}
