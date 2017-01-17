using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankController : MonoBehaviour {

	// Public
	public GameObject projectilePrefab;
	public Transform shotSource;
	public float shotPower = 30.0f;
	public float shotPowerModifier = 10.0f;

	public Transform turret;
	public Transform playerCameraSpot;

	// Private
	Rigidbody rb;
	HorizontalTurretMover horizontalTurret;
	VerticalTurretMover verticalTurret;
	int playerNumber;
	List<Transform> spawnPoints;
	Transform spawnPoint;
	bool togglePowerInputAmount;
	float savedPowerModifier;
	bool tankReadyToShoot = true;

	// Hidden Public
	[HideInInspector]  // This makes the next variable following this to be public but not show up in the inspector.
	public GameObject liveProjectile;

	// Use this for initialization
	void Awake () {
		togglePowerInputAmount = false;
		savedPowerModifier = shotPowerModifier;
		spawnPoints = new List<Transform> ();
		horizontalTurret = GetComponentInChildren<HorizontalTurretMover> ();
		verticalTurret = GetComponentInChildren<VerticalTurretMover> ();
		if (name == "PlayerOne") {
			GameObject tempGO = GameObject.Find ("PlayerOneSpawnPoints");
			if (tempGO != null) {
				foreach (Transform child in tempGO.transform) {
					spawnPoints.Add (child);
				}
			} else {
				Debug.Log ("Can't find the spawn points for player one.");
			}
		} else if (name == "PlayerTwo") {
			GameObject tempGO = GameObject.Find ("PlayerTwoSpawnPoints");
			if (tempGO != null) {
				foreach (Transform child in tempGO.transform) {
					spawnPoints.Add (child);
				}
			} else {
				Debug.Log ("Can't find the spawn points for player two.");
			}
		}
		spawnPoint = spawnPoints [Random.Range (0, spawnPoints.Count)];
		transform.position = spawnPoint.position;
	}

	public float HorizAngle(){
		return horizontalTurret.aimHorizontal;
	}

	public float VertAngle(){
		return verticalTurret.aimVertical;
	}

	public float ShotPower(){
		return shotPower;
	}

	public void InputAdjustPower(float specificPower){
		shotPower = specificPower;
	}

	public void ReadyToShoot(){
		tankReadyToShoot = true;
	}

	public void DialAdjustPower(int offset){
		bool isNegative = offset < 0;
		float tweakAmt = 0.0f;
		switch (Mathf.FloorToInt(Mathf.Abs (offset))) {
		case 1:
			tweakAmt = 5.0f;
			break;
		case 2:
			tweakAmt = 60.0f;
			break;
		case 3:
			tweakAmt = 200.0f;
			break;
		}
		if (isNegative) {
			shotPower -= tweakAmt;
		} else {
			shotPower += tweakAmt;
		}
	}

	public void SleepControls(bool toSleep){
		bool isEnabled = (toSleep == false);
		Debug.Log ("isEnabled = " + isEnabled);
		horizontalTurret.enabled = isEnabled;
		verticalTurret.enabled = isEnabled;
		this.enabled = isEnabled;
	}

	void OnTriggerEnter(Collider other){
		ProjectileController tempPC = other.GetComponent<ProjectileController> ();
		if (tempPC != null) {
			if (tempPC.name != name + "Projectile") {
				TurnManager.instance.GameOverMan (true);
				Destroy (other);
				Destroy (gameObject);
			}

		}
	}
	
	// Update is called once per frame
	void Update () {
		if (TurnManager.instance.GetGameOverState () == false) {
			if (Input.GetKeyDown (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
				togglePowerInputAmount = true;
			}
			if (Input.GetKeyUp (KeyCode.LeftShift) || Input.GetKeyUp (KeyCode.RightShift)) {
				togglePowerInputAmount = false;
			}
			if (Input.GetKeyDown (KeyCode.Space) && tankReadyToShoot == true) {
				liveProjectile = (GameObject)GameObject.Instantiate (projectilePrefab);
				liveProjectile.name = name + "Projectile";
				liveProjectile.transform.position = shotSource.position;
				rb = liveProjectile.GetComponent<Rigidbody> ();
				rb.AddForce (shotSource.forward * shotPower);
				tankReadyToShoot = false;
			}
			if (togglePowerInputAmount == false) {
				if (Input.GetKey (KeyCode.LeftBracket)) {
					shotPower -= shotPowerModifier;
					if (shotPower <= 0.0f) {
						shotPower = 0.0f;
					}
				}

				if (Input.GetKey (KeyCode.RightBracket)) {
					shotPower += shotPowerModifier;
				}
			} else {
				if (Input.GetKeyDown (KeyCode.LeftBracket)) {
					shotPower -= shotPowerModifier;
					if (shotPower <= 0.0f) {
						shotPower = 0.0f;
					}
				}

				if (Input.GetKeyDown (KeyCode.RightBracket)) {
					shotPower += shotPowerModifier;
				}
			}

		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}

		transform.rotation = Quaternion.AngleAxis (horizontalTurret.aimHorizontal, Vector3.up);

		// These two lines stay together
		turret.rotation = Quaternion.AngleAxis (horizontalTurret.aimHorizontal, Vector3.up) *
		Quaternion.AngleAxis (verticalTurret.aimVertical, Vector3.right);

		// Camera look at code
		playerCameraSpot.position = transform.position - transform.forward * 2.0f + Vector3.up * 1.5f;
		playerCameraSpot.LookAt (transform.position + transform.forward * 15.0f);
	}
}
