using UnityEngine;
using UnityEngine.UI;

public class LobbyLoader : MonoBehaviour {

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public GameObject lobbyPrefab;
    public GameObject mainMenu;

    void Start() {
        // nothing to do if lobby is already instantiated
        Debug.Log("LobbyLoader start is getting called");
        if (SingedLobbyManager.singleton != null){
            Debug.Log("The lobby manager exists~");
            lobbyPrefab = GameObject.Find("LobbyManager(Clone)");
			mainMenu = GameObject.Find("MainMenu");
			if (mainMenu != null)
			{
				mainMenu.SetActive(false);
			}
            Debug.Log("I'm about to return after the manager exists");
            return;
        } 
        lobbyPrefab = (GameObject)GameObject.Instantiate(lobbyPrefab);
        lobbyPrefab.SetActive(false);
        // instantiate if not
        // Debug.Log("instantiating lobby");
    }
    public void LoadLobby(){
        // Debug.Log("I'm calling load lobby!");
        lobbyPrefab.SetActive(true);
        mainMenu.SetActive(false);
    }
}
