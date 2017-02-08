using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

	Terrain terrain;
	TurnManager manager;

	ExplosionKind explosionKind = ExplosionKind.fire;

	// Use this for initialization
	void Start () {
		terrain = Terrain.activeTerrain;
		manager = TurnManager.GetGameManager();
		DisableCollisions(0.2f);
	}

	void OnCollisionEnter(Collision coll){
		Debug.Log("ProjectileController OnCollisionEnter with: " + coll.collider.name);
		// only trigger explosion (spawn) if we currently have authority
		if (hasAuthority) {
			CmdExplode();
		}
	}

	/// <summary>
	/// Called from the client, executed on the server
	/// Generate Explosion for current projectile
	/// </summary>
	[Command]
	void CmdExplode() {
		Debug.Log("CmdExplode: " + this);
		// Get list of colliders in range of this explosion
		Collider[] flakReceivers = Physics.OverlapSphere(transform.position, 10.0f);

		foreach (Collider flakReceiver in flakReceivers) {
			GameObject gameObjRef = flakReceiver.gameObject;
			// TODO use a better method to identify Player tank objects?
			if (gameObjRef.name.Contains ("Player") && !gameObjRef.name.Contains("Projectile")) {
				Debug.Log (gameObjRef.name + " received splash damage");

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
				TankController tankCtrlRef = gameObjRef.GetComponent<TankController> ();
				tankCtrlRef.hitPoints -= damagePoints;
				Debug.Log ("Damage done to " + name + ": " + damagePoints + ". Remaining: " + tankCtrlRef.hitPoints);

				// Do shock displacement
				Vector3	displacementDirection = cannonballCenterToTankCenter.normalized;
				Debug.Log (string.Format ("Displacement stats: direction={0}, magnitude={1}", displacementDirection, damagePoints));
				tankCtrlRef.rb.AddForce (tankCtrlRef.rb.mass * (displacementDirection * damagePoints * 0.8f), ForceMode.Impulse);	// Force = mass * accel


				// Destroy the tank (TODO - the tank should do this on its own)
				if (tankCtrlRef.hitPoints <= 0) {
					Destroy (gameObjRef);
					TurnManager.instance.GameOverMan (true);
				}
			}
		}

		// instantiate explosion
		var explosionPrefab = PrefabRegistry.singleton.GetExplosion(explosionKind);
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
		NetworkServer.Destroy(gameObject);

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
		if (transform.position.y < terrainY) {
			NetworkServer.Destroy (gameObject);
		}
	}
}
