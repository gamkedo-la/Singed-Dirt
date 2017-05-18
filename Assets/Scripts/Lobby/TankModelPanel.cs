using System;
using UnityEngine;
using UnityEngine.UI;

public class TankModelPanel : MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text headerText;
    public TankModel tankModel;

    SingedLobbyPlayer linkedPlayer;

	public float rotationSpeed = 10.0f;
	public float elevationSpeed = 30.0f;

	private AudioClip tankOption;
	private AudioClip tankRotate;

    public void Awake() {
        Debug.Log("TankModelPanel Awake");
        GameObject modelGo = GameObject.Find("ModelPosition");
        if (modelGo != null) {
            var playerSetupGo = modelGo.transform.Find("TankModel");
            Debug.Log("found tankModel: " + playerSetupGo);
            tankModel = playerSetupGo.GetComponent<TankModel>();
        }

    }

	void GetAudioClipFile(MenuSoundKind sound) {
		tankOption = (AudioClip)Resources.Load("MenuSound/" + sound);
		tankRotate = (AudioClip)Resources.Load("MenuSound/" + sound);
	}

    void Update() {
        tankModel.tankRotation += Input.GetAxis ("Horizontal") * Time.deltaTime * rotationSpeed;
        tankModel.turretElevation += Input.GetAxis ("Vertical") * Time.deltaTime * elevationSpeed;
    }

    public void LinkPlayer(SingedLobbyPlayer lobbyPlayer) {
        // set linked player
        linkedPlayer = lobbyPlayer;
        // copy local state
        Debug.Log("linkedPlayer: " + linkedPlayer);
        Debug.Log("tankModel: " + tankModel);
        tankModel.tankBaseKind = linkedPlayer.tankBaseKind;
        tankModel.turretBaseKind = linkedPlayer.turretBaseKind;
        tankModel.turretKind = linkedPlayer.turretKind;
        tankModel.hatKind = linkedPlayer.hatKind;
        // update current avatar
        tankModel.UpdateAvatar();
    }

    public void OnEnable() {
        tankModel.gameObject.SetActive(true);
    }

    public void OnClickNextHat() {
        var intSelection = (int) tankModel.hatKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankHatKind)).Length) intSelection = 0;
        tankModel.hatKind = (TankHatKind) intSelection;
        tankModel.UpdateAvatar();
		GetAudioClipFile (MenuSoundKind.ui_tank_option);
		SoundManager.instance.PlayAudioClip (tankOption);
    }
    public void OnClickPrevHat() {
        var intSelection = (int) tankModel.hatKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankHatKind)).Length - 1;
        tankModel.hatKind = (TankHatKind) intSelection;
        tankModel.UpdateAvatar();
		GetAudioClipFile (MenuSoundKind.ui_tank_option);
		SoundManager.instance.PlayAudioClip (tankOption);
    }

    public void OnClickNextTurret() {
        var intSelection = (int) tankModel.turretKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankTurretKind)).Length) intSelection = 0;
        tankModel.turretKind = (TankTurretKind) intSelection;
        tankModel.UpdateAvatar();
		GetAudioClipFile (MenuSoundKind.ui_tank_option);
		SoundManager.instance.PlayAudioClip (tankOption);
    }
    public void OnClickPrevTurret() {
        var intSelection = (int) tankModel.turretKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankTurretKind)).Length - 1;
        tankModel.turretKind = (TankTurretKind) intSelection;
        tankModel.UpdateAvatar();
		GetAudioClipFile (MenuSoundKind.ui_tank_option);
		SoundManager.instance.PlayAudioClip (tankOption);
    }

    public void OnClickNextTurretBase() {
        var intSelection = (int) tankModel.turretBaseKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankTurretBaseKind)).Length) intSelection = 0;
        tankModel.turretBaseKind = (TankTurretBaseKind) intSelection;
        tankModel.UpdateAvatar();
		GetAudioClipFile (MenuSoundKind.ui_tank_option);
		SoundManager.instance.PlayAudioClip (tankOption);
    }
    public void OnClickPrevTurretBase() {
        var intSelection = (int) tankModel.turretBaseKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankTurretBaseKind)).Length - 1;
        tankModel.turretBaseKind = (TankTurretBaseKind) intSelection;
        tankModel.UpdateAvatar();
		GetAudioClipFile (MenuSoundKind.ui_tank_option);
		SoundManager.instance.PlayAudioClip (tankOption);
    }

    public void OnClickNextBase() {
        var intSelection = (int) tankModel.tankBaseKind + 1;
        if (intSelection >= Enum.GetValues(typeof(TankBaseKind)).Length) intSelection = 0;
        tankModel.tankBaseKind = (TankBaseKind) intSelection;
        tankModel.UpdateAvatar();
		GetAudioClipFile (MenuSoundKind.ui_tank_option);
		SoundManager.instance.PlayAudioClip (tankOption);
    }
    public void OnClickPrevBase() {
        var intSelection = (int) tankModel.tankBaseKind - 1;
        if (intSelection <= 0) intSelection = Enum.GetValues(typeof(TankBaseKind)).Length - 1;
        tankModel.tankBaseKind = (TankBaseKind) intSelection;
        tankModel.UpdateAvatar();
		GetAudioClipFile (MenuSoundKind.ui_tank_option);
		SoundManager.instance.PlayAudioClip (tankOption);
    }

    public void OnClickRotateRight() {
        tankModel.tankRotation -= rotationSpeed;
		GetAudioClipFile (MenuSoundKind.ui_tank_rotate);
		SoundManager.instance.PlayAudioClip(tankRotate);
    }
    public void OnClickRotateLeft() {
        tankModel.tankRotation += rotationSpeed;
		GetAudioClipFile (MenuSoundKind.ui_tank_rotate);
		SoundManager.instance.PlayAudioClip(tankRotate);
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
        tankModel.gameObject.SetActive(false);
    }

    public void OnClickAccept() {
        if (linkedPlayer != null) {
            linkedPlayer.tankBaseKind = tankModel.tankBaseKind;
            linkedPlayer.turretBaseKind = tankModel.turretBaseKind;
            linkedPlayer.turretKind = tankModel.turretKind;
            linkedPlayer.hatKind = tankModel.hatKind;
        }
        // change back to lobby
        var manager = SingedLobbyManager.s_singleton;
        if (manager != null) {
            manager.ChangeTo(manager.lobbyPanel.gameObject, () => { manager.StopHostCallback(); });
        }
        // disassociate linked player
        linkedPlayer = null;
        // turn off game model
        tankModel.gameObject.SetActive(false);
    }


}
