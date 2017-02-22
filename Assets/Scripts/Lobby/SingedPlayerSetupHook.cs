using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Hook is called when changing scenes between lobby and game and is executed
/// for each player.
/// </summary>
public class SingedPlayerSetupHook : LobbyPlayerSetupHook {

    public override void SetupPlayer(
        NetworkManager manager,
        GameObject lobbyPlayer,
        GameObject gamePlayer
    ) {
        var lobbyController = lobbyPlayer.GetComponent<SingedLobbyPlayer>();
        var gameController = gamePlayer.GetComponent<TankController>();
        if (lobbyController != null && gameController != null) {
            // copy state from lobby -> game
            gameController.playerName = lobbyController.playerName;
        }
    }
}
