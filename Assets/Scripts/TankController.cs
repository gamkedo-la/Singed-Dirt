using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


public class TankController : NetworkBehaviour {

	// Public
	public GameObject projectilePrefab;
	public Transform shotSource;
	public float shotPower = 30.0f;
	public float shotPowerModifier = 10.0f;
	public int hitPoints = 100;

	public float rotationSpeedVertical = 5.0f;
	public float aimVertical = 45.0f;
	public float rotationSpeedHorizontal = 5.0f;
	public float aimHorizontal = 45.0f;

	public Transform turret;
	public Transform playerCameraSpot;

	public GameObject[] upperMeshes;
	public GameObject[] middleMeshes;
	public GameObject[] lowerMeshes;

	// Private
	Rigidbody rb;
	int playerNumber;
	List<Transform> spawnPoints;
	Transform spawnPoint;
	bool togglePowerInputAmount;
	float savedPowerModifier;
	AvatarSetup tankAvatarScript;
	int[] playerMeshSetup;
	int lowerMeshNum;
	int middleMeshNum;
	int upperMeshNum;
	GameObject turretSpot;

	// Hidden Public
	[HideInInspector]  // This makes the next variable following this to be public but not show up in the inspector.
	public GameObject liveProjectile;

	void FindSpawnPointAndAddToList(string playerName)
	{
		GameObject tempGO = GameObject.Find (playerName + "SpawnPoints");
		if (tempGO != null) {
			foreach (Transform child in tempGO.transform) {
				spawnPoints.Add (child);
			}
		} else {
			Debug.Log ("Can't find the spawn points for " + playerName + ".");
		}
	}

	// Use this for initialization
	public void SetupTank () {
		togglePowerInputAmount = false;
		savedPowerModifier = shotPowerModifier;
		spawnPoints = new List<Transform> ();
		Debug.Log ("Name is " + name + " and is local player " + isLocalPlayer);
		FindSpawnPointAndAddToList (name);
		spawnPoint = spawnPoints [Random.Range (0, spawnPoints.Count)];
		transform.position = spawnPoint.position;
		lowerMeshNum = PlayerPrefs.GetInt (name + "lowerMeshNum");
		middleMeshNum = PlayerPrefs.GetInt (name + "middleMeshNum");
		upperMeshNum = PlayerPrefs.GetInt (name + "upperMeshNum");
		Debug.Log ("meshes are " + lowerMeshNum + " " + middleMeshNum + " " + upperMeshNum);
		tankAvatarScript = GetComponent<AvatarSetup> ();
		tankAvatarScript.SetActiveMeshes (lowerMeshNum, middleMeshNum, upperMeshNum);
		tankAvatarScript.updateAvatar ();
		GameObject centerPoint = GameObject.Find ("MapCenterLookAt");
		aimHorizontal = Mathf.Atan2 (centerPoint.transform.position.z - transform.position.z,
			centerPoint.transform.position.x - transform.position.x) * Mathf.Rad2Deg;

		turretSpot = new GameObject ("turretPos");
		turretSpot.transform.position = turret.transform.position;
		turretSpot.transform.rotation = turret.transform.rotation;
		turretSpot.transform.SetParent (transform);
		turret.SetParent (null);
	}

	public float HorizAngle(){
		return aimHorizontal;
	}

	public float VertAngle(){
		return aimVertical;
	}

	public float ShotPower(){
		return shotPower;
	}

	public int HitPoints(){
		return hitPoints;
	}

	public void InputAdjustPower(float specificPower){
		Debug.Log ("recieved " + specificPower);
		shotPower = specificPower;
	}


	public void SetPlayerModels(){
		
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
		this.enabled = isEnabled;
	}

