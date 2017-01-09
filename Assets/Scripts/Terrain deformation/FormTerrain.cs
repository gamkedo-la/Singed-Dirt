using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormTerrain : MonoBehaviour {

	public TerrainDeformationManager tdManager;
	public GameObject[] terrainShapes;
//	public Transform tempPos;
	public Transform[] SpawnBox;

	// Use this for initialization
	void Start () {
		StartCoroutine (SpawnTerrain ());
	}
	
	// Update is called once per frame
	IEnumerator SpawnTerrain () {
		for (int i = 0; i < 15; i++) {
			yield return new WaitForSeconds (0.5f);
			GameObject tempGO = GameObject.Instantiate (terrainShapes [0]);
			Vector3 randInSpawnBox;
			randInSpawnBox = new Vector3 (Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f));
			randInSpawnBox = SpawnBox[Random.Range(0, SpawnBox.Length)].TransformPoint (randInSpawnBox * 0.5f);
			tdManager.ApplyDeform (tempGO.GetComponent<TerrainDeformer>(), randInSpawnBox);
		}
	}
}
