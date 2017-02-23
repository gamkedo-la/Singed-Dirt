using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSetup : MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text headerText;
    // empty game objects marking parent location for each part of the tank
    public GameObject tankBase;
    public GameObject turretBase;
    public GameObject turret;
    public GameObject hat;

    SingedLobbyPlayer linkedPlayer;
    TankBaseKind tankBaseKind = TankBaseKind.standard;
    TankTurretBaseKind turretBaseKind = TankTurretBaseKind.standard;
    TankTurretKind turretKind = TankTurretKind.standard;
    TankHatKind hatKind = TankHatKind.sunBlue;

    // instantiated prefabs for each part of the tank
    GameObject tankBasePrefab = null;
    GameObject turretBasePrefab = null;
    GameObject turretPrefab = null;
    GameObject hatPrefab = null;

    void UpdateAvatar() {
        // instantiate tank base
        if (tankBasePrefab != null) DestroyImmediate(tankBasePrefab);
        var prefab = PrefabRegistry.singleton.GetPrefab<TankBaseKind>(tankBaseKind);
		tankBasePrefab = (GameObject) GameObject.Instantiate(prefab, tankBase.transform);

        // instantiate turret base
        if (turretBasePrefab != null) DestroyImmediate(turretBasePrefab);
        prefab = PrefabRegistry.singleton.GetPrefab<TankTurretBaseKind>(turretBaseKind);
		turretBasePrefab = (GameObject) GameObject.Instantiate(prefab, turretBase.transform);

        // instantiate turret
        if (turretPrefab != null) DestroyImmediate(turretPrefab);
        prefab = PrefabRegistry.singleton.GetPrefab<TankTurretKind>(turretKind);
		turretPrefab = (GameObject) GameObject.Instantiate(prefab, turret.transform);

        // instantiate hat
        if (hatPrefab != null) DestroyImmediate(hatPrefab);
        prefab = PrefabRegistry.singleton.GetPrefab<TankHatKind>(hatKind);
		hatPrefab = (GameObject) GameObject.Instantiate(prefab, hat.transform);
    }

    public void LinkPlayer(SingedLobbyPlayer lobbyPlayer) {
        // set linked player
        linkedPlayer = lobbyPlayer;
        // copy local state
        tankBaseKind = linkedPlayer.tankBaseKind;
        turretBaseKind = linkedPlayer.turretBaseKind;
        turretKind = linkedPlayer.turretKind;
        hatKind = linkedPlayer.hatKind;
        // update current avatar
        UpdateAvatar();
    }

    public void OnClickNextHat() {
        var intSelection = (int) hatKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankHatKind)).Length) intSelection = 0;
        hatKind = (TankHatKind) intSelection;
        UpdateAvatar();
    }
    public void OnClickPrevHat() {
        var intSelection = (int) hatKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankHatKind)).Length - 1;
        hatKind = (TankHatKind) intSelection;
        UpdateAvatar();
    }

    public void OnClickNextTurret() {
        var intSelection = (int) turretKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankTurretKind)).Length) intSelection = 0;
        turretKind = (TankTurretKind) intSelection;
        UpdateAvatar();
    }
    public void OnClickPrevTurret() {
        var intSelection = (int) turretKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankTurretKind)).Length - 1;
        turretKind = (TankTurretKind) intSelection;
        UpdateAvatar();
    }

    public void OnClickNextTurretBase() {
        var intSelection = (int) turretBaseKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankTurretBaseKind)).Length) intSelection = 0;
        turretBaseKind = (TankTurretBaseKind) intSelection;
        UpdateAvatar();
    }
    public void OnClickPrevTurretBase() {
        var intSelection = (int) turretBaseKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankTurretBaseKind)).Length - 1;
        turretBaseKind = (TankTurretBaseKind) intSelection;
        UpdateAvatar();
    }

    public void OnClickNextBase() {
        var intSelection = (int) tankBaseKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankBaseKind)).Length) intSelection = 0;
        tankBaseKind = (TankBaseKind) intSelection;
        UpdateAvatar();
    }
    public void OnClickPrevBase() {
        var intSelection = (int) tankBaseKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankBaseKind)).Length - 1;
        tankBaseKind = (TankBaseKind) intSelection;
        UpdateAvatar();
    }

    public void OnClickCancel() {
        // change back to lobby
        var manager = SingedLobbyManager.s_singleton;
        if (manager != null) {
            manager.ChangeTo(manager.lobbyPanel.gameObject, () => { manager.StopHostCallback(); });
        }
        // disassociate linked player
        linkedPlayer = null;
    }

    public void OnClickAccept() {
        if (linkedPlayer != null) {
            linkedPlayer.tankBaseKind = tankBaseKind;
            linkedPlayer.turretBaseKind = turretBaseKind;
            linkedPlayer.turretKind = turretKind;
            linkedPlayer.hatKind = hatKind;
        }
        // change back to lobby
        var manager = SingedLobbyManager.s_singleton;
        if (manager != null) {
            manager.ChangeTo(manager.lobbyPanel.gameObject, () => { manager.StopHostCallback(); });
        }
        // disassociate linked player
        linkedPlayer = null;
    }


}
