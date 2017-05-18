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
        if (SingedLobbyManager.singleton != null) return;
        lobbyPrefab = (GameObject)GameObject.Instantiate(lobbyPrefab);
        lobbyPrefab.SetActive(false);
        // instantiate if not
        // Debug.Log("instantiating lobby");
    }
    public void LoadLobby(){
        Debug.Log("I'm calling load lobby!");
        lobbyPrefab.SetActive(true);
        mainMenu.SetActive(false);
    }
}
