using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TurnManager : NetworkBehaviour {

	// Public variables

	public static TurnManager instance;
	public Text hud;
	public Text gameOverText;
	public List<TankController> tanks;
	public InputField powerValue;
	public Transform healthBar;

	// the list of all tanks, mapping their tank ID to tank controller
	// NOTE: this mapping is maintained on both client and server
	public Dictionary<int, TankController> tankRegistry;

	// number of players in game
	public int expectedPlayers = 2;

	// the list of tanks currently active in the round... as tanks die, they are removed from this list
	public List<int> activeTanks;

	// Private variables

	int nextTankId = 1;

	CameraController camController;
	TankController activeTank;
	TankController localTank;
	float horizontalTurret;
	float verticalTurret;
	float shotPower;
	int tankHitPoints;
	int tankTurnIndex = 0;

	[SyncVar]
	bool gameOverState = false;

	GameObject liveProjectile = null;
	GameObject liveExplosion = null;

	void Awake(){
		instance = this;
		gameOverText.enabled = false;
		tankRegistry = new Dictionary<int, TankController>();
		activeTanks = new List<int>();
	}

	public void GameOverMan(bool isGameOver){
		gameOverState = isGameOver;
	}

	public bool GetGameOverState(){
		return gameOverState;
	}

	// Use this for initialization
	void Start () {
		if (isServer) {
			StartCoroutine(ServerLoop());
		}
		camController = Camera.main.GetComponent<CameraController> ();
	}

	void SetActiveTank(TankController tank){
		Debug.Log("Activating " + tank.name);
		activeTank = tank;
		tank.ServerEnableControl();
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

	void GetLocalTankHud(){
		if (localTank != null) {
			horizontalTurret = localTank.HorizAngle ();
			verticalTurret = localTank.VertAngle ();
			shotPower = localTank.ShotPower ();

			// calculate health
			var health = localTank.GetComponent<Health>();
			if (health != null) {
				tankHitPoints = health.health;
				var healthScale = (float) health.health/(float)Health.maxHealth;
				healthBar.localScale = new Vector3(healthScale,1f,1f);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		GetLocalTankHud ();
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

	int[] GetTurnOrder(Dictionary<int, TankController> tanks) {
		var keys = new List<int>(tanks.Keys);
		var turnOrder = new List<int>();
		// randomize turn order based on given set of tanks
		while (keys.Count > 0) {
			var nextIndex = UnityEngine.Random.Range(0, keys.Count);
			turnOrder.Add(keys[nextIndex]);
			keys.RemoveAt(nextIndex);
		}
		return turnOrder.ToArray();
	}

    // ------------------------------------------------------
    // CLIENT-ONLY METHODS
	public void ClientRegisterPlayer(TankController player) {
		if (player.isLocalPlayer) {
			localTank = player;
		}
		Debug.Log("ClientRegisterPlayer: " + player);
		tankRegistry[player.playerIndex] = player;
	}

    // ------------------------------------------------------
    // SERVER-ONLY METHODS
	public void ServerRegisterPlayer(TankController player) {
		if (!isServer) return;
		if (player.isLocalPlayer) {
			localTank = player;
		}
		Debug.Log("ServerRegisterPlayer: " + player);

		// assign tank index
		var newTankIndex = nextTankId;
		nextTankId++;
		tankRegistry[newTankIndex] = player;

		// server assigns tank index and acknowledges registration...
		player.ServerAssignIndex(newTankIndex);
	}

	public void ServerHandleShotFired(TankController player, GameObject projectileGO) {
		Debug.Log("ServerHandleShotFired: " + projectileGO);
		if (!isServer) return;
		liveProjectile = projectileGO;
	}

	public void ServerHandleExplosion(GameObject explosionGO) {
		Debug.Log("ServerHandleExplosion: " + explosionGO);
		if (!isServer) return;
		liveExplosion = explosionGO;
	}

	public void ServerHandleTankDeath(GameObject playerGO) {
		Debug.Log("ServerHandleTankDeath: " + playerGO);
		var player = playerGO.GetComponent<TankController>();
		if (player != null) {
			// remove player from set of active tanks
			activeTanks.Remove(player.playerIndex);
			Debug.Log("removing index: " + player.playerIndex + " new active tanks -> " + String.Join(",", activeTanks.Select(v=>v.ToString()).ToArray()));
		}
	}

    // ------------------------------------------------------
    // SERVER->CLIENT METHODS

	/// <summary>
	/// Called from the server, executed on the client
	/// Start the main client loop
	/// </summary>
	[ClientRpc]
	void RpcStart() {
		Debug.Log("starting game on client");
		StartCoroutine(ClientLoop());
	}

	/// <summary>
	/// Called from the server, executed on the client
	/// Set the view to the local tank
	/// </summary>
	[ClientRpc]
	void RpcViewLocalTank() {
		if (localTank != null) {
			Debug.Log("setting camera view to local: " + localTank.name);
			camController.WatchPlayer(localTank);
			//camController.SetPlayerCameraFocus(localTank);
		}
	}

	[ClientRpc]
	void RpcViewShot(GameObject playerGO, GameObject projectileGO, bool localOnly) {
		if (playerGO.GetComponent<TankController>().isLocalPlayer || !localOnly) {
			//camController.ShakeCamera(0.8f, 0.8f);
			camController.WatchProjectile(projectileGO);
		}
	}

	[ClientRpc]
	void RpcViewExplosion(GameObject playerGO, GameObject explosionGO, bool localOnly) {
		Debug.Log("RpcViewExplosion: isLocalPlayer: " + playerGO.GetComponent<TankController>().isLocalPlayer);
		if (playerGO.GetComponent<TankController>().isLocalPlayer || !localOnly) {
			camController.ShakeCamera(2.0f, 0.9f);
			camController.WatchExplosion(explosionGO);
		}
	}

    // ------------------------------------------------------
    // STATE ENGINES
	/// <summary>
	/// This is the main server loop
	/// </summary>
	IEnumerator ServerLoop() {
		Debug.Log("starting ServerLoop");
		// wait for players to join
        yield return StartCoroutine(ListenForTanks());

		// build world
		yield return StartCoroutine(BuildWorld());

		// adjust camera
		RpcViewLocalTank();

		// start the game on client
		RpcStart();

		// start the game
        yield return StartCoroutine(PlayRound());
		// FIXME: need to rework game win/loss logic
		gameOverState = true;
		Debug.Log("finishing ServerLoop");
	}

	/// <summary>
	/// This is the main client loop
	/// </summary>
	IEnumerator ClientLoop() {
		Debug.Log("starting ClientLoop");
		// wait for players to join
        yield return StartCoroutine(ListenForTanks());

		camController.WatchPlayer(localTank);
		//camController.SetPlayerCameraFocus(localTank);
	}

	/// <summary>
	/// Build out the world
	/// </summary>
	IEnumerator BuildWorld() {
		// spawn terrain
		var formTerrain = GetComponent<FormTerrain>();
		formTerrain.ServerGenerate();
		yield return null;
	}

	IEnumerator ListenForTanks(){
		// wait for new players to join, up to expected number of players
		while (tankRegistry.Count < expectedPlayers) {
			yield return null;
		}

		// we have expected number of tanks... initialize each tank
		foreach(int tankId in tankRegistry.Keys) {
			tankRegistry[tankId].name = "Player " + tankId.ToString();
			tankRegistry[tankId].SetupTank();
		}

		Debug.Log ("Tanks reporting for duty!");
	}

	IEnumerator PlayRound() {
		Debug.Log ("Starting the game!!!");

		// add current tanks to the active tank list
		activeTanks = new List<int>(tankRegistry.Keys);

		// set starting camera positions for each player (this is done client side)
		RpcViewLocalTank();

		// determine turn order
		var turnOrder = GetTurnOrder(tankRegistry);
		Debug.Log("turn order is -> " + String.Join(",", turnOrder.Select(v=>v.ToString()).ToArray()));
		var currentIndex = 0;
		yield return null;

		// continue to play the round while at least two tanks are active
		while (activeTanks.Count >= 2) {
			// determine next tank, advance current Index
			var nextTankId = turnOrder[(currentIndex++)%turnOrder.Length];

			// validate tank is active
			if (!activeTanks.Contains(nextTankId)) {
				Debug.Log("skipping inactive tank: " + tankRegistry[nextTankId].name);
				continue;
			}

			// select active tank and take turn
			yield return StartCoroutine(TakeTankTurn(tankRegistry[nextTankId]));

		}

		Debug.Log("Round is over, winner is " + tankRegistry[activeTanks[0]].name);
	}

	IEnumerator TakeTankTurn(TankController tank) {
		Debug.Log("taking turn for " + tank.name);
		// activate the tank
		SetActiveTank(tank);

		// wait for shot fired by this tank
		while (tank.hasControl) {
			yield return null;
		}

		// follow tank projectile
		if (liveProjectile != null) {
			Debug.Log("live projectile detected");
			// update local camera to watch live projectile
			RpcViewShot(tank.gameObject, liveProjectile, true);
		}
		// wait until the projectile is destroyed
		while (liveProjectile != null) {
			yield return null;
		}

		// wait for explosion
		if (liveExplosion != null) {
			Debug.Log("live explosion detected");
			// update local camera to watch live explosion
			RpcViewExplosion(tank.gameObject, liveExplosion, true);
		}
		// wait until the explosion is destroyed
		while (liveExplosion != null) {
			yield return null;
		}

		// reset view to local tank view
		RpcViewLocalTank();
	}


	public static TurnManager GetGameManager() {
		// find manager GO and manager script
		GameObject managerGO = GameObject.Find("GameManager");
		if (managerGO == null) {
			Debug.Log("server registration failed, can't find game manager");
			return null;
		}
		var manager = managerGO.GetComponent<TurnManager>();
		if (manager == null) {
			Debug.Log("server registration failed, can't find game manager script");
			return null;
		}
		return manager;
	}


}
