using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.Events;

public class MushBehavior : NetworkBehaviour {

    public UnityEvent nukeIsReady;
    public NukeScript theNuke;
    public TankController owner,
        shooter;
    public Terrain terrain;
    public ExplosionKind explosionKind = ExplosionKind.fire;
    public DeformationKind deformationKind = DeformationKind.shotCrater;
    public string areaOfEffect = "radiation";
    public float effectRadius = 20f;
    public GameObject mushPrefab,
        groundZero;
    public Rigidbody mushRigidBody;
    public ParticleSystem nukeGreen,
        explosion;
    public int lifeSpan = 5;
    public Vector3 growthSpurt;
    public Health health;
    public Color messageColor,
        warningColor;

    private List<ProjectileKind> lootPool = new List<ProjectileKind>();
    private List<int> lootCount = new List<int>();
    private bool scaleIt = false,
        isDead = false,
        lifeCycleOver = false,
        hasCollided = false;
    private Vector3 scaleTo;
    private int stage = 0,
        startingTurn,
        explosionScale = 2;
    private float timeToScale = 0.75f,
        scaleDelay = 0.75f,
        scaleTime = 0f;

    private void Start() {
        nukeIsReady = new UnityEvent();
        nukeIsReady.AddListener(TurnManager.singleton.NukeIsReady);
        terrain = Terrain.activeTerrain;
        theNuke = GameObject.FindWithTag("nuke").GetComponent<NukeScript>();
        health.onDeathEvent.AddListener(OnDeath);
        scaleTo = transform.localScale * 2;
        StartCoroutine(GrowTheShroom());
        startingTurn = TurnManager.singleton.numberOfTurns;
    }

    // Update is called once per frame
    void Update() {
        if (theNuke.blastKill && !lifeCycleOver) Destroy(gameObject);
        if (scaleIt) {
            if (scaleTime <= 1) {
                scaleTime += Time.deltaTime / timeToScale;
                gameObject.transform.localScale = Vector3.Lerp(transform.localScale, scaleTo, scaleTime);
            }
            else scaleIt = false;
        }
        if (!lifeCycleOver) {
            if (stage > lifeSpan) {
                mushRigidBody.detectCollisions = false;
                lifeCycleOver = true;
                StartCoroutine(FinishThem());
            }
            else if (stage < TurnManager.singleton.numberOfTurns - startingTurn) {
                stage++;
                AdvanceStage();
            }
        }
        GravityCheck();
    }

    void OnDeath(GameObject from) {
        if (!isDead) {
            isDead = true;
            transform.FindChild("Model").gameObject.SetActive(false);

            if (isServer) {
                DealDamage(from);
                CreateExplosion();
                PerformTerrainDeformation(terrain.gameObject);
                UxChatController.SendToConsole(owner.playerName + "'s MushBoom was destroyed!");

                float aNumber = UnityEngine.Random.Range(0f, 1f);
                if (aNumber < 0.5) {
                    if (owner != null) {
                        UxChatController.SendToConsole(owner.playerName + " scavenged a spore!");
                        var inventory = owner.GetComponent<ProjectileInventory>();
                        inventory.ServerModify(ProjectileKind.mushboom, 1);
                    }
                }
                Destroy(gameObject);
            }
        }
    }

    private void AdvanceStage() {
        switch (stage) {
            case 1:
                explosionScale = 2;
                break;
            case 2:
                effectRadius += 10;
                explosionScale = 3;
                deformationKind = DeformationKind.littleZero2;
                break;
            case 3:
                effectRadius += 10;
                explosionScale = 4;
                deformationKind = DeformationKind.littleZero3;
                break;
            case 4:
                effectRadius += 10;
                explosionScale = 5;
                deformationKind = DeformationKind.littleZero4;
                break;
            case 5:
                effectRadius += 10;
                explosionScale = 6;
                deformationKind = DeformationKind.littleZero5;
                break;
        }

        scaleTo = transform.localScale + growthSpurt;
        int percentReady = stage * 19;
        switch (percentReady) {
            case 19:
            case 38:
            case 57:
            case 76:
                UxChatController.SendToConsole(owner.playerName + "'s MushBoom", messageColor, percentReady.ToString() + "% critical mass! #Growing");
                break;
            case 95:
                UxChatController.SendToConsole(owner.playerName + "'s MushBoom", warningColor, "Ready soon! #Wheaties");
                break;
        }
        StartCoroutine(GrowTheShroom());
    }


