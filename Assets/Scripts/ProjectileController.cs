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

	public GameObject clusterBomblet;

	public float clusterHeight = 20.0f;
  	private bool passedClusterHeight = false;
	private Rigidbody rb;
    public float bomletForceKick = 50.0f;
    public int numberOfBomblets = 8;

	private Vector3 startPos;

    // Use this for initialization
    void Start () {
		terrain = Terrain.activeTerrain;
		manager = TurnManager.GetGameManager();
		DisableCollisions(0.2f);
		rb = GetComponent<Rigidbody>();
		startPos = transform.position;
	}

	void OnCollisionEnter(Collision collision){
		Debug.Log("ProjectileController OnCollisionEnter with: " + collision.collider.name);
		// only trigger explosion (spawn) if we currently have authority
		// run collisions on server only
		if (isServer && !hasCollided) {
			// single collision/explosion per projectile
			hasCollided = true;
			ServerExplode(collision);
		}
	}

    // ------------------------------------------------------
    // SERVER-ONLY METHODS

	/// <summary>
	/// Generate explosion and apply damage for projectile
	/// </summary>
	void ServerExplode(Collision collision) {
		Debug.Log("ServerExplode: " + this);
		// Get list of colliders in range of this explosion
		// FIXME: range of projectile shouldn't be hard-coded
		Collider[] flakReceivers = Physics.OverlapSphere(transform.position, 10.0f);
		// keep track of list of root objects already evaluated
		var hitList = new List<GameObject>();

		foreach (Collider flakReceiver in flakReceivers) {
			var rootObject = flakReceiver.transform.root.gameObject;

			// has this object already been hit by this projectile?
			if (!hitList.Contains(rootObject)) {
				hitList.Add(rootObject);

				GameObject gameObjRef = flakReceiver.gameObject;
				Debug.Log("hit gameObject: " + rootObject.name);
				var health = rootObject.GetComponent<Health>();
				var tankCtrlRef = rootObject.GetComponent<TankController>();
				if (health != null && tankCtrlRef != null) {
					Debug.Log (tankCtrlRef.playerName + " received splash damage");

					Vector3 cannonballCenterToTankCenter = transform.position - gameObjRef.transform.position;
					Debug.Log (string.Format ("cannonball position: {0}, tank position: {1}", transform.position, gameObjRef.transform.position));
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
					if (damagePoints > 0) {
						health.TakeDamage(damagePoints);
						Debug.Log ("Damage done to " + tankCtrlRef.name + ": " + damagePoints + ". Remaining: " + health.health);

						// Do shock displacement
						Vector3	displacementDirection = cannonballCenterToTankCenter.normalized;
						Debug.Log (string.Format ("Displacement stats: direction={0}, magnitude={1}", displacementDirection, damagePoints));
						tankCtrlRef.rb.AddForce (tankCtrlRef.rb.mass * (displacementDirection * damagePoints * 0.8f), ForceMode.Impulse);	// Force = mass * accel

					}
				}
			}
		}

		// perform terrain deformation (if terrain was hit)
		var terrainManager = collision.gameObject.GetComponent<TerrainDeformationManager>();
		if (terrainManager != null) {
			var deformationPrefab = PrefabRegistry.singleton.GetPrefab<DeformationKind>(deformationKind);
			Debug.Log("CmdExplode instantiate deformation: " + deformationPrefab);
			GameObject deformation = Instantiate(deformationPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
			NetworkServer.Spawn(deformation);
			// determine deformation seed
			var seed = Random.Range(1, 1<<24);
			// execute terrain deformation on client
			terrainManager.RpcApplyDeform(deformation, seed);
		}

		// instantiate explosion
		var explosionPrefab = PrefabRegistry.singleton.GetPrefab<ExplosionKind>(explosionKind);
		Debug.Log("CmdExplode instantiate explosion: " + explosionPrefab);
		GameObject explosion = Instantiate (explosionPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
		NetworkServer.Spawn(explosion);

		// notify manager
		manager.ServerHandleExplosion(explosion);

		// set explosion duration (destroy after duration)
		var explosionController = explosion.GetComponent<ExplosionController>();
		var explosionDuration = (explosionController != null) ? explosionController.duration : 3f;
		Destroy (explosion, explosionDuration);

		// destroy the projectile on collision
		Destroy(gameObject, .2f);
		//NetworkServer.Destroy(gameObject);

	}

	public void DisableCollisions(float timer) {
		Debug.Log("Collisions Disabled");
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
		Debug.Log("Collisions Enabled");
		GetComponent<Rigidbody>().detectCollisions = true;
	}

	// Update is called once per frame
	void Update () {
		float terrainY = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight (transform.position);
		// adding a little buffer here... the logic isn't correct, and should be handled by collider, but hitting points where it isn't working
		if (transform.position.y < terrainY - 1f) {
			NetworkServer.Destroy (gameObject);
		}
		// Debug.Log("terrainY is " + terrainY);
		if(clusterBomblet != null){
			float shotHeightAboveTerrain = transform.position.y - terrainY;
			Debug.Log("shotHeightAboveTerrain is " + shotHeightAboveTerrain);
			if(rb.velocity.y < 0.0f && passedClusterHeight == false){
				clusterHeight = (startPos.y + transform.position.y) / 2.0f - terrainY;
				passedClusterHeight = true;
			}
			if(rb.velocity.y < 0.0f && shotHeightAboveTerrain < clusterHeight && passedClusterHeight){
				var explosionPrefab = PrefabRegistry.singleton.GetPrefab<ExplosionKind>(explosionKind);
				GameObject explosion = Instantiate (explosionPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
				NetworkServer.Spawn(explosion);
				manager.ServerHandleExplosion(explosion);
				var explosionController = explosion.GetComponent<ExplosionController>();
				var explosionDuration = (explosionController != null) ? explosionController.duration : 3f;
				Destroy (explosion, explosionDuration);
				Destroy(gameObject, 0.2f);
				for(int i = 0; i < numberOfBomblets; i++){
					GameObject bomblet = GameObject.Instantiate(clusterBomblet, transform.position, transform.rotation);
					NetworkServer.Spawn(bomblet);
					// if (TurnManager.singleton != null) TurnManager.singleton.ServerHandleShotFired(bomblet);  # This didn't help
					Rigidbody bombletRB = bomblet.GetComponent<Rigidbody>();
					bombletRB.AddForce(Random.Range(-bomletForceKick, bomletForceKick), Random.Range(-bomletForceKick, bomletForceKick) * 0.5f, Random.Range(-bomletForceKick, bomletForceKick));
					
					// preventing MULTIBOMBS (too many spawn during multiple frames)
				}
				clusterBomblet = null;
			}
    	}

        // Make sure the projectile always points in the direction it travels.
        //Vector3 vel = GetComponent<Rigidbody>().velocity;
        //transform.rotation = Quaternion.LookRotation(vel);
	}
}
