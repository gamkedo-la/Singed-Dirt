using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Hook is called when changing scenes between lobby and game and is executed
/// for each player.
/// </summary>
public class SingedPlayerSetupHook : TankModelHook {

    public override void SetupPlayer(
        NetworkManager manager,
        GameObject lobbyPlayer,
        GameObject gamePlayer
    ) {
        var lobbyController = lobbyPlayer.GetComponent<SingedLobbyPlayer>();
        var gameController = gamePlayer.GetComponent<TankController>();
        Debug.Log("SetupPlayer called for " + lobbyController.playerName);
        if (lobbyController != null && gameController != null) {
            // copy state from lobby -> game
            gameController.playerName = lobbyController.playerName;
            gameController.tankBaseKind = lobbyController.tankBaseKind;
            gameController.turretBaseKind = lobbyController.turretBaseKind;
            gameController.turretKind = lobbyController.turretKind;
            gameController.hatKind = lobbyController.hatKind;
        }
    }
}
