using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;


public class TankController : NetworkBehaviour {

    private bool isSlowed = false;

    // Public
    public GameObject modelPrefab;
    public TankModel model;

    public float shotPower = 30.0f;
    public float shotPowerModifier = 10.0f;
    public float maxShotPower = 2500f;

    public float rotationSpeedVertical = 5.0f;
    public float rotationSpeedHorizontal = 5.0f;

    public Transform passiveCameraSource
    {
        get
        {
            if (model != null) {
                return model.passiveCameraSource;
            }
            else {
                return transform;
            }
        }
    }

    public Transform chaseCameraSource
    {
        get
        {
            if (model != null) {
                return model.chaseCameraSource;
            }
            else {
                return transform;
            }
        }
    }

    public Rigidbody rb;
    public ProjectileKind selectedShot;
    public ProjectileInventory shotInventory;

    bool togglePowerInputAmount = false;
    float savedPowerModifier;

    // state management variables
    [SyncVar]
    bool hasRegistered = false;     // am I registered to turn controller

    [SyncVar(hook = "OnChangeControl")]
    public bool hasControl = false;

    [SyncVar]
    public int playerIndex = -1;

    [SyncVar]
    public string playerName = "";

    public int charVoice = 1;
    public AudioClip speech;

    private TankSoundKind tankSoundKind = TankSoundKind.canonFire1;
    private AudioClip tankSound;
    private AudioClip tankPowerSound;
    private AudioClip turretHorizontalMovementSound;
    private AudioClip turretVerticalMovementSound;
    private AudioSource tankHorizontalMovementAudioSource;
    private AudioSource tankVerticalMovementAudioSource;
    private AudioSource tankPowerAudioSource;
    private GameObject secondaryAudioSource;
    private GameObject powerAudioSource;

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
    public bool hasVirus = false;
    private float virusDuration = 2;
    public ParticleSystem virusParticles;
    public GameObject virusParticlesGO;
    private Health playerHealth;
    private GameObject infectingPlayer;

    void Awake() {
        //Debug.Log("TankController Awake: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
        // lookup/cache required components
        rb = GetComponent<Rigidbody>();
        shotInventory = GetComponent<ProjectileInventory>();

        // disable rigidbody physics until activated by turn manager
        SetPhysicsActive(false);

        // create underlying model
        CreateModel();

        // let there be sound!
        tankSound = (AudioClip)Resources.Load("TankSound/" + tankSoundKind);
        turretHorizontalMovementSound = (AudioClip)Resources.Load("TankSound/" + TankSoundKind.tank_movement_LeftRight_LOOP_01);
        turretVerticalMovementSound = (AudioClip)Resources.Load("TankSound/" + TankSoundKind.tank_movement_UpDown_LOOP_01);
        tankPowerSound = (AudioClip)Resources.Load("TankSound/" + TankSoundKind.tank_power_UpDown_LOOP);
        tankHorizontalMovementAudioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        tankHorizontalMovementAudioSource.loop = true;
        tankHorizontalMovementAudioSource.clip = turretHorizontalMovementSound;

        secondaryAudioSource = new GameObject("secondaryAudioSource");
        secondaryAudioSource.transform.SetParent(transform);
        tankVerticalMovementAudioSource = secondaryAudioSource.AddComponent<AudioSource>() as AudioSource;
        tankVerticalMovementAudioSource.loop = true;
        tankVerticalMovementAudioSource.clip = turretVerticalMovementSound;
        charVoice = BarkManager.self.AssignCharVoice();

        powerAudioSource = new GameObject("powerAudioSource");
        powerAudioSource.transform.SetParent(transform);
        tankPowerAudioSource = powerAudioSource.AddComponent<AudioSource>() as AudioSource;
        tankPowerAudioSource.loop = true;
        tankPowerAudioSource.clip = tankPowerSound;
        
        virusParticlesGO = (GameObject)GameObject.Instantiate(Resources.Load("Spawn/" + SpawnKind.InfectedParticles));
        virusParticlesGO.transform.SetParent(transform);
        virusParticlesGO.name = "E-Virus";
        virusParticles = virusParticlesGO.GetComponentInChildren<ParticleSystem>();
        playerHealth = GetComponent<Health>();
    }

    public void ServerActivate() {
        RpcActivate();
    }

    public void ServerPlace(Vector3 position) {
        RpcPlace(position);
    }

