using UnityEngine;
using UnityEngine.UI;

public class LobbyStatus : MonoBehaviour {

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text gameStatusInfo;
    public Text hostInfo;
    public Text portInfo;
    public Button backButton;

    [HideInInspector]
    public bool isInGame = false;
    bool isDisplayed = true;

    private AudioClip menuBackSound;

    void Awake() {
        isInGame = false;
        // Debug.Log("LobbyStatus isInGame: " + isInGame);
        menuBackSound = (AudioClip)Resources.Load("MenuSound/" + MenuSoundKind.menuBack);
    }

    void Update() {
        // nothing to do if game is not playing
        if (!isInGame) return;

        // if game is playing, and escape is pressed, toggle status panel visibility
        if (Input.GetKeyDown(KeyCode.Escape)) {
            // Debug.Log("isInGame: " + isInGame);
            ToggleVisibility(!isDisplayed);
        }
    }

    void PlaySound(){
        SingedLobbyManager.s_singleton.PlayAudioClip(menuBackSound);
    }

    /// <summary>
    /// set the status fields, including status message, host and port
    /// </summary>
    public void SetStatus(
        string status,
        string host,
        int port
    ) {
        gameStatusInfo.text = status;
        hostInfo.text = host;
        portInfo.text = port.ToString();
    }

    /// <summary>
    /// enable/disable the back button and set callback (if enabled) to be called for back button
    /// </summary>
    public void SetBackEnabled(
        bool isEnabled,
        UnityEngine.Events.UnityAction backCallback
    ) {
        backButton.gameObject.SetActive(isEnabled);
        // Debug.Log("SetBackEnabled: " + isEnabled + " with callback: " + backCallback);

        // setup back button action (if enabled)
        if (isEnabled && backCallback != null) {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(backCallback);
            backButton.onClick.AddListener(() => { backButton.onClick.RemoveAllListeners(); });
            backButton.onClick.AddListener(PlaySound);
        }
    }

    /// <summary>
    /// toggle on/off the visibility of the status panel (different from enabling/disabling),
    /// as disable will also turn off the update functionality
    /// </summary>
    public void ToggleVisibility(
        bool visible
    ) {
        isDisplayed = visible;

        // toggle visibility of all objects under top panel
        foreach (Transform t in transform) {
            t.gameObject.SetActive(isDisplayed);
        }

        // toggle background image of panel
        var panelImage = GetComponent<Image>();
        if (panelImage != null) {
            panelImage.enabled = isDisplayed;
        }
    }

}
