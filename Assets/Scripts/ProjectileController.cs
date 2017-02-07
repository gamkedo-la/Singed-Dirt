using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

	Terrain terrain;
	public GameObject explosion;

	public enum shotKind {
		Standard, Cluster, Fire, End
	}
	public GameObject[] explosionKind;
	public GameObject[] projectileShape;

	shotKind myKind = shotKind.Standard;
	GameObject shape;
	// Use this for initialization
	void Start () {
		terrain = Terrain.activeTerrain;
	}

	void OnCollisionEnter(Collision coll){
		Debug.Log("ProjectileController OnCollisionEnter with: " + coll.collider.name);
		// only trigger explosion (spawn) if we currently have authority
		if (hasAuthority) {
			CmdExplode();
		}
	}

	public void SetupShot(shotKind kind, Transform shotSource){
		myKind = kind;
		shape = (GameObject)Instantiate (projectileShape [(int)myKind], shotSource.position, shotSource.rotation);
		shape.transform.SetParent (transform);
		NetworkServer.Spawn (shape);
		Debug.Log ("New shot kind is: " + myKind);
	}

	/// <summary>
	/// Called from the client, executed on the server
	/// Generate Explosion for current projectile
	/// </summary>
	[Command]
	void CmdExplode() {
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

		GameObject fire = Instantiate (explosionKind[(int)myKind], gameObject.transform.position, Quaternion.identity) as GameObject;
		NetworkServer.Spawn(fire);
		//NetworkServer.Destroy (shape);
		Destroy (fire, 5);
	}

	// Update is called once per frame
	void Update () {
		float terrainY = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight (transform.position);
		if (transform.position.y < terrainY) {
			NetworkServer.Destroy (gameObject);
		}
	}
}