    private void GravityCheck() {
        if (terrain != null) {
            Vector3 fixedSpot = transform.position;
            float heightDiff = fixedSpot.y - (terrain.SampleHeight(fixedSpot) + terrain.transform.position.y);
            if (heightDiff > transform.localScale.y / 3) mushRigidBody.useGravity = true;
            else mushRigidBody.useGravity = false;
        }
    }

    void OnCollisionEnter(Collision collision) {
        GameObject theObject = collision.gameObject;
        //Debug.Log("ProjectileController OnCollisionEnter with: " + collision.collider.name);
        // only trigger explosion (spawn) if we currently have authority
        // run collisions on server only
        if (isServer && theObject.name == "lootbox(Clone)") {
            Health boxHealth = theObject.GetComponent<Health>();
            if (boxHealth != null) boxHealth.TakeDamage(10, owner.gameObject);
        }
    }

    public void CollectLoot(ProjectileKind ammo, int ammoCount) {
        lootPool.Add(ammo);
        lootCount.Add(ammoCount);
    }

    private void DivvyLoot() {
        List<int> theTanks = TurnManager.singleton.activeTanks;
        TankController notTheOwner,
            theWinner;
        int whoWins = 1;

        if (theTanks.Count > 1) {
            if (TurnManager.singleton.tankRegistry[theTanks[0]] == owner) {
                notTheOwner = TurnManager.singleton.tankRegistry[theTanks[1]];
            }
            else notTheOwner = TurnManager.singleton.tankRegistry[theTanks[0]];

            for (int i = 0; i < lootPool.Count; i++) {
                if (whoWins > 0) theWinner = owner;
                else theWinner = notTheOwner;

                if (theWinner == null) break;
                else {
                    var inventory = theWinner.GetComponent<ProjectileInventory>();
                    if (inventory != null) inventory.ServerModify(lootPool[i], lootCount[i]);

                    UxChatController.SendToConsole(
                        String.Format("{0} scavenged {1} {2}",
                        theWinner.playerName,
                        lootCount[i],
                        NameMapping.ForProjectile(lootPool[i])));

                    whoWins *= -1;
                }
            }
        }
        lootPool.Clear();
        lootCount.Clear();
    }

    private void DealDamage(GameObject from) {
        shooter = from.GetComponent<TankController>();
        Collider[] flakReceivers = Physics.OverlapSphere(transform.position, effectRadius);

        // keep track of list of root objects already evaluated
        var hitList = new List<GameObject>();

        foreach (Collider flakReceiver in flakReceivers) {
            var rootObject = flakReceiver.transform.root.gameObject;

            // has this object already been hit by this projectile?
            if (!hitList.Contains(rootObject)) {
                hitList.Add(rootObject);

                GameObject gameObjRef = flakReceiver.gameObject;
                //Debug.Log("hit gameObject: " + rootObject.name);

                // Debuff
                TankController tankObj = rootObject.GetComponent<TankController>();
                if (tankObj != null) {
                    tankObj.SetAreaOfEffect(areaOfEffect);
                }

                // potentially apply damage to any object that has health component
                var health = rootObject.GetComponent<Health>();
                if (health != null) {
                    //Debug.Log (rootObject.name + " received splash damage");

                    Vector3 cannonballCenterToTankCenter = transform.position - gameObjRef.transform.position;
                    //Debug.Log (string.Format ("cannonball position: {0}, tank position: {1}", transform.position, gameObjRef.transform.position));
                    //Debug.Log (string.Format ("cannonballCenterToTankCenter: {0}", cannonballCenterToTankCenter));

                    // Some projectiles have AoE > 10, so damage radius needs to be clamped
                    float hitDistToTankCenter = Mathf.Min(10f, cannonballCenterToTankCenter.magnitude);
                    //Debug.Log ("Distance to tank center: " + hitDistToTankCenter);

                    // NOTE: The damagePoints formula below is taken from an online quadratic regression calculator. The idea
                    // was to plug in some values and come up with a damage computation formula.  The above formula yields:
                    // direct hit (dist = 0m): 100 hit points
                    // Hit dist 5m: about 25 hit points
                    // hit dist 10m: about 1 hit point
                    // The formula is based on a max proximity damage distance of 10m
                    int damagePoints = (int)(1.23f * hitDistToTankCenter * hitDistToTankCenter - 22.203f * hitDistToTankCenter + 100.012f);
                    damagePoints += (int)(effectRadius * 0.4);

                    if (damagePoints > 0 && rootObject != null) {
                        if (rootObject.name == "lootbox(Clone)") health.TakeDamage(damagePoints, gameObject);
                        else if (tankObj == null || !tankObj.hasControl) health.TakeDamage(damagePoints, (shooter != null) ? shooter.gameObject : null);
                        else health.RegisterDelayedDamage(damagePoints, (shooter != null) ? shooter.gameObject : null);

                        // FIXME: maybe add a specific sound for this explosion?

                        // Do shock displacement
                        // if target has rigidbody, apply displacement force to rigidbody
                        var rigidbody = rootObject.GetComponent<Rigidbody>();
                        if (rigidbody != null && rootObject.name != "mushMine(Clone)") {
                            Vector3 displacementDirection = cannonballCenterToTankCenter.normalized;
                            //Debug.Log (string.Format ("Displacement stats: direction={0}, magnitude={1}", displacementDirection, damagePoints));
                            rigidbody.AddForce(rigidbody.mass * (displacementDirection * damagePoints * 0.8f), ForceMode.Impulse);  // Force = mass * accel
                        }
                    }
                }
            }
        }
        DivvyLoot();
        if (shooter != null) shooter.GetComponent<Health>().TakeDelayedDamage();
    }

