using System;
using UnityEngine;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text winnerText;
    public Button playAgainButton;
    public Button lobbyButton;
    public Button mainMenuButton;

    string[] winningStrings = {
        "All the base are belong to {0}",
        "{0} was in the base, killing the d00dz",
        "In life, there are no winners or losers...\r\nbut {0} totally won",
        "#{0}\r\n#Winning",
        "{0} was too legit to quit",
        "{0} was the winner we deserved",
        "The princess was in {0}'s castle",
    };
    string[] nukeStrings = {
        "Winner, winner, {0} got\r\na chicken dinner!",
        "{0} had the strongest morels!",
        "#{0}\r\n#MicDrop\r\n#RUWearing2MillionSunblock",
        "{0} asks,\r\n\"Are you not entertained?!\""
    };

    public void OnClickPlayAgain() {
    }

    public void OnClickLobby() {
        if (SingedLobbyManager.s_singleton != null) {
            SingedLobbyManager.s_singleton.SendReturnToLobby();
        }
    }

    public void OnClickMainMenu() {
        if (SingedLobbyManager.s_singleton != null) {
            SingedLobbyManager.s_singleton.loadMainMenu = true;
            SingedLobbyManager.s_singleton.StopGameCallback();
        }
    }

    public void SetWinner(string playerName, bool nukeEnding = false) {
        int index;
        if (nukeEnding) {
            index = UnityEngine.Random.Range(0, nukeStrings.Length);
            winnerText.text = String.Format(nukeStrings[index], playerName);
        }
        else {
            index = UnityEngine.Random.Range(0, winningStrings.Length);
            winnerText.text = String.Format(winningStrings[index], playerName);
        }
    }

}