	void OnTriggerEnter(Collider other){
		// NOTE:  Currently, this trigger is entered whenever the cannonball collider
		// intersects the tank's collider.  At the moment, the tank's collider is 'oversized'.
		// Some refinement is needed, to make sure the trigger occurs only when the cannonball explodes
		// Right now, a collision trigger can occur if the tank is situated off the ground (e.g., on a rock), and the
		// cannonball passes through the collider's box before it hits the ground

		// We can also leave the collider small, and make proximity damage is based purely on
		// distance thresholds defined somewhere (perhaps the TankController class).  This approach would require 
		// separate distance tests to see which tanks, if any, the cannonball landed near.

		ProjectileController tempPC = other.GetComponent<ProjectileController> ();
		if (tempPC != null) {
			if (tempPC.name != name + "Projectile" ) {
				Vector3 cannonballCenterToTankCenter = this.transform.position - other.transform.position;
				Debug.Log (string.Format ("tank position: {0}, cannonball position: {1}", this.transform.position, other.transform.position));
				Debug.Log (string.Format ("cannonballCenterToTankCenter: {0}", cannonballCenterToTankCenter));

				float hitDistToTankCenter = cannonballCenterToTankCenter.magnitude;
				Debug.Log ("Distance to tank center: " + hitDistToTankCenter);

				// NOTE: The damagePoints formula below is taken from an online quadratic regression calculator. The idea
				// was to plug in some values and come up with a damage computation formula.  The above formula yields:
				// direct hit (dist = 0m): 100 hit points
				// Hit dist 5m: about 25 hit points
				// hit dist 10m: about 1 hit point
				// The formula is based on a max proximity damage distance of 10m
				int damagePoints = (int) (1.23f * hitDistToTankCenter * hitDistToTankCenter - 22.203f * hitDistToTankCenter + 100.012f);
				hitPoints -= damagePoints;
				Debug.Log ("Damage done to " + name + ": " + damagePoints + ". Remaining: " + hitPoints);

				// Do shock displacement
				rb = GetComponent<Rigidbody> ();
				Vector3	displacementDirection = cannonballCenterToTankCenter.normalized;
				Debug.Log (string.Format ("Displacement stats: direction={0}, magnitude={1}", displacementDirection, damagePoints));
				rb.AddForce (rb.mass * (displacementDirection * damagePoints * 0.8f), ForceMode.Impulse);	// Force = mass * accel


				if (hitPoints <= 0) {
					Destroy (other);
					TurnManager.instance.GameOverMan (true);
					Destroy (gameObject);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (TurnManager.instance == null) {
			return;
		}
		if (isLocalPlayer == false) {
			return;
		}
		if (TurnManager.instance.GetGameOverState () == false) {
			if (Input.GetKeyDown (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
				togglePowerInputAmount = true;
			}
			if (Input.GetKeyUp (KeyCode.LeftShift) || Input.GetKeyUp (KeyCode.RightShift)) {
				togglePowerInputAmount = false;
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				if (this == TurnManager.instance.GetActiveTank ()) {
					liveProjectile = (GameObject)GameObject.Instantiate (projectilePrefab);
					NetworkServer.Spawn (liveProjectile);
					liveProjectile.name = name + "Projectile";
					liveProjectile.transform.position = shotSource.position;
					rb = liveProjectile.GetComponent<Rigidbody> ();
					rb.AddForce (shotSource.forward * shotPower);
				} else {
					// need to provide feedback that you tried to fire out of turn
					Debug.Log("nope");
				}
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

		if (TurnManager.instance.GetGameOverState () == false) {
			aimVertical += Input.GetAxis ("Vertical") * Time.deltaTime * rotationSpeedVertical;
			aimHorizontal += Input.GetAxis ("Horizontal") * Time.deltaTime * rotationSpeedHorizontal;
		}

		transform.rotation = Quaternion.AngleAxis (aimHorizontal, Vector3.up);

		// These two lines stay together
		turret.rotation = Quaternion.AngleAxis (aimHorizontal, Vector3.up) *
		Quaternion.AngleAxis (aimVertical, Vector3.right);

	}

	void LateUpdate(){
		if (turret == null || turretSpot == null) {
			return;
		}
		turret.transform.position = turretSpot.transform.position;

	}
}
