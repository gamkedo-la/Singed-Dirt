using UnityEngine;
using UnityEngine.UI;

public class LobbyInfoPanelController : MonoBehaviour {

    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text statusInfo;
    public Text buttonTextInfo;
    public Button cancelButton;

    public void Display(
        string statusText,
        string buttonText,
        UnityEngine.Events.UnityAction buttonCallback
    ) {

        // enable info panel
        gameObject.SetActive(true);

        // set status message and button text
        statusInfo.text = statusText;
        buttonTextInfo.text = buttonText;

        // setup cancel button action
        cancelButton.onClick.RemoveAllListeners();

        // register passed button action
        if (buttonCallback != null) {
            cancelButton.onClick.AddListener(buttonCallback);
        }

        // deactivate panel when putton is clicked
        cancelButton.onClick.AddListener(() => { gameObject.SetActive(false); });

    }

}
