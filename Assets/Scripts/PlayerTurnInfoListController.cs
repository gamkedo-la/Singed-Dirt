using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnInfoListController: UxListController {
    public GameObject turnInfoPrefab;
    Dictionary<int, PlayerTurnInfoController> playerRegistry;

    void Awake() {
        playerRegistry = new Dictionary<int, PlayerTurnInfoController>();
    }

    public void AddPlayer(TankController player) {
        if (playerRegistry.ContainsKey(player.playerIndex)) {
            Remove(playerRegistry[player.playerIndex]);
        }

        // instantiated new
        var turnInfoGo = (GameObject) Instantiate(turnInfoPrefab);
        var turnInfoController = turnInfoGo.GetComponent<PlayerTurnInfoController>();
        turnInfoController.AssignPlayer(player);
        Add(turnInfoController);

        // save to registry
        playerRegistry[player.playerIndex] = turnInfoController;
    }

    public void RemovePlayer(TankController player) {
        if (playerRegistry.ContainsKey(player.playerIndex)) {
            Remove(playerRegistry[player.playerIndex]);
        }
    }

    public void ActivatePlayer(TankController player) {
        if (playerRegistry.ContainsKey(player.playerIndex)) {
            // mark all current players as inactive
            foreach (var playerIndex in playerRegistry.Keys) {
                playerRegistry[playerIndex].SetPlayerIsActive(false);
            }
            // mark selected player as active
            playerRegistry[player.playerIndex].SetPlayerIsActive(true);
        }

    }

}
