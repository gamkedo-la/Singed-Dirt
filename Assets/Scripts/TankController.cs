using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


public class TankController : NetworkBehaviour {

	// Public
	public GameObject modelPrefab;
	public TankModel model;

	public float shotPower = 30.0f;
	public float shotPowerModifier = 10.0f;

	public float rotationSpeedVertical = 5.0f;
	public float rotationSpeedHorizontal = 5.0f;

	public Transform passiveCameraSource {
		get {
			if (model != null) {
				return model.passiveCameraSource;
			} else {
				return transform;
			}
		}
	}

	public Transform chaseCameraSource {
		get {
			if (model != null) {
				return model.chaseCameraSource;
			} else {
				return transform;
			}
		}
	}

	public Rigidbody rb;
	public ProjectileKind selectedShot;

	// FIXME: remove from player controller
	List<Transform> spawnPoints;
	Transform spawnPoint;

	bool togglePowerInputAmount;
	float savedPowerModifier;

	// state management variables
	bool hasRegistered = false; 	// am I registered to turn controller

	[SyncVar(hook="OnChangeControl")]
	public bool hasControl=false;

	[SyncVar]
	public int playerIndex = -1;

	[SyncVar]
	public string playerName = "";

	[SyncVar]
    public TankBaseKind tankBaseKind = TankBaseKind.standard;
	[SyncVar]
    public TankTurretBaseKind turretBaseKind = TankTurretBaseKind.standard;
	[SyncVar]
    public TankTurretKind turretKind = TankTurretKind.standard;
	[SyncVar]
    public TankHatKind hatKind = TankHatKind.sunBlue;
    private float minTurretElevation = 0.00f;
    private float maxTurretElevation = 70.0f;

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

