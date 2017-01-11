using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormTerrain : MonoBehaviour {

	public TerrainDeformationManager tdManager;
	public GameObject[] terrainShapes;
	public int terrainSpawnCount = 1;
	public Transform[] SpawnBox;

	// Use this for initialization
	void Start () {
		StartCoroutine (SpawnTerrain ());
	}
	
	// Update is called once per frame
	IEnumerator SpawnTerrain () {
		for (int i = 0; i < terrainSpawnCount; i++) {
			yield return new WaitForSeconds (0.5f);
			GameObject tempGO = GameObject.Instantiate (terrainShapes [Random.Range(0, terrainShapes.Length)]);
			Vector3 randInSpawnBox;
			randInSpawnBox = new Vector3 (Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f));
			randInSpawnBox = SpawnBox[Random.Range(0, SpawnBox.Length)].TransformPoint (randInSpawnBox * 0.5f);
			tdManager.ApplyDeform (tempGO.GetComponent<TerrainDeformer>(), randInSpawnBox);
		}
	}
}