    private void CreateExplosion() {
        // instantiate explosion
        var explosionPrefab = PrefabRegistry.singleton.GetPrefab<ExplosionKind>(explosionKind);
        //Debug.Log("CmdExplode instantiate explosion: " + explosionPrefab);
        GameObject anExplosion = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
        anExplosion.transform.localScale = anExplosion.transform.localScale * explosionScale;
        NetworkServer.Spawn(anExplosion);

        // set explosion duration (destroy after duration)
        var explosionController = anExplosion.GetComponent<ExplosionController>();
        var explosionDuration = (explosionController != null) ? explosionController.duration : 3.0f;
        Destroy(anExplosion, explosionDuration);
    }

    private void PerformTerrainDeformation(GameObject terrain) {
        Debug.Log(terrain.name);
        // perform terrain deformation (if terrain was hit)
        var terrainManager = terrain.GetComponent<TerrainDeformationManager>();
        if (terrainManager != null) {
            var deformationPrefab = PrefabRegistry.singleton.GetPrefab<DeformationKind>(deformationKind);
            // FIXME: Different sound effects needed
            /* SingedMessages.SendPlayAudioClip(
            PrefabRegistry.GetResourceName<ProjectileSoundKind>(ProjectileSoundKind.projectile_explo));
            */
            //Debug.Log("CmdExplode instantiate deformation: " + deformationPrefab);
            GameObject deformation = Instantiate(deformationPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(deformation);
            // determine deformation seed
            var seed = UnityEngine.Random.Range(1, 1 << 24);
            // execute terrain deformation on client
            terrainManager.RpcApplyDeform(deformation, seed);
        }
    }

    private IEnumerator GrowTheShroom() {
        mushRigidBody.isKinematic = false;
        explosion.Play();
        scaleTime = 0f;

        yield return new WaitForSeconds(scaleDelay);
        scaleIt = true;
        StopCoroutine(GrowTheShroom());
    }

    private IEnumerator FinishThem() {
        UxChatController.SendToConsole(owner.playerName + "'s MushBoom", messageColor, "It's been a blast! #Laterz");
        scaleDelay = 0;
        timeToScale = 10f;
        groundZero = GameObject.FindWithTag("groundZero");
        Vector3 fixedSpot = new Vector3(100f, 15f, 100f);
        deformationKind = DeformationKind.groundZero;

        yield return new WaitForSeconds(1f);
        scaleTo *= 4f;
        nukeGreen.Emit(150);
        StartCoroutine(GrowTheShroom());

        yield return new WaitForSeconds(1f);
        nukeIsReady.Invoke();
        nukeGreen.Emit(150);
        nukeGreen.transform.localScale *= 2f;

        yield return new WaitForSeconds(0.75f);
        nukeGreen.Emit(200);

        yield return new WaitForSeconds(1f);
        nukeGreen.transform.localScale *= 2;
        nukeGreen.Emit(200);

        yield return new WaitForSeconds(1.25f);
        explosion.Play();
        nukeGreen.Emit(200);

        yield return new WaitForSeconds(4.5f);
        Destroy(explosion);
        Destroy(nukeGreen);
        transform.position = fixedSpot;
        if (isServer) PerformTerrainDeformation(groundZero);
        Destroy(gameObject);
    }

}