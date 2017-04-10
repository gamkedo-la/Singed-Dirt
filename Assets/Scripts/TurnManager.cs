﻿using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TurnManager : NetworkBehaviour {

	// Public variables

	public static TurnManager singleton;
	// [SyncVar]
	public Text hud;
	public Text gameOverText;
	public InputField powerValue;
	public Transform healthBar;

	public GameObject[] selectedProjectileModels;
	public ProjectileKind selectedProjectile;

	// the list of all tanks, mapping their tank ID to tank controller
	// NOTE: this mapping is maintained on both client and server
	public Dictionary<int, TankController> tankRegistry;

	// number of players in game
	public int expectedPlayers = 2;

	// the list of tanks currently active in the round... as tanks die, they are removed from this list
	public List<int> activeTanks;

	// Private variables
	bool isReady = false;
	bool gameStarted = false;

	int nextTankId = 1;

	CameraController camController;
	TankController activeTank;
	TankController lastLocalTank;
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
        // Debug.Log("TurnManager Awake: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
		singleton = this;
		gameOverText.enabled = false;
		tankRegistry = new Dictionary<int, TankController>();
		activeTanks = new List<int>();
		selectedProjectile = ProjectileKind.acorn;
		camController = Camera.main.GetComponent<CameraController> ();
	}

	public void GameOverMan(bool isGameOver){
		gameOverState = isGameOver;
	}

	// function for use with buttons in HUD to increase/decrease tank powerValue
	public void TellTankAdjustPower(int power){
		activeTank.DialAdjustPower(power);
	}

	public bool GetGameOverState(){
		return gameOverState;
	}

	// Use this for initialization
	void Start () {
        // Debug.Log("TurnManager Start: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
		isReady = true;
	}

	void SetActiveTank(TankController tank){
		// Debug.Log("Activating " + tank.name);
		activeTank = tank;
		if (activeTank.isLocalPlayer) {
			lastLocalTank = activeTank;
		}
		
		tank.ServerEnableControl();
	}

	public TankController GetActiveTank(){
		return activeTank;
	}

	void GetLocalTankHud(){
		if (lastLocalTank != null){
			Debug.Log("Am I being called by network player (or is lastlocaltank a thing): " + lastLocalTank.name);	
		} else {
			Debug.Log("Last Local tank is null");
			lastLocalTank = GameObject.Find("TankSpawn2").GetComponent<TankController>();
		}
		
		if (lastLocalTank != null) {
			if (lastLocalTank.model != null) {
				horizontalTurret = lastLocalTank.model.tankRotation;
				verticalTurret = lastLocalTank.model.turretElevation;
				shotPower = lastLocalTank.shotPower;
				if(selectedProjectile != lastLocalTank.selectedShot){
					selectedProjectile = lastLocalTank.selectedShot;
					for(int i =0; i < selectedProjectileModels.Length;i++){
						selectedProjectileModels[i].SetActive((int)selectedProjectile == i);
					}

				}
			}

			// calculate health
			var health = lastLocalTank.GetComponent<Health>();
			if (health != null) {
				tankHitPoints = health.health;
				var healthScale = (float) health.health/(float)Health.maxHealth;
				healthBar.localScale = new Vector3(healthScale,1f,1f);
			}
		} else {
			horizontalTurret = -999.9f;
		}
	}

	// Update is called once per frame
	void Update () {
		if (!gameStarted && isServer) {
			gameStarted = true;
			StartCoroutine(ServerLoop());
		}

		// FIXME: this should get moved... not really related to turn management
		GetLocalTankHud ();
		hud.text =
			"Heading: " + horizontalTurret + "degrees\n" +
			"Elevation: " + verticalTurret + " degrees\n" +
			"Muzzle Velocity: " + shotPower + "m/s\n" +
			"HitPoints: " + tankHitPoints + "\n"; // + "m/s\n" +
			// "projectile: " + selectedProjectile;
		powerValue.text = "" + shotPower;

		if (gameOverState == true) {
			gameOverText.enabled = true;
		} else {
			gameOverText.enabled = false;
		}
		if (Input.GetKeyDown (KeyCode.N)) {
			gameOverState = true;
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
    // SERVER-ONLY METHODS
	public bool ServerRegisterPlayer(TankController player) {
		if (!isReady) return false;
		// Debug.Log("isServer: " + isServer);
		// Debug.Log("Network.isServer: " + Network.isServer);
		if (!isServer) return false;
		// Debug.Log("ServerRegisterPlayer: " + player);

		// assign tank index
		var newTankIndex = nextTankId;
		nextTankId++;
		tankRegistry[newTankIndex] = player;

		// server assigns tank index and acknowledges registration...
		player.ServerAssignIndex(newTankIndex);

		// update client tank registration
		RpcRegisterPlayer(player.gameObject, newTankIndex);
		return true;

	}

	public void ServerHandleShotFired(TankController player, GameObject projectileGO) {
		// Debug.Log("ServerHandleShotFired: " + projectileGO);
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
		// Debug.Log("starting game on client");
		StartCoroutine(ClientLoop());
	}

	/// <summary>
	/// Called from the server, executed on the client
	/// Start the main client loop
	/// </summary>
	[ClientRpc]
	void RpcRegisterPlayer(GameObject playerGo, int newTankIndex) {
		tankRegistry[newTankIndex] = playerGo.GetComponent<TankController>();
	}

	/// <summary>
	/// Called from the server, executed on the client
	/// Set the view to the local tank
	/// </summary>
	[ClientRpc]
	void RpcViewLocalTank(GameObject tankGO) {
		var tank = tankGO.GetComponent<TankController>();
		if (tank != null && tank.isLocalPlayer) {
			// Debug.Log("setting camera view to local: " + tank.name);
			camController.WatchPlayer(tank);
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
		// Debug.Log("RpcViewExplosion: isLocalPlayer: " + playerGO.GetComponent<TankController>().isLocalPlayer);
		if (playerGO.GetComponent<TankController>().isLocalPlayer || !localOnly) {
			camController.ShakeCamera(2.0f, 0.9f);
			camController.WatchExplosion(explosionGO);
		}
	}

	Vector3 GroundPosition(Vector3 position) {
		var groundPosition = new Vector3(
			position.x,
			Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y,
			position.z
		);
		return groundPosition;
	}

	void PlaceTank(TankController tank) {
		var tagName = String.Format("p{0}spawn", tank.playerIndex);
		var spawnPoints = GameObject.FindGameObjectsWithTag(tagName);
		if (spawnPoints != null) {
			var tankPosition = GroundPosition(spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position);
			Debug.Log("Placing tank: " + tank.playerName + " @ " + tankPosition);
			tank.ServerPlace(tankPosition);
			//tank.transform.position = tankPosition;
		} else {
			Debug.Log("failed to place tank, no spawn points");
		}
	}

    // ------------------------------------------------------
    // STATE ENGINES
	/// <summary>
	/// This is the main server loop
	/// </summary>
	IEnumerator ServerLoop() {
		// Debug.Log("starting ServerLoop");
		// wait for players to join
        yield return StartCoroutine(ListenForTanks());

		// build world
		yield return StartCoroutine(BuildWorld());

		// place tanks
		foreach (var tank in tankRegistry.Values) {
			PlaceTank(tank);
			tank.ServerActivate();
		}
		yield return null;

		// adjust camera
		//RpcViewLocalTank();

		// start the game on client
		RpcStart();

		// start the game
        yield return StartCoroutine(PlayRound());
		// FIXME: need to rework game win/loss logic
		gameOverState = true;
		// Debug.Log("finishing ServerLoop");
	}

	/// <summary>
	/// This is the main client loop
	/// </summary>
	IEnumerator ClientLoop() {
		Debug.Log("starting ClientLoop");
		// wait for players to join
        //yield return StartCoroutine(ListenForTanks());

		//camController.WatchPlayer(localTank);
		//camController.SetPlayerCameraFocus(localTank);
		yield return null;
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
		}

		//Debug.Log ("Tanks reporting for duty!");
	}

	IEnumerator PlayRound() {
		Debug.Log ("Starting the game!!!");

		// add current tanks to the active tank list
		activeTanks = new List<int>(tankRegistry.Keys);

		// set starting camera positions for each player (this is done client side)
		//RpcViewLocalTank();

		// determine turn order
		var turnOrder = GetTurnOrder(tankRegistry);
		// Debug.Log("turn order is -> " + String.Join(",", turnOrder.Select(v=>v.ToString()).ToArray()));
		var currentIndex = 0;
		yield return null;

		// continue to play the round while at least two tanks are active
		while (activeTanks.Count >= 2) {
			// determine next tank, advance current Index
			var nextTankId = turnOrder[(currentIndex++)%turnOrder.Length];

			// validate tank is active
			if (!activeTanks.Contains(nextTankId)) {
				// Debug.Log("skipping inactive tank: " + tankRegistry[nextTankId].name);
				continue;
			}

			// select active tank and take turn
			yield return StartCoroutine(TakeTankTurn(tankRegistry[nextTankId]));

		}

		// Debug.Log("Round is over, winner is " + tankRegistry[activeTanks[0]].name);
	}

	IEnumerator TakeTankTurn(TankController tank) {
		// Debug.Log("taking turn for " + tank.name);
		// activate the tank
		SetActiveTank(tank);

		// set starting camera positions for each player (this is done client side)
		RpcViewLocalTank(tank.gameObject);

		// wait for shot fired by this tank
		while (tank.hasControl) {
			yield return null;
		}

		// follow tank projectile
		if (liveProjectile != null) {
			// Debug.Log("live projectile detected");
			// update local camera to watch live projectile
			RpcViewShot(tank.gameObject, liveProjectile, true);
		}
		// wait until the projectile is destroyed
		while (liveProjectile != null) {
			yield return null;
		}

		// wait for explosion
		if (liveExplosion != null) {
			// Debug.Log("live explosion detected");
			// update local camera to watch live explosion
			RpcViewExplosion(tank.gameObject, liveExplosion, true);
		}
		// wait until the explosion is destroyed
		while (liveExplosion != null) {
			yield return null;
		}

		// reset view to local tank view
		RpcViewLocalTank(tank.gameObject);
	}


	public static TurnManager GetGameManager() {
		// find manager GO and manager script
		GameObject managerGO = GameObject.Find("GameManager");
		if (managerGO == null) {
			// Debug.Log("server registration failed, can't find game manager");
			return null;
		}
		var manager = managerGO.GetComponent<TurnManager>();
		if (manager == null) {
			// Debug.Log("server registration failed, can't find game manager script");
			return null;
		}
		return manager;
	}


}
