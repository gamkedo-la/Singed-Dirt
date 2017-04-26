using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ChatItemController: UxListElement {
    [Header("UI Reference")]
    public Text chatText;

    public void SetMessage(string message, Color color) {
        chatText.text = message;
        chatText.color = color;
    }

}
