using UnityEngine;
using UnityEngine.UI;

public class LobbyLoader : MonoBehaviour {

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public GameObject lobbyPrefab;

    void Start() {
        // nothing to do if lobby is already instantiated
        if (SingedLobbyManager.singleton != null) return;

        // instantiate if not
        // Debug.Log("instantiating lobby");
        Instantiate(lobbyPrefab);
    }
}