	void Awake() {
        Debug.Log("TankController Awake: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
		// lookup/cache required components
		rb = GetComponent<Rigidbody> ();
		CreateModel();
	}

	void CreateModel() {
		// upon start up ... instantiate the model
		var modelGo = (GameObject) GameObject.Instantiate (
			modelPrefab,
			transform.position,
			Quaternion.identity,
			this.transform
		);
		model = modelGo.GetComponent<TankModel>();
		// add two child network transforms
		gameObject.SetActive(false);

		var netChild = gameObject.AddComponent<NetworkTransformChild>();
		netChild.target = modelGo.transform;

		netChild = gameObject.AddComponent<NetworkTransformChild>();
		netChild.target = model.turret.transform;

		gameObject.SetActive(true);

		// manually set center of mass for tank
		rb.centerOfMass = model.centerOfMass.position;
	}

	void PlaceOnGround() {
		// place tank model on ground
		if (Terrain.activeTerrain != null) {
			Vector3 fixedSpot = transform.position;
			fixedSpot.y = Terrain.activeTerrain.SampleHeight(fixedSpot) + Terrain.activeTerrain.transform.position.y;
			transform.position = fixedSpot;
			Debug.Log("placing on ground @ " + fixedSpot);
		}
	}

	void Start() {
        Debug.Log("TankController Start: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);

		// copy state to model
        model.tankBaseKind = tankBaseKind;
        model.turretBaseKind = turretBaseKind;
        model.turretKind = turretKind;
        model.hatKind = hatKind;
		model.UpdateAvatar();
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
		PlaceOnGround();
		GameObject centerPoint = GameObject.Find ("MapCenterLookAt");

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

	// Update is called once per frame
	void Update () {
		// register to turn manager (if required)
		// applies to all copies of player on the server
		if (!hasRegistered) {
			if (!isServer) {
				Debug.Log("playerIndex: " + playerIndex);
			}
			if (isServer) {
				ServerRegisterToManager();
			// NOTE: wait to register client until playerIndex has been assignedj
			} else if (playerIndex != -1) {
				ClientRegisterToManager();
			}
		}
	}

    // ------------------------------------------------------
    // CLIENT-ONLY METHODS

	/// <summary>
	/// Executed on the client
	/// Register this tank to turn manager
	/// </summary>
	void ClientRegisterToManager() {
		Debug.Log("ClientRegisterToManager for " + this.name);
		if (isServer) return;

		// ensure we haven't already registered
		if (hasRegistered) return;

		// register
		if (TurnManager.singleton != null) {
			hasRegistered = TurnManager.singleton.ClientRegisterPlayer(this);
		}
	}

    // ------------------------------------------------------
    // SERVER-ONLY METHODS

	/// <summary>
	/// Executed on the server
	/// Assign index to tank... this same index will be used on both client and server and is synced when assigned
	/// </summary>
	public void ServerAssignIndex(int index) {
		if (!isServer) return;
		Debug.Log("assigning index " + index + " to " + name);
		playerIndex = index;
	}

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
		Debug.Log("ServerRegisterToManager, manager: " + TurnManager.singleton);
		// this is a server function
		if (!isServer) return;

		// ensure we haven't already registered
		if (hasRegistered) return;

		// register
		if (TurnManager.singleton != null) {
			hasRegistered = TurnManager.singleton.ServerRegisterPlayer(this);
		}
	}

	/// <summary>
	/// Callback executed on the client when the hasControl variable changes
	/// Fire selected projectile if current player controller is in proper state.
	/// </summary>
	void OnChangeControl(bool currentHasControl) {
		Debug.Log("OnChangeControl called for " + this.name + " with isServer: " + isServer + " hasControl: " + currentHasControl);
		// only apply change control to local player
		if (!isLocalPlayer) return;
		if (currentHasControl == hasControl) return;
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
		// disable tank physics
		rb.isKinematic = true;

		// line up and take the shot
        yield return StartCoroutine(AimStateEngine());

		// relinquish control
		Debug.Log("ShootStateEngine release control for " + this.name);
		CmdReleaseControl();

		// re-enable tank physics
		rb.isKinematic = false;

	}

	/// <summary>
	/// This is the state-engine driving the aim/power/shot control for the tank.
	/// Stay in this state until a shot is fired.
	/// NOTE: ensure that all yield calls are using next of frame (yield return null) to ensure proper input handling
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
			var numShots = System.Enum.GetValues(typeof(ProjectileKind)).Length;
			int shotInt = (int) selectedShot;
			if (Input.GetKeyDown (KeyCode.Comma)) {
				shotInt--;
				if (shotInt < 0) {
					shotInt = numShots - 1;
				}
				selectedShot = (ProjectileKind)shotInt;
				Debug.Log ("getting smaller: " + shotInt);
			}
			if (Input.GetKeyDown (KeyCode.Period)) {
				shotInt++;
				if (shotInt >= numShots) {
					shotInt = 0;
				}
				selectedShot = (ProjectileKind)shotInt;
				Debug.Log ("getting larger: " + shotInt);
			}

			// Shoot already ... when shot is fired, finish this coroutine;
			if (Input.GetKeyDown (KeyCode.Space)) {
				Debug.Log("space is down, calling CmdFire");
				CmdFire(shotPower, selectedShot);
				yield break;
			}

			if (model != null) {
				model.tankRotation += Input.GetAxis ("Horizontal") * Time.deltaTime * rotationSpeedVertical;
				model.turretElevation += Input.GetAxis ("Vertical") * Time.deltaTime * rotationSpeedHorizontal;
				model.turretElevation = Mathf.Clamp(model.turretElevation, minTurretElevation, maxTurretElevation);
			}

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
	void CmdFire(float shotPower, ProjectileKind projectiledKind) {
		Debug.Log("CmdFire for " + name + " hasControl: " + hasControl);
		// controller state-based authorization check
		if (!hasControl) {
			Debug.Log("nope");
			return;
		}

		// instantiate from prefab
		var prefab = PrefabRegistry.singleton.GetPrefab<ProjectileKind>(projectiledKind);
		var liveProjectile = (GameObject)GameObject.Instantiate (
			prefab,
			model.shotSource.position,
			model.shotSource.rotation
		);
		liveProjectile.name = name + "Projectile";
		liveProjectile.layer = gameObject.layer;

		// set initial velocity/force
		liveProjectile.GetComponent<Rigidbody>().AddForce(model.shotSource.forward * shotPower);

		// set network spawn
		NetworkServer.Spawn (liveProjectile);

		// update manager
		if (TurnManager.singleton != null) TurnManager.singleton.ServerHandleShotFired(this, liveProjectile);
	}

	/// <summary>
	/// Called from the client, executed on the server
	/// release control
	/// </summary>
	[Command]
	void CmdReleaseControl() {
		Debug.Log("CmdReleaseControl for " + name);
		/// release control
		hasControl = false;

		// tell the manager
		//manager.ServerReleaseTank(this);
		// FIXME
	}

}
