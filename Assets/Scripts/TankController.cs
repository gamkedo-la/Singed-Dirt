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

	public Rigidbody rb;

	// Private

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

	// state management variables
	bool hasRegistered = false; 	// am I registered to turn controller

	[SyncVar(hook="OnChangeControl")]
	bool hasControl=false;

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

		// lookup/cache required components
		rb = GetComponent<Rigidbody> ();
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

	void OnTriggerEnter(Collider other){

	}

	// Update is called once per frame
	void Update () {
		// register to turn manager (if required and if server)
		// applies to all copies of player on the server
		if (!hasRegistered && isServer) {
			ServerRegisterToManager();
		}

		/*
		if (TurnManager.instance == null) {
			return;
		}

		// multiplayer instantiates multiple player game objects... only control the local player
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
				CmdFire(shotPower);
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
		*/
	}

    // ------------------------------------------------------
    // SERVER-ONLY METHODS

	/// <summary>
	/// Executed on the server
	/// Enable tank controls
	/// </summary>
	public void ServerEnableControl() {
		// this is a server function
		if (!isServer) return;

		// enable controls, note this is a SyncVar meaning the value gets synced from server to client
		hasControl = true;
	}

	/// <summary>
	/// Executed on the server
	/// Disable tank controls
	/// </summary>
	public void ServerDisableControl() {
		// this is a server function
		if (!isServer) return;

		// enable controls, note this is a SyncVar meaning the value gets synced from server to client
		hasControl = false;
	}

	/// <summary>
	/// Executed on the server
	/// Register this tank to turn manager
	/// </summary>
	void ServerRegisterToManager() {
		// this is a server function
		if (!isServer) return;

		// ensure we haven't already registered
		if (hasRegistered) return;

		// find manager GO and manager script
		GameObject managerGO = GameObject.Find("GameManager");
		if (managerGO == null) {
			Debug.Log("server registration failed, can't find game manager");
			return;
		}
		var manager = managerGO.GetComponent<TurnManager>();
		if (manager == null) {
			Debug.Log("server registration failed, can't find game manager script");
			return;
		}

		// register
		manager.RegisterPlayer(this);
		hasRegistered = true;
	}

	/// <summary>
	/// Callback executed on the client when the hasControl variable changes
	/// Fire selected projectile if current player controller is in proper state.
	/// </summary>
	void OnChangeControl(bool currentHasControl) {
		Debug.Log("OnChangeControl called for " + this.name + " with isServer: " + isServer);
		// only apply change control to local player
		if (!isLocalPlayer) return;
		hasControl = currentHasControl;

		// disable -> enable
		if (currentHasControl) {
	        StartCoroutine(ShootStateEngine());

		// enable -> disable
		} else {

		}
	}

    // ------------------------------------------------------
    // STATE ENGINES
	/// <summary>
	/// This is the state-engine driving tank operations while controls are active
	/// </summary>
	IEnumerator ShootStateEngine() {
		Debug.Log("ShootStateEngine called for " + this.name + " with isServer: " + isServer);
		// disable physics
		rb.isKinematic = true;

		// line up and take the shot
        yield return StartCoroutine(AimStateEngine());

		// shot is in the air

		// target hit

		// re-enable physics
		rb.isKinematic = false;

		// relinquish control
		Debug.Log("ShootStateEngine release control for " + this.name);
		CmdReleaseControl();
	}

	/// <summary>
	/// This is the state-engine driving the aim/power/shot control for the tank.
	/// Stay in this state until a shot is fired.
	/// NOTE: ensure that all yield calls are using next of frame (yield null) to ensure proper input handling
	/// </summary>
	IEnumerator AimStateEngine() {
		Debug.Log("AimStateEngine called for " + this.name + " with isServer: " + isServer + " and hasControl: " + hasControl);
		// continue while we have control
		while (hasControl) {
			if (Input.GetKeyDown (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
				togglePowerInputAmount = true;
			}
			if (Input.GetKeyUp (KeyCode.LeftShift) || Input.GetKeyUp (KeyCode.RightShift)) {
				togglePowerInputAmount = false;
			}
			if (togglePowerInputAmount == false) {
				if (Input.GetKey (KeyCode.LeftBracket)) {
					shotPower -= shotPowerModifier;
					if (shotPower <= 0.0f) {
						shotPower = 0.0f;
					}
				}

				if (Input.GetKey (KeyCode.RightBracket)) {
					Debug.Log("right bracket");
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
					Debug.Log("right bracket");
					shotPower += shotPowerModifier;
				}
			}

			// Shoot already ... when shot is fired, finish this coroutine;
			if (Input.GetKeyDown (KeyCode.Space)) {
				CmdFire(shotPower);
				yield break;
			}

			aimVertical += Input.GetAxis ("Vertical") * Time.deltaTime * rotationSpeedVertical;
			aimHorizontal += Input.GetAxis ("Horizontal") * Time.deltaTime * rotationSpeedHorizontal;

			transform.rotation = Quaternion.AngleAxis (aimHorizontal, Vector3.up);
			//turret.rotation = Quaternion.AngleAxis (aimVertical, Vector3.right);
			turret.rotation = Quaternion.AngleAxis (aimHorizontal, Vector3.up) *
				Quaternion.AngleAxis (aimVertical, Vector3.right);

			// These two lines stay together
			//turret.rotation = Quaternion.AngleAxis (aimHorizontal, Vector3.up) *
			//Quaternion.AngleAxis (aimVertical, Vector3.right);

			// continue on next frame
			yield return null;
		}
	}

    // ------------------------------------------------------
    // CLIENT->SERVER METHODS

	/// <summary>
	/// Called from the client, executed on the server
	/// Fire selected projectile if current player controller is in proper state.
	/// </summary>
	[Command]
	void CmdFire(float shotPower) {
		// controller state-based authorization check
		if (!hasControl) {
			Debug.Log("nope");
			return;
		}

		// instantiate from prefab
		liveProjectile = (GameObject)GameObject.Instantiate (
			projectilePrefab,
			shotSource.position,
			shotSource.rotation
		);
		liveProjectile.name = name + "Projectile";

		// set initial velocity/force
		liveProjectile.GetComponent<Rigidbody>().AddForce(shotSource.forward * shotPower);

		// set network spawn
		NetworkServer.Spawn (liveProjectile);
	}

	[Command]
	void CmdReleaseControl() {
		/// Called from the client, executed on the server
		/// release control
		hasControl = false;
	}

}
