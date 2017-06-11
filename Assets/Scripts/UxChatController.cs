using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// ========================================================================
/// <summary>
/// Class to manage the list of spawns
/// </summary>
public class UxChatController : NetworkBehaviour {
    public GameObject chatItemPrefab;
    public UxListController uxChatList;
    public Scrollbar uxScrollbar;
    public InputField uxChatInput;

    // ------------------------------------------------------
    // STATIC VARIABLES

    // ------------------------------------------------------
    // INSTANCE VARIABLES

    public void OnChatSend(string newChat) {
        newChat = uxChatInput.text;
        //Debug.Log("OnChatSend: " + newChat);
        uxChatInput.text = "";
        //GameManager.singleton.localPlayer.SendToConsole(newChat);
        // get active player
        var turnManager = TurnManager.singleton;
        if (newChat != "" && turnManager != null && turnManager.GetLocalActiveTank() != null) {
    		turnManager.GetLocalActiveTank().SendToConsole(newChat);
        }
    }

    public void AddMessage(string sender, Color color, string message) {
        var prefix = String.Format("[{0}] ", sender);
        var finalMessage = prefix + message;
        AddMessage(color, finalMessage);
    }

    public void AddMessage(string message) {
        if (isServer) {
            var prefix = "[System] ";
            var finalMessage = prefix + message;
            AddMessage(Color.white, finalMessage);
        }
    }

    public void AddMessage(Color color, string finalMessage) {
        if (isServer) {
            RpcAddMessage(color, finalMessage);
        } else {
            //Debug.Log("calling CmdAddMessage with: " + finalMessage);
            CmdAddMessage(color, finalMessage);
        }
    }

    [ClientRpc]
    void RpcAddMessage(Color color, string finalMessage) {
        //Debug.Log("RpcAddMessage with: " + finalMessage);
        // instantiate queue item
        var queueItem = (GameObject) Instantiate(chatItemPrefab);
        var itemController = queueItem.GetComponent<ChatItemController>();

        // set message properties
        itemController.SetMessage(finalMessage, color);

        uxChatList.Add(itemController);

        // we can't just set the scrollbar value to zero (go to bottom) here,
        // adding an entry to the chat causes the scrollbar value to automatically change such that
        // the scrollbar doesn't change position when adding an entry to the end... to account for this,
        // register a listener to onValueChanged so that we override the value a second time
        if (uxScrollbar != null) {
    		uxScrollbar.value = 0f;
            uxScrollbar.onValueChanged.AddListener(OnScrollValueChanged);
        }
    }

    public void
    OnScrollValueChanged(float value) {
        // remove self as listener
        uxScrollbar.onValueChanged.RemoveListener(OnScrollValueChanged);
        // set value to 0
		uxScrollbar.value = 0f;
    }

    [Command]
    void CmdAddMessage(Color color, string finalMessage) {
        //Debug.Log("CmdAddMessage with: " + finalMessage);
        RpcAddMessage(color, finalMessage);
    }

    // ------------------------------------------------------
    // STATIC FUNCTIONS
    public static void SendToConsole(string message) {
		var consoleGo = GameObject.FindWithTag("console");
		if (consoleGo != null) {
			var consoleController = consoleGo.GetComponent<UxChatController>();
			consoleController.AddMessage(message);
		}
    }

    public static void SendToConsole(string messageFrom, Color textColor, string message) {
        var consoleGo = GameObject.FindWithTag("console");
        if (consoleGo != null) {
            var consoleController = consoleGo.GetComponent<UxChatController>();
            consoleController.AddMessage(messageFrom, textColor, message);
        }
    }

    public static void SendToConsole(TankController player, string message) {
		var consoleGo = GameObject.FindWithTag("console");
		if (consoleGo != null) {
			var consoleController = consoleGo.GetComponent<UxChatController>();
			consoleController.AddMessage(player.playerName, Color.yellow, message);
		}
    }

}
