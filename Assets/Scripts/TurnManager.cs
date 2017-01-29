using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour {

	// Public variables

	public static TurnManager instance;
	public Text hud;
	public Text gameOverText;
	public List<TankController> tanks;
	public InputField powerValue;

	// Private variables

	CameraController camController;
	TankController activeTank;
	float horizontalTurret;
	float verticalTurret;
	float shotPower;
	int tankHitPoints;
	int tankTurnIndex = 0;
	bool gameOverState = false;


	void Awake(){
		instance = this;
		gameOverText.enabled = false;

	}

	public void GameOverMan(bool isGameOver){
		gameOverState = isGameOver;
	}

	public bool GetGameOverState(){
		return gameOverState;
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
		activeTank.ReadyToShoot ();
		Debug.Log ("hitpoints val is " + activeTank.HitPoints());
	}

	public TankController GetActiveTank(){
		return activeTank;
	}

	public void TellTankAdjustPower(int power){
		activeTank.DialAdjustPower (power);
	}

	public void TellTankSpecificPower(Text power){
		Debug.Log("power is " + power.text);
		activeTank.InputAdjustPower (float.Parse(power.text));
	}

	public void CycleActiveTank(){
		tankTurnIndex++;
		if (tankTurnIndex >= tanks.Count) {
			tankTurnIndex = 0;
			Debug.Log ("Everyone had a turn.");
		}
		if (gameOverState == false) {
			SetActiveTank (tanks [tankTurnIndex]);
		}

	}

	void GetCurrentTankHud(){
		horizontalTurret = activeTank.HorizAngle ();
		verticalTurret = activeTank.VertAngle ();
		shotPower = activeTank.ShotPower ();
		tankHitPoints = activeTank.HitPoints ();
	}
	
	// Update is called once per frame
	void Update () {
		GetCurrentTankHud ();
		hud.text = 
			"Heading: " + horizontalTurret + "degrees\n" +
			"Elevation: " + verticalTurret + " degrees\n" +
			"Muzzle Velocity: " + shotPower + "m/s\n" +
			"HitPoints: " + tankHitPoints;
		powerValue.text = "" + shotPower;

		if (gameOverState == true) {
			gameOverText.enabled = true;
		} else {
			gameOverText.enabled = false;
		}
	}
}
