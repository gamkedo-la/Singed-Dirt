using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

    Terrain terrain;
    TurnManager manager;
    bool hasCollided = false;

    public ExplosionKind explosionKind = ExplosionKind.fire;
    public DeformationKind deformationKind = DeformationKind.shotCrater;
    private ProjectileKind myKind;

    public GameObject clusterBomblet;

    public bool isMushboom = false;
    public string areaOfEffect = "None";
    public float effectRadius = 10f;
    public float clusterHeight = 20.0f;
    private bool passedClusterHeight = false;
    private Rigidbody rb;
    public float bomletForceKick = 50.0f;
    public int numberOfBomblets = 8;
    public TankController shooter;
    private Vector3 startPos;

    // Use this for initialization
    void Start() {
        terrain = Terrain.activeTerrain;
        manager = TurnManager.GetGameManager();
        DisableCollisions(0.2f);
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    public void SetProjectileKind(ProjectileKind kind) {
        myKind = kind;
    }

    void OnCollisionEnter(Collision collision) {
        //Debug.Log("ProjectileController OnCollisionEnter with: " + collision.collider.name);
        // only trigger explosion (spawn) if we currently have authority
        // run collisions on server only
        if (isServer && !hasCollided) {
            // single collision/explosion per projectile
            hasCollided = true;
            if (!isMushboom) {
                Debug.Log("collision is " + collision.gameObject.name);
                ServerExplode(collision);
            }
            else {
                GetComponent<MushBehavior>().PlantIt();
            }
        }
    }

    // ------------------------------------------------------
    // SERVER-ONLY METHODS

    /// <summary>
    /// Generate explosion and apply damage for projectile
    /// </summary>
    void ServerExplode(Collision collision) {
        //Debug.Log("ServerExplode: " + this);
        // Get list of colliders in range of this explosion
        // FIXME: range of projectile shouldn't be hard-coded
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

                    float hitDistToTankCenter = cannonballCenterToTankCenter.magnitude;
                    //Debug.Log ("Distance to tank center: " + hitDistToTankCenter);

                    // NOTE: The damagePoints formula below is taken from an online quadratic regression calculator. The idea
                    // was to plug in some values and come up with a damage computation formula.  The above formula yields:
                    // direct hit (dist = 0m): 100 hit points
                    // Hit dist 5m: about 25 hit points
                    // hit dist 10m: about 1 hit point
                    // The formula is based on a max proximity damage distance of 10m
                    int damagePoints = (int)(1.23f * hitDistToTankCenter * hitDistToTankCenter - 22.203f * hitDistToTankCenter + 100.012f);
                    if (damagePoints > 0 && deformationKind != DeformationKind.pillarDeformer) {
                        if (myKind == ProjectileKind.cannonBall) {
                            damagePoints /= 2;
                        }
                        if (myKind == ProjectileKind.acorn) {
                            damagePoints = (int)(damagePoints * 1.5);
                        }
                        Debug.Log("MyKind is " + myKind);
                        if (tankObj != null) {
                            if(myKind == ProjectileKind.artilleryShell && tankObj.hasVirus == false){
                                tankObj.InfectPlayer(rootObject);
                                damagePoints = 20;
                            }
                        }

                        health.TakeDamage(damagePoints, (shooter != null) ? shooter.gameObject : null);
                        SingedMessages.SendPlayAudioClip(
                            PrefabRegistry.GetResourceName<ProjectileSoundKind>(ProjectileSoundKind.tank_hit));
                        //Debug.Log ("Damage done to " + rootObject.name + ": " + damagePoints + ". Remaining: " + health.health);

                        // Do shock displacement
                        // if target has rigidbody, apply displacement force to rigidbody
                        var rigidbody = rootObject.GetComponent<Rigidbody>();
                        if (rigidbody != null) {
                            Vector3 displacementDirection = cannonballCenterToTankCenter.normalized;
                            //Debug.Log (string.Format ("Displacement stats: direction={0}, magnitude={1}", displacementDirection, damagePoints));

                            rigidbody.AddForce(rigidbody.mass * (displacementDirection * damagePoints * 0.8f), ForceMode.Impulse);  // Force = mass * accel
                        }

                    }
                }
            }
        }

        // perform terrain deformation (if terrain was hit)
        var terrainManager = collision.gameObject.GetComponent<TerrainDeformationManager>();
        if (terrainManager != null) {
            var deformationPrefab = PrefabRegistry.singleton.GetPrefab<DeformationKind>(deformationKind);
            SingedMessages.SendPlayAudioClip(
                PrefabRegistry.GetResourceName<ProjectileSoundKind>(ProjectileSoundKind.projectile_explo));
            //Debug.Log("CmdExplode instantiate deformation: " + deformationPrefab);
            GameObject deformation = Instantiate(deformationPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(deformation);
            // determine deformation seed
            var seed = Random.Range(1, 1 << 24);
            // execute terrain deformation on client
            terrainManager.RpcApplyDeform(deformation, seed);
        }

        // instantiate explosion
        var explosionPrefab = PrefabRegistry.singleton.GetPrefab<ExplosionKind>(explosionKind);
        //Debug.Log("CmdExplode instantiate explosion: " + explosionPrefab);
        GameObject explosion = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(explosion);

        // notify manager
        manager.ServerHandleExplosion(explosion);

        // set explosion duration (destroy after duration)
        var explosionController = explosion.GetComponent<ExplosionController>();
        var explosionDuration = (explosionController != null) ? explosionController.duration : 3.0f;
        Destroy(explosion, explosionDuration);

        // destroy the projectile on collision
        Destroy(gameObject, .2f);
        //NetworkServer.Destroy(gameObject);

    }

    public void DisableCollisions(float timer) {
        //Debug.Log("Collisions Disabled");
        // disable collisions
        var rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.detectCollisions = false;
            // start timer to re-enable
            StartCoroutine(EnableCollisionTimer(timer));
        }
    }

    IEnumerator EnableCollisionTimer(float timer) {
        while (timer > 0) {
            timer -= Time.deltaTime;

            // wait until next frame
            yield return null;
        }

        // enable collisions
        //Debug.Log("Collisions Enabled");
        GetComponent<Rigidbody>().detectCollisions = true;

        if (clusterBomblet != null) {
            if (isServer) {
                //Debug.Log("enabling cluster split coroutine");
                yield return StartCoroutine(ServerClusterSplit());
            }
        }
    }

    IEnumerator ServerClusterSplit() {
        float terrainY = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight(transform.position);
        // adding a little buffer here... the logic isn't correct, and should be handled by collider, but hitting points where it isn't working

        // Debug.Log("terrainY is " + terrainY);
        var isAscending = true;
        while (isAscending) {
            isAscending = (rb.velocity.y >= 0.0f);
            // wait for next frame
            yield return null;
        }
        //Debug.Log("descending");

        // intantiate explosion for cluster split
        // NOTE: do not notify turn manager of this explosion, as it is not the final explosion for the cluster
        var explosionPrefab = PrefabRegistry.singleton.GetPrefab<ExplosionKind>(explosionKind);
        GameObject explosion = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(explosion);
        var explosionController = explosion.GetComponent<ExplosionController>();
        var explosionDuration = (explosionController != null) ? explosionController.duration : 3.0f;
        Destroy(explosion, explosionDuration);

        for (int i = 0; i < numberOfBomblets; i++) {
            // instantiate cluster bomblet ...
            GameObject bomblet = GameObject.Instantiate(clusterBomblet, transform.position, transform.rotation);
            NetworkServer.Spawn(bomblet);
            // register first bomblet w/ turn manager
            TurnManager.singleton.ServerHandleShotFired(null, bomblet.gameObject);

            // assign initial velocity equal to parent
            Rigidbody bombletRB = bomblet.GetComponent<Rigidbody>();
            bombletRB.velocity = rb.velocity;

            // add random bomblet spread
            bombletRB.AddForce(Random.Range(-bomletForceKick, bomletForceKick), Random.Range(-bomletForceKick, bomletForceKick) * 0.5f, Random.Range(-bomletForceKick, bomletForceKick));

            // copy state from original projectile
            var newController = bomblet.GetComponent<ProjectileController>();
            if (newController != null) {
                newController.shooter = shooter;
            }
        }

        // hide original projectile
        foreach (Transform child in transform) {
            if (!child.gameObject.name.Contains("Player")) {
                child.gameObject.SetActive(false);
            }
        }
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        yield return null;
    }

    // Update is called once per frame
    void Update() {
        float terrainY = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight(transform.position);
        if (transform.position.y < terrainY - 1f) {  // this used to be before if(clusterBomblet) (testing for cluster bomb issues)
            NetworkServer.Destroy(gameObject);
        }
        // Make sure the projectile always points in the direction it travels.
        //Vector3 vel = GetComponent<Rigidbody>().velocity;
        //transform.rotation = Quaternion.LookRotation(vel);
    }
}
