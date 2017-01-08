using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

	Terrain terrain;
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
}
