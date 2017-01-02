using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour {

	// Public variables

	public static TurnManager instance;
	public Text hud;
	public List<TankController> tanks;

	// Private variables

	CameraController camController;
	TankController activeTank;
	float horizontalTurret;
	float verticalTurret;
	float shotPower;
	int tankTurnIndex = 0;

	void Awake(){
		instance = this;
	}

	// Use this for initialization
	void Start () {
		TankController [] tankList = GameObject.FindObjectsOfType(typeof(TankController)) as TankController [];
		tanks = tankList.ToList ();
		Debug.Log ("Tank count: " + tanks.Count);
		camController = Camera.main.GetComponent<CameraController> ();
		SetActiveTank (tanks [tankTurnIndex]);
	}

	void SetActiveTank(TankController tank){
		activeTank = tank;
		camController.SetPlayerCameraFocus (tank);
		foreach (TankController eachTank in tanks) {
			eachTank.SleepControls(eachTank != activeTank);
		}
	}

	public void CycleActiveTank(){
		tankTurnIndex++;
		if (tankTurnIndex >= tanks.Count) {
			tankTurnIndex = 0;
			Debug.Log ("Everyone had a turn.");
		}
		SetActiveTank (tanks [tankTurnIndex]);
	}

	void GetCurrentTankHud(){
		horizontalTurret = activeTank.HorizAngle ();
		verticalTurret = activeTank.VertAngle ();
		shotPower = activeTank.ShotPower ();
	}
	
	// Update is called once per frame
	void Update () {
		GetCurrentTankHud ();
		hud.text = 
			"Heading: " + horizontalTurret + "degrees\n" +
			"Elevation: " + verticalTurret + " degrees\n" +
			"Muzzle Velocity: " + shotPower + "m/s";
	}
}
