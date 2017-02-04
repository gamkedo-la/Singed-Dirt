using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

	Terrain terrain;
	public GameObject explosion;

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

	/// <summary>
	/// Called from the client, executed on the server
	/// Generate Explosion for current projectile
	/// </summary>
	[Command]
	void CmdExplode() {
		Debug.Log ("Your head asplode");

		//foreach (TankController tank in turnMgr.instance.tanks) {
		foreach (TankController tank in TurnManager.instance.tanks) {
			Debug.Log("Testing for splash damage: " + tank.name);
		}
		GameObject fire = Instantiate (explosion, gameObject.transform.position, Quaternion.identity) as GameObject;
		NetworkServer.Spawn(fire);
		Destroy (fire, 5);
	}

	// Update is called once per frame
	void Update () {
		float terrainY = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight (transform.position);
		if (transform.position.y < terrainY) {
			Destroy (gameObject);
		}
	}
	void OnDestroy(){
		/*
		if (hasAuthority) {
			CmdExplode();
		}
		*/
	}
}
