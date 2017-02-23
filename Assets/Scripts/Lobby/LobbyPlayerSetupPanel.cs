using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSetupPanel : MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text headerText;
    public LobbyPlayerSetup playerSetupModel;

    SingedLobbyPlayer linkedPlayer;

    public void Awake() {
        Debug.Log("LobbyPlayerSetupPanel Awake");
        GameObject modelGo = GameObject.Find("ModelPosition");
        if (modelGo != null) {
            var playerSetupGo = modelGo.transform.Find("PlayerSetupModel");
            Debug.Log("found playerSetupModel: " + playerSetupGo);
            playerSetupModel = playerSetupGo.GetComponent<LobbyPlayerSetup>();
        }

    }

    public void LinkPlayer(SingedLobbyPlayer lobbyPlayer) {
        // set linked player
        linkedPlayer = lobbyPlayer;
        // copy local state
        Debug.Log("linkedPlayer: " + linkedPlayer);
        Debug.Log("playerSetupModel: " + playerSetupModel);
        playerSetupModel.tankBaseKind = linkedPlayer.tankBaseKind;
        playerSetupModel.turretBaseKind = linkedPlayer.turretBaseKind;
        playerSetupModel.turretKind = linkedPlayer.turretKind;
        playerSetupModel.hatKind = linkedPlayer.hatKind;
        // update current avatar
        playerSetupModel.UpdateAvatar();
    }

    public void OnEnable() {
        playerSetupModel.gameObject.SetActive(true);
    }

    public void OnClickNextHat() {
        var intSelection = (int) playerSetupModel.hatKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankHatKind)).Length) intSelection = 0;
        playerSetupModel.hatKind = (TankHatKind) intSelection;
        playerSetupModel.UpdateAvatar();
    }
    public void OnClickPrevHat() {
        var intSelection = (int) playerSetupModel.hatKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankHatKind)).Length - 1;
        playerSetupModel.hatKind = (TankHatKind) intSelection;
        playerSetupModel.UpdateAvatar();
    }

    public void OnClickNextTurret() {
        var intSelection = (int) playerSetupModel.turretKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankTurretKind)).Length) intSelection = 0;
        playerSetupModel.turretKind = (TankTurretKind) intSelection;
        playerSetupModel.UpdateAvatar();
    }
    public void OnClickPrevTurret() {
        var intSelection = (int) playerSetupModel.turretKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankTurretKind)).Length - 1;
        playerSetupModel.turretKind = (TankTurretKind) intSelection;
        playerSetupModel.UpdateAvatar();
    }

    public void OnClickNextTurretBase() {
        var intSelection = (int) playerSetupModel.turretBaseKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankTurretBaseKind)).Length) intSelection = 0;
        playerSetupModel.turretBaseKind = (TankTurretBaseKind) intSelection;
        playerSetupModel.UpdateAvatar();
    }
    public void OnClickPrevTurretBase() {
        var intSelection = (int) playerSetupModel.turretBaseKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankTurretBaseKind)).Length - 1;
        playerSetupModel.turretBaseKind = (TankTurretBaseKind) intSelection;
        playerSetupModel.UpdateAvatar();
    }

    public void OnClickNextBase() {
        var intSelection = (int) playerSetupModel.tankBaseKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankBaseKind)).Length) intSelection = 0;
        playerSetupModel.tankBaseKind = (TankBaseKind) intSelection;
        playerSetupModel.UpdateAvatar();
    }
    public void OnClickPrevBase() {
        var intSelection = (int) playerSetupModel.tankBaseKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankBaseKind)).Length - 1;
        playerSetupModel.tankBaseKind = (TankBaseKind) intSelection;
        playerSetupModel.UpdateAvatar();
    }

    public void OnClickCancel() {
        // change back to lobby
        var manager = SingedLobbyManager.s_singleton;
        if (manager != null) {
            manager.ChangeTo(manager.lobbyPanel.gameObject, () => { manager.StopHostCallback(); });
        }
        // disassociate linked player
        linkedPlayer = null;
        // turn off game model
        playerSetupModel.gameObject.SetActive(false);
    }

    public void OnClickAccept() {
        if (linkedPlayer != null) {
            linkedPlayer.tankBaseKind = playerSetupModel.tankBaseKind;
            linkedPlayer.turretBaseKind = playerSetupModel.turretBaseKind;
            linkedPlayer.turretKind = playerSetupModel.turretKind;
            linkedPlayer.hatKind = playerSetupModel.hatKind;
        }
        // change back to lobby
        var manager = SingedLobbyManager.s_singleton;
        if (manager != null) {
            manager.ChangeTo(manager.lobbyPanel.gameObject, () => { manager.StopHostCallback(); });
        }
        // disassociate linked player
        linkedPlayer = null;
        // turn off game model
        playerSetupModel.gameObject.SetActive(false);
    }


}
