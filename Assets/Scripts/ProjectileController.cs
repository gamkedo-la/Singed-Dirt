using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileController : MonoBehaviour {

	Terrain terrain;
	public GameObject explosion;
	// Use this for initialization
	void Start () {
		terrain = Terrain.activeTerrain;
	}

	void OnCollisionEnter(Collision coll){
		Debug.Log (coll.collider.name);
	}
	
	// Update is called once per frame
	void Update () {
		float terrainY = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight (transform.position);
		if (transform.position.y < terrainY) {
			Destroy (gameObject);
		}
	}

	void OnDestroy(){
		GameObject fire = Instantiate (explosion, gameObject.transform.position, Quaternion.identity) as GameObject;
		NetworkServer.Spawn (fire);
		Destroy (fire, 5);
	}
}
