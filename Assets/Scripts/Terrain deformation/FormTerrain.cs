using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormTerrain : MonoBehaviour {

	public TerrainDeformationManager tdManager;
	public GameObject[] shortTerrainShapes;
	public GameObject[] midTerrainShapes;
	public GameObject[] tallTerrainShapes;
	public int shortTerrainSpawnCount = 0;
	public int midTerrainSpawnCount = 0;
	public int tallTerrainSpawnCount = 0;
	public Transform[] shortSpawnBox;
	public Transform[] midSpawnBox;
	public Transform[] tallSpawnBox;

	// Use this for initialization
	void Start () {
		SpawnTerrain ();
	}

	IEnumerator SpawnTerrainType(int terrainCount, GameObject[] terrainShapes, Transform[] spawnBoxes){
		for (int i = 0; i < terrainCount; i++) {
			yield return new WaitForSeconds (0.5f);
			GameObject tempGO = GameObject.Instantiate (terrainShapes [Random.Range(0, terrainShapes.Length)]);
			Vector3 randInSpawnBox;
			randInSpawnBox = new Vector3 (Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f));
			randInSpawnBox = spawnBoxes[Random.Range(0, spawnBoxes.Length)].TransformPoint (randInSpawnBox * 0.5f);
			tdManager.ApplyDeform (tempGO.GetComponent<TerrainDeformer>(), randInSpawnBox);
		}
	}
	
	// Update is called once per frame
	void SpawnTerrain () {
		if (shortTerrainSpawnCount > 0) {
			StartCoroutine (SpawnTerrainType (shortTerrainSpawnCount, shortTerrainShapes, shortSpawnBox));
		}
		if (midTerrainSpawnCount > 0) {
			StartCoroutine (SpawnTerrainType (midTerrainSpawnCount, midTerrainShapes, midSpawnBox));
		}
		if (tallTerrainSpawnCount > 0) {
			StartCoroutine (SpawnTerrainType (tallTerrainSpawnCount, tallTerrainShapes, tallSpawnBox));
		}

	}
}