    void OnDeath(GameObject from) {
        Debug.Log("OnDeath");
        UxChatController.SendToConsole(
            String.Format("{0} terminated {1}",
                from.GetComponent<TankController>().playerName,
                playerName));
        var manager = TurnManager.GetGameManager();
        if (manager != null) {
            manager.ServerHandleTankDeath(gameObject);
        }
    }

    void OnDestroy() {
        Debug.Log("TankController.OnDestroy, isServer: " + isServer);
        if (TurnManager.singleton != null) {
            TurnManager.singleton.ServerDeletePlayer(this);
        }
    }

    void SetPhysicsActive(bool status) {
        rb.isKinematic = !status;
        rb.detectCollisions = status;
    }
    static int spawnOrder = 0;
    void CreateModel() {
        // upon start up ... instantiate the model
        var modelGo = (GameObject)GameObject.Instantiate(
            modelPrefab,
            transform.position,
            Quaternion.identity,
            this.transform
        );
        model = modelGo.GetComponent<TankModel>();
        spawnOrder++;
        gameObject.name = "TankSpawn" + spawnOrder;
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

    void Start() {
        // cheater!!!
        // following loop gives max ammo for each shot type
        // disable to allow lootbox system to be worthwile
        /*
		for(int i = 0; i < System.Enum.GetValues(typeof(ProjectileKind)).Length; i++){
			shotInventory.Modify((ProjectileKind) i, int.MaxValue);
		}
		*/

        // link health onDeath event
        var health = GetComponent<Health>();
        if (health != null) {
            health.onDeathEvent.AddListener(OnDeath);
        }
        // Debug.Log("TankController Start: isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);

        // copy state to model
        model.tankBaseKind = tankBaseKind;
        model.turretBaseKind = turretBaseKind;
        model.turretKind = turretKind;
        model.hatKind = hatKind;
        model.UpdateAvatar();

        savedPowerModifier = shotPowerModifier;
    }

    public void InfectPlayer(GameObject patientZero){
        virusParticles.Play();
        hasVirus = true;
        infectingPlayer = patientZero;
        UxChatController.SendToConsole("" + gameObject.name + " has been infected with an E-Virus!");
    }

    public void DialAdjustPower(int offset) {
        bool isNegative = offset < 0;
        float tweakAmt = 0.0f;
        switch (Mathf.FloorToInt(Mathf.Abs(offset))) {
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
        }
        else {
            shotPower += tweakAmt;
        }

        if (shotPower > maxShotPower) {
            shotPower = maxShotPower;
        }
    }

    public void DialAdjustElevation(int offset) {
        bool isNegative = offset < 0;
        float tweakAmt = 0.0f;
        switch (Mathf.FloorToInt(Mathf.Abs(offset))) {
            case 1:
                tweakAmt = 1.0f;
                break;
            case 2:
                tweakAmt = 5.0f;
                break;
            case 3:
                tweakAmt = 10.0f;
                break;
        }

        if (isNegative) {
            model.turretElevation -= tweakAmt;
        }
        else {
            model.turretElevation += tweakAmt;
        }
    }

    public string AmmoDisplayCountText() {
        var available = shotInventory.GetAvailable(selectedShot);
        if (available == int.MaxValue) {
            return "Unlimited";
        }
        else {
            return available.ToString();
        }

    }

    public void DialAdjustHeading(int offset) {
        bool isNegative = offset < 0;
        float tweakAmt = 0.0f;
        switch (Mathf.FloorToInt(Mathf.Abs(offset))) {
            case 1:
                tweakAmt = 1.0f;
                break;
            case 2:
                tweakAmt = 5.0f;
                break;
            case 3:
                tweakAmt = 10.0f;
                break;
        }
        if (isNegative) {
            model.tankRotation -= tweakAmt;
        }
        else {
            model.tankRotation += tweakAmt;
        }
    }

    public void SetAreaOfEffect(string effect) {
        switch (effect) {
            case "slow":
            case "Slow":
                isSlowed = true;
                transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                break;
            default:
                break;
        }
    }

    void Register() {
        // local player handles own registration
        if (!isLocalPlayer) return;
        // only register once
        if (hasRegistered) return;

        // server-side: register directly
        if (isServer) {
            ServerRegisterToManager();

            // client-side: send command to server
        }
        else {
            CmdRegister();
        }
    }

    // Update is called once per frame
    void Update() {
        // register to turn manager (if required)
        if (!hasRegistered) {
            Register();
        }
    }

    // ------------------------------------------------------
    // CLIENT-ONLY METHODS

    // ------------------------------------------------------
    // SERVER-ONLY METHODS

    /// <summary>
    /// Executed on the server
    /// Assign index to tank... this same index will be used on both client and server and is synced when assigned
    /// </summary>
    public void ServerAssignIndex(int index) {
        if (!isServer) return;
        // Debug.Log("assigning index " + index + " to " + name);
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
        //Debug.Log("ServerRegisterToManager, manager: " + TurnManager.singleton);
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
        // Debug.Log("OnChangeControl called for " + this.name + " with isServer: " + isServer + " hasControl: " + currentHasControl);
        // only apply change control to local player
        if (currentHasControl == hasControl) return;
        hasControl = currentHasControl;
        if (!isLocalPlayer) return;

        // disable -> enable
        if (currentHasControl) {
            StartCoroutine(ShootStateEngine());

            // enable -> disable
        }
        else {

        }
    }

    // ------------------------------------------------------
    // STATE ENGINES
    /// <summary>
    /// This is the state-engine driving tank operations while controls are active
    /// </summary>
    IEnumerator ShootStateEngine() {
        // Debug.Log("ShootStateEngine called for " + this.name + " with isServer: " + isServer);
        // disable tank physics
        rb.isKinematic = true;

        // turn related effects
        if (hasVirus == true) {
            if (virusDuration <= 0) {
                hasVirus = false;
                virusDuration = 2;
                virusParticles.Stop();
            } else {
                playerHealth.TakeDamage(10, infectingPlayer);
                virusDuration -= 1;
            }
        }

        // line up and take the shot
        yield return StartCoroutine(AimStateEngine());

        // relinquish control
        // Debug.Log("ShootStateEngine release control for " + this.name);
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
        // check to see if we still have availability for selected shot
        if (shotInventory.GetAvailable(selectedShot) <= 0) {
            selectedShot = shotInventory.NextAvailableShot(selectedShot);
            Debug.Log("now using shot: " + selectedShot);
        }

        // Debug.Log("AimStateEngine called for " + this.name + " with isServer: " + isServer + " and hasControl: " + hasControl);
        // continue while we have control
        while (hasControl) {
            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.tag == "inputexclusive") {
                yield return null;
                continue;
            }
            tankHorizontalMovementAudioSource.volume = SoundManager.instance.SFXVolume;
            tankVerticalMovementAudioSource.volume = SoundManager.instance.SFXVolume;
            tankPowerAudioSource.volume = SoundManager.instance.SFXVolume;

            //Debug.Log("OnComma: focused control is: " + EventSystem.current.currentSelectedGameObject);
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) {
                togglePowerInputAmount = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) {
                togglePowerInputAmount = false;
            }
            if (togglePowerInputAmount == false) {
                if (Input.GetKey(KeyCode.LeftBracket)) {
                    if(tankPowerAudioSource.isPlaying == false){
                        tankPowerAudioSource.Play();
                    }
                    shotPower -= shotPowerModifier;
                    if (shotPower <= 0.0f) {
                        shotPower = 0.0f;
                    }
                }

                if (Input.GetKey(KeyCode.RightBracket)) {
                    if(tankPowerAudioSource.isPlaying == false){
                        tankPowerAudioSource.Play();
                    }
                    shotPower += shotPowerModifier;
                }
            }
            else {
                if (Input.GetKeyDown(KeyCode.LeftBracket)) {
                    if(tankPowerAudioSource.isPlaying == false){
                        tankPowerAudioSource.Play();
                    }
                    shotPower -= shotPowerModifier;
                    if (shotPower <= 0.0f) {
                        shotPower = 0.0f;
                    }
                }

                if (Input.GetKeyDown(KeyCode.RightBracket)) {
                    Debug.Log("right bracket");
                    if(tankPowerAudioSource.isPlaying == false){
                        tankPowerAudioSource.Play();
                    }
                    shotPower += shotPowerModifier;
                }
            }
            if(Input.GetKeyUp(KeyCode.RightBracket) || Input.GetKeyUp(KeyCode.LeftBracket)){
                tankPowerAudioSource.Stop();                    
            }

            if (Input.GetKeyDown(KeyCode.Comma)) {
                selectedShot = shotInventory.PrevAvailableShot(selectedShot);
                Debug.Log("now using shot: " + selectedShot);
            }
            if (Input.GetKeyDown(KeyCode.Period)) {
                selectedShot = shotInventory.NextAvailableShot(selectedShot);
                Debug.Log("now using shot: " + selectedShot);
            }

            if (shotPower > maxShotPower) {
                shotPower = maxShotPower;
            }

            // Shoot already ... when shot is fired, finish this coroutine;
            if (Input.GetKeyDown(KeyCode.Space)) {
                //Debug.Log("space is down, calling CmdFire");
                // sanity check for ammo
                tankVerticalMovementAudioSource.Stop();
                tankHorizontalMovementAudioSource.Stop();
                if (shotInventory.GetAvailable(selectedShot) > 0) {
                    speech = BarkManager.self.GetTheShotOneLiner(selectedShot, charVoice);
                    SoundManager.instance.PlayAudioClip(tankSound);
                    SoundManager.instance.PlayClipDelayed(tankSound.length / 2, speech);
                    CmdFire(shotPower, selectedShot);
                    // decrease ammo count
                    shotInventory.ServerModify(selectedShot, -1);
                    // check to see if we still have availability for selected shot
                    if (shotInventory.GetAvailable(selectedShot) <= 0) {
                        selectedShot = shotInventory.NextAvailableShot(selectedShot);
                        Debug.Log("now using shot: " + selectedShot);
                    }
                }
                else {
                    Debug.Log("out of ammo for shottype " + selectedShot);
                }

                yield break;
            }
            if (Input.GetKeyDown(KeyCode.O)) {
                speech = BarkManager.self.GetTheShotOneLiner(selectedShot, charVoice);
            }

            if (model != null) {
                model.tankRotation += Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeedVertical;
                model.turretElevation += Input.GetAxis("Vertical") * Time.deltaTime * rotationSpeedHorizontal;
                model.turretElevation = Mathf.Clamp(model.turretElevation, minTurretElevation, maxTurretElevation);
            }
            if (Input.GetAxis("Horizontal") != 0) {
                if (tankHorizontalMovementAudioSource.isPlaying == false) {
                    tankHorizontalMovementAudioSource.Play();
                }
            }
            if (Input.GetAxis("Vertical") != 0) {
                if (tankVerticalMovementAudioSource.isPlaying == false) {
                    tankVerticalMovementAudioSource.Play();
                }
            }

            if (Input.GetAxis("Vertical") == 0) {
                tankVerticalMovementAudioSource.Stop();
            }
            if (Input.GetAxis("Horizontal") == 0) {
                tankHorizontalMovementAudioSource.Stop();
            }

            // continue on next frame
            yield return null;
        }
    }

    // ------------------------------------------------------
    // SERVER->CLIENT METHODS
    [ClientRpc]
    void RpcActivate() {
        //Debug.Log("activating tank: " + playerName);
        SetPhysicsActive(true);
    }

    [ClientRpc]
    void RpcPlace(Vector3 position) {
        if (isLocalPlayer) {
            //Debug.Log(String.Format("positioning tank: {0} @ {1}", playerName, position));
            transform.position = position;
        }
    }

    // ------------------------------------------------------
    // CLIENT->SERVER METHODS
    public void SendToConsole(string newChat) {
        CmdSendToConsole(newChat);
    }

    [Command]
    public void CmdSendToConsole(string newChat) {
        UxChatController.SendToConsole(this, newChat);
    }

    [Command]
    void CmdRegister() {
        ServerRegisterToManager();
    }

    /// <summary>
    /// Called from the client, executed on the server
    /// Fire selected projectile if current player controller is in proper state.
    /// </summary>
    [Command]
    void CmdFire(float shotPower, ProjectileKind projectiledKind) {
        //Debug.Log("CmdFire for " + name + " hasControl: " + hasControl);
        // controller state-based authorization check
        if (!hasControl) {
            Debug.Log("nope");
            return;
        }

        // instantiate from prefab
        var prefab = PrefabRegistry.singleton.GetPrefab<ProjectileKind>(projectiledKind);
        var liveProjectile = (GameObject)GameObject.Instantiate(
            prefab,
            model.shotSource.position,
            model.shotSource.rotation
        );
        if (isSlowed) {
            liveProjectile.GetComponent<Rigidbody>().mass *= 2;
            liveProjectile.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
            transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            isSlowed = false;
        }
        liveProjectile.name = name + "Projectile";
        liveProjectile.layer = gameObject.layer;
        liveProjectile.GetComponent<ProjectileController>().shooter = this;
        liveProjectile.GetComponent<ProjectileController>().SetProjectileKind(selectedShot);

        // set initial velocity/force
        liveProjectile.GetComponent<Rigidbody>().AddForce(model.shotSource.forward * shotPower);

        // set network spawn
        NetworkServer.Spawn(liveProjectile);

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
