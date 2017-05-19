using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class TurnManager : NetworkBehaviour {


    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public HudController hudController;
    public GameOverController gameOverController;

    public float minPlayerSpacing = 30f;
    public bool useRandomSpawn = true;

    // Public variables
    public static TurnManager singleton;

    // the list of all tanks, mapping their tank ID to tank controller
    // NOTE: this mapping is maintained on both client and server
    public Dictionary<int, TankController> tankRegistry;

    // number of players in game
    public int expectedPlayers = 2;
    public int currentRound = 1;

    // the list of tanks currently active in the round... as tanks die, they are removed from this list
    public List<int> activeTanks;

    // Private variables
    bool isReady = false;
    bool gameStarted = false;

    // list of spawn points
    public List<Vector3> spawnPoints;

    int nextTankId = 1;

    CameraController camController;
    TankController activeTank;
    TankController localActiveTank;
    float horizontalTurret;
    float verticalTurret;
    float shotPower;
    int tankHitPoints;

    public GameObject helpUI;
    bool helpVisible = false;

    [SyncVar]
    bool gameOverState = false;

    GameObject liveProjectile = null;
    GameObject liveExplosion = null;

    void Awake() {
        // Debug.Log("TurnManager Awake: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
        singleton = this;
        tankRegistry = new Dictionary<int, TankController>();
        activeTanks = new List<int>();
        camController = Camera.main.GetComponent<CameraController>();
        spawnPoints = new List<Vector3>();
    }

    public void ServerGameOver() {
        // winning tank should be the only tank left in the registry...
        var winner = tankRegistry[activeTanks[0]];
        gameOverState = true;
        currentRound = 1;
        LootSpawnController.singleton.mushboomCount = 0;

        // declare winner on each client
        RpcGameOver(winner.gameObject);

    }

    public bool GetGameOverState() {
        return gameOverState;
    }

    // Use this for initialization
    void Start() {
        // Debug.Log("TurnManager Start: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
        isReady = true;
    }

    void ServerSetActiveTank(TankController tank) {
        Debug.Log(String.Format("ServerSetActiveTank Activating tank: {0}, isLocalPlayer: {1}", tank.name, tank.isLocalPlayer));
        //activeTank = tank;
        tank.ServerEnableControl();
        RpcSetActiveTank(tank.gameObject);
    }

    public TankController GetActiveTank() {
        return activeTank;
    }
    public TankController GetLocalActiveTank() {
        return localActiveTank;
    }

    // Update is called once per frame
    void Update() {
        if (!gameStarted && isServer) {
            gameStarted = true;
            StartCoroutine(ServerLoop());
        }

        if (EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.tag == "inputexclusive") {
            return;
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            ServerGameOver();
        }
        if (Input.GetKeyDown(KeyCode.H) && helpVisible == false) {
            helpUI.SetActive(true);
            helpVisible = true;
        }
        else if (Input.GetKeyDown(KeyCode.H) && helpVisible == true) {
            helpUI.SetActive(false);
            helpVisible = false;
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

    public void ServerDeletePlayer(TankController player) {
        if (!isServer) return;
        Debug.Log("ServerDeletePlayer: " + player.playerName);

        // remove player from active tank list and registry
        if (activeTanks.Contains(player.playerIndex)) {
            activeTanks.Remove(player.playerIndex);
        }
        if (tankRegistry.ContainsKey(player.playerIndex)) {
            tankRegistry.Remove(player.playerIndex);
        }
    }

    public void ServerHandleShotFired(TankController player, GameObject projectileGO) {
        // Debug.Log("ServerHandleShotFired: " + projectileGO);
        if (!isServer) return;
        liveProjectile = projectileGO;
    }

    public void ServerHandleExplosion(GameObject explosionGO) {
        // Debug.Log("ServerHandleExplosion: " + explosionGO);
        if (!isServer) return;
        liveExplosion = explosionGO;
    }

    public void ServerHandleTankDeath(GameObject playerGO) {
        // Debug.Log("ServerHandleTankDeath: " + playerGO);
        var player = playerGO.GetComponent<TankController>();
        if (player != null) {
            // remove player from set of active tanks
            activeTanks.Remove(player.playerIndex);
            // Debug.Log("removing index: " + player.playerIndex + " new active tanks -> " + String.Join(",", activeTanks.Select(v=>v.ToString()).ToArray()));
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
    void RpcGameOver(GameObject winnerGo) {
        var winner = winnerGo.GetComponent<TankController>();
        if (winner != null) {
            gameOverController.SetWinner(winner.playerName);
            gameOverController.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Called from the server, executed on the client
    /// Set the current active tank in the client-side turn manager
    /// </summary>
    [ClientRpc]
    void RpcSetActiveTank(GameObject tankGo) {
        var tank = tankGo.GetComponent<TankController>();
        if (tank != null) {
            Debug.Log(String.Format("RpcSetActiveTank Activating tank: {0}, isLocalPlayer: {1}", tank.name, tank.isLocalPlayer));
            activeTank = tank;
            if (activeTank.isLocalPlayer) {
                hudController.AssignTank(activeTank);
                localActiveTank = activeTank;
            }
        }
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
    void RpcRegisterPlayerForTurn(GameObject playerGo) {
        var player = playerGo.GetComponent<TankController>();
        if (player == null) return;

        // update game turn order panel
        var turnPanelGo = GameObject.FindGameObjectWithTag("turnpanel");
        var turnPanel = (turnPanelGo != null) ? turnPanelGo.GetComponent<PlayerTurnInfoListController>() : null;
        if (turnPanel != null) {
            turnPanel.AddPlayer(player);
        }
    }

    [ClientRpc]
    void RpcAssignTurn(GameObject playerGo) {
        var player = playerGo.GetComponent<TankController>();
        if (player == null) return;

        // assign turn via turn panel
        var turnPanelGo = GameObject.FindGameObjectWithTag("turnpanel");
        var turnPanel = (turnPanelGo != null) ? turnPanelGo.GetComponent<PlayerTurnInfoListController>() : null;
        if (turnPanel != null) {
            turnPanel.ActivatePlayer(player);
        }
    }

    [ClientRpc]
    void RpcViewShot(GameObject playerGO, GameObject projectileGO, bool localOnly) {
        if (playerGO.GetComponent<TankController>().isLocalPlayer || !localOnly) {
            //camController.ShakeCamera(0.8f, 0.8f);
            Debug.Log("playerGO name is " + playerGO.name);
            camController.WatchLaunch(projectileGO, playerGO);
        }
    }

    [ClientRpc]
    void RpcViewExplosion(GameObject playerGO, GameObject explosionGO, bool localOnly) {
        // Debug.Log("RpcViewExplosion: isLocalPlayer: " + playerGO.GetComponent<TankController>().isLocalPlayer);
        if (playerGO.GetComponent<TankController>().isLocalPlayer || !localOnly) {
            camController.ShakeCamera(2.0f, 0.9f);
            // camController.WatchExplosion(explosionGO);
        }
    }

    [ClientRpc]
    void RpcAddSpawn(Vector3 spawnLocation) {
        spawnPoints.Add(spawnLocation);
    }

    Vector3 GroundPosition(Vector3 position) {
        var groundPosition = new Vector3(
            position.x,
            Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y,
            position.z
        );
        return groundPosition;
    }

    void PlaceTank(TankController tank, Vector3 spawnPoint) {
        var tankPosition = GroundPosition(spawnPoint);
        // Debug.Log("Placing tank: " + tank.playerName + " @ " + tankPosition);
        tank.ServerPlace(tankPosition);
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

        // create spawn points
        ISpawnGenerator spawnGenerator;
        if (useRandomSpawn) {
            var maxX = 200f;
            var maxZ = 200f;
            var terrain = Terrain.activeTerrain;
            if (terrain != null) {
                maxX = terrain.terrainData.size.x;
                maxZ = terrain.terrainData.size.z;
            }
            spawnGenerator = new RandomSpawnGenerator(minPlayerSpacing, maxX, maxZ);
        }
        else {
            spawnGenerator = new FixedSpawnGenerator();
        }
        // add two extra spawn points to break up the map into more chunks (if player count is low)
        var numToSpawn = (tankRegistry.Count <= 4) ? tankRegistry.Count + 2 : tankRegistry.Count;
        var spawnPoints = spawnGenerator.Generate(numToSpawn);

        // register spawn points over network
        foreach (var spawnPoint in spawnPoints) {
            RpcAddSpawn(spawnPoint);
        }

        // build world
        yield return StartCoroutine(BuildWorld());

        // place tanks
        foreach (var tankId in tankRegistry.Keys) {
            var spawnPoint = spawnPoints[tankId - 1];
            var tank = tankRegistry[tankId];
            PlaceTank(tank, spawnPoint);
            // add spawn exclusion
            if (LootSpawnController.singleton != null) {
                LootSpawnController.singleton.AddExclusion(spawnPoint, 10f);
            }
            tank.ServerActivate();
        }
        yield return null;

        // spawn initial lootboxes
        StartCoroutine(DelayLootSpawn());

        // adjust camera
        //RpcViewLocalTank();

        // start the game on client
        RpcStart();

        // start the game
        yield return StartCoroutine(PlayRound());
        // FIXME: need to rework game win/loss logic
        ServerGameOver();
        // Debug.Log("finishing ServerLoop");
    }

    /// <summary>
    /// This is the main client loop
    /// </summary>
    IEnumerator ClientLoop() {
        // Debug.Log("starting ClientLoop");
        // wait for players to join
        //yield return StartCoroutine(ListenForTanks());

        //camController.WatchPlayer(localTank);
        //camController.SetPlayerCameraFocus(localTank);
        yield return null;
    }

    /// <summary>
    /// Delay loot box spawn until after terrain deformation is done.
    /// NOTE: this is a hack... it would be better to have this state driven
    /// but the state hooks aren't there to handle waiting for a multitude of
    /// terrain deformers to finish...
    /// So hacking this to just delay for X seconds before spawning loot boxes
    /// </summary>
    IEnumerator DelayLootSpawn() {
        // FIXME: tunable for wait and # of spawn boxes
        yield return new WaitForSeconds(4);
        if (LootSpawnController.singleton != null) {
            LootSpawnController.singleton.ServerSpawnInit();
        }
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

    IEnumerator ListenForTanks() {
        // wait for new players to join, up to expected number of players
        while (tankRegistry.Count < expectedPlayers) {
            yield return null;
        }

        // we have expected number of tanks... initialize each tank
        foreach (int tankId in tankRegistry.Keys) {
            tankRegistry[tankId].name = "Player " + tankId.ToString();
        }

        //Debug.Log ("Tanks reporting for duty!");
    }

    IEnumerator PlayRound() {
        // Debug.Log ("Starting the game!!!");

        // add current tanks to the active tank list
        activeTanks = new List<int>(tankRegistry.Keys);

        // set starting camera positions for each player (this is done client side)
        //RpcViewLocalTank();

        // determine turn order
        var turnOrder = GetTurnOrder(tankRegistry);
        // Debug.Log("turn order is -> " + String.Join(",", turnOrder.Select(v=>v.ToString()).ToArray()));
        var currentIndex = 0;
        yield return null;

        // update game turn order panel
        foreach (var index in turnOrder) {
            RpcRegisterPlayerForTurn(tankRegistry[index].gameObject);
        }

        // continue to play the round while at least two tanks are active
        while (activeTanks.Count >= 2) {
            // determine next tank, advance current Index
            var nextTankId = turnOrder[(currentIndex++) % turnOrder.Length];

            // validate tank is active
            if (!activeTanks.Contains(nextTankId)) {
                // Debug.Log("skipping inactive tank: " + tankRegistry[nextTankId].name);
                continue;
            }

            // show tank is active in turn order panel
            RpcAssignTurn(tankRegistry[nextTankId].gameObject);

            // select active tank and take turn
            yield return StartCoroutine(TakeTankTurn(tankRegistry[nextTankId]));

            // end of round
            if (currentIndex % turnOrder.Length == turnOrder.Length - 1) {
                if (LootSpawnController.singleton != null) {
                    currentRound++;
                    LootSpawnController.singleton.ServerSpawnRound();
                }
            }

        }

        // Debug.Log("Round is over, winner is " + tankRegistry[activeTanks[0]].name);
    }

    IEnumerator TakeTankTurn(TankController tank) {
        // Debug.Log("taking turn for " + tank.name);
        // activate the tank
        ServerSetActiveTank(tank);

        // set starting camera positions for each player (this is done client side)
        RpcViewLocalTank(tank.gameObject);

        // wait for shot fired by this tank
        while (tank != null && tank.hasControl) {
            yield return null;
        }

        // follow tank projectile
        if (tank != null && liveProjectile != null) {
            // Debug.Log("live projectile detected");
            // update local camera to watch live projectile
            RpcViewShot(tank.gameObject, liveProjectile, true);
        }
        // wait until the projectile is destroyed
        while (liveProjectile != null) {
            yield return null;
        }

        // wait for explosion
        if (tank != null && liveExplosion != null) {
            // Debug.Log("live explosion detected");
            // update local camera to watch live explosion
            RpcViewExplosion(tank.gameObject, liveExplosion, true);
        }
        // wait until the explosion is destroyed
        while (liveExplosion != null) {
            yield return null;
        }

        // reset view to local tank view
        if (tank != null) {
            RpcViewLocalTank(tank.gameObject);
        }
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
