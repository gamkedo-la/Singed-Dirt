using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	void OnCollisionEnter(Collision coll){
		Debug.Log (coll.collider.name);
	}
	
	// Update is called once per frame
	void Update () {
		float terrainY = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight (transform.position);
		if (transform.position.y < terrainY) {
			Debug.Log ("Ball tried to go underground but we stopped it!");
			Vector3 tempV3 = transform.position;
			tempV3.y = terrainY;
			transform.position = tempV3;
		}
	}
}
