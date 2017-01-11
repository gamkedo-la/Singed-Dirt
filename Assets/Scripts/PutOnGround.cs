using UnityEngine;
using System.Collections;

public class PutOnGround : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		Vector3 fixedSpot = transform.position;
		fixedSpot.y = Terrain.activeTerrain.SampleHeight(fixedSpot) + Terrain.activeTerrain.transform.position.y;
		transform.position = fixedSpot;
	}
}
