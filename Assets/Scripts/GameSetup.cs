using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSetup : MonoBehaviour {

    public enum GameMode {
        singlePlayer,
        hostMultiPlayer,
        joinMultiPlayer
    }

    // Public variables
    // main UI panels
    public GameObject startPanel;
    public GameObject hostMultiPanel;
    public GameObject joinMultiPanel;
    GameObject[] allPanels;

    // UI elements to pull data from
    public Toggle hotSeatToggle;
    public InputField hostMultiHostInput;
    public InputField hostMultiPortInput;
    public InputField joinMultiHostInput;
    public InputField joinMultiPortInput;
    public Text numPlayersDropdownLabel;

    // game state
    public GameMode gameMode;
    public bool hotSeat;
    public string host;
    public int port;
    public int numPlayers;

	/// <summary>
	/// setup convenience vars, and set this gameObject up so that it will not be destroyed between scene loads
	/// </summary>
    void Awake() {
        allPanels = new GameObject[] { startPanel, hostMultiPanel, joinMultiPanel };
        DontDestroyOnLoad(transform.gameObject);
    }

	/// <summary>
	/// enable the starting panel, disable all others
	/// </summary>
    void Start() {
        DisablePanels();
        EnableStartPanel();
    }

	/// <summary>
	/// disable all panels
	/// </summary>
    void DisablePanels() {
        foreach (var panel in allPanels) {
            panel.SetActive(false);
        }
    }

	/// <summary>
	/// enable the start panel
	/// </summary>
    public void EnableStartPanel() {
        DisablePanels();
        startPanel.SetActive(true);
    }

	/// <summary>
	/// enable the host multiplayer panel
	/// </summary>
    public void EnableHostMultiPanel() {
        DisablePanels();
        gameMode = GameMode.hostMultiPlayer;
        hostMultiPanel.SetActive(true);
    }

	/// <summary>
	/// enable the join multiplayer panel
	/// </summary>
    public void EnableJoinMultiPanel() {
        DisablePanels();
        gameMode = GameMode.joinMultiPlayer;
        joinMultiPanel.SetActive(true);
    }

	/// <summary>
	/// handle host multiplayer start
	/// </summary>
    public void OnHostMultiStart() {
        // gather game state
        hotSeat = hotSeatToggle.isOn;
        host = hostMultiHostInput.text;
        port = Int32.Parse(hostMultiPortInput.text);
        var numPlayersText = numPlayersDropdownLabel.GetComponent<Text>();
        numPlayers = Int32.Parse(numPlayersText.text);

        /*Debug.Log(String.Format("gameMode: {0} " +
            "hotSeat: {1} " +
            "host: {2} " +
            "port: {3} " +
            "numPlayers: {4} ",
            gameMode, hotSeat, host, port, numPlayers));*/
        StartGame();
    }

	/// <summary>
	/// handle join multiplayer start
	/// </summary>
    public void OnJoinMultiStart() {
        // gather game state
        host = joinMultiHostInput.text;
        port = Int32.Parse(joinMultiPortInput.text);

        /*Debug.Log(String.Format("gameMode: {0} " +
            "host: {1} " +
            "port: {2} ",
            gameMode, host, port));*/
        StartGame();
    }

	/// <summary>
	/// actually start the game by loading the Game scene
	/// </summary>
    public void StartGame() {
        SceneManager.LoadSceneAsync("Game");
    }

}
