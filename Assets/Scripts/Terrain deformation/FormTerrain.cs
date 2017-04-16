using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FormTerrain : NetworkBehaviour {

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

	public bool rollingTerrain = false;
	[Range (0.0001f,0.001f)]
	public float noiseFrequency = 0.001f;
	[Range (0.1f,0.25f)]
	public float noiseAmplitude = 0.1f;

	INoiseGenerator noiseGenerator;

	// Use this for initialization
	void Awake () {
		var generator = new FastNoise();
		generator.SetFrequency(noiseFrequency);
        generator.SetFractalOctaves(4);
		generator.SetNoiseType(FastNoise.NoiseType.PerlinFractal);
		noiseGenerator = generator;
	}

    // ------------------------------------------------------
    // SERVER-ONLY METHODS
	/// <summary>
	/// Terrain generation is started on the server
	/// </summary>
	public void ServerGenerate() {
		if (!isServer) return;
		// pick a seed
		var seed = Random.Range(1, 1<<24);

		// tell clients to start terrain build
		RpcSpawnTerrain(seed);
	}

	static float SampleHeight(int x, int y, INoiseGenerator generator) {
		//This creates the noise used for the ground.
		//The last value (8.0f) is the amp that defines (roughly) the maximum
		//and minimum vaule the ground varies from the surface level
		//return perlin.FractalNoise3D(pos.x, pos.y, pos.z, 4, 80.0f, 8.0f);
		var noise = generator.GetNoise(x, y);
		//if (x == 0 ) Debug.Log("noise: " + noise.ToString("F4"));
		return (1.0f + noise) * 0.3f;
	}

    // ------------------------------------------------------
    // SERVER->CLIENT METHODS

	/// <summary>
	/// The actual work of terrain generation happens on the client, initialized w/ seed value passed from server
	/// </summary>
	[ClientRpc]
	void RpcSpawnTerrain (int seed) {
		Debug.Log("RpcSpawnTerrain with seed: " + seed);

		// initialize terrain heights
		if (rollingTerrain) {
			noiseGenerator.SetSeed(seed);
		}
		float[,] heights = new float[tdManager.terrainWidth, tdManager.terrainHeight];
		for (var i=0; i<tdManager.terrainWidth; i++)
		for (var j=0; j<tdManager.terrainHeight; j++) {
			if (rollingTerrain) {
				heights[i,j] = 0.1f + SampleHeight(i,j, noiseGenerator) * noiseAmplitude;
			} else {
				heights[i,j] = 0.1f;
			}
		}
	    tdManager.SetTerrainHeights(heights);

		// initialize random state
        if (seed != 0) Random.InitState(seed);

		// start spawns
		StartCoroutine(SpawnTerrainTypes());

		/*
		if (shortTerrainSpawnCount > 0) {
			StartCoroutine (SpawnTerrainType (shortTerrainSpawnCount, shortTerrainShapes, shortSpawnBox));
		}
		if (midTerrainSpawnCount > 0) {
			StartCoroutine (SpawnTerrainType (midTerrainSpawnCount, midTerrainShapes, midSpawnBox));
		}
		if (tallTerrainSpawnCount > 0) {
			StartCoroutine (SpawnTerrainType (tallTerrainSpawnCount, tallTerrainShapes, tallSpawnBox));
		}
		*/
	}

    // ------------------------------------------------------
    // STATE MACHINES

	/// <summary>
	/// handle terrain deformation spawning
	/// </summary>
	IEnumerator SpawnTerrainTypes(){
		if (shortTerrainSpawnCount > 0) {
			//yield return SpawnTerrainType (shortTerrainSpawnCount, shortTerrainShapes, shortSpawnBox);
			SpawnTerrainType (shortTerrainSpawnCount, shortTerrainShapes, shortSpawnBox);
		}
		if (midTerrainSpawnCount > 0) {
			//yield return SpawnTerrainType (midTerrainSpawnCount, midTerrainShapes, midSpawnBox);
			SpawnTerrainType (midTerrainSpawnCount, midTerrainShapes, midSpawnBox);
		}
		if (tallTerrainSpawnCount > 0) {
			//yield return SpawnTerrainType (tallTerrainSpawnCount, tallTerrainShapes, tallSpawnBox);
			SpawnTerrainType (tallTerrainSpawnCount, tallTerrainShapes, tallSpawnBox);
		}
		yield return null;
	}

	//IEnumerator SpawnTerrainType(int terrainCount, GameObject[] terrainShapes, Transform[] spawnBoxes){
	void SpawnTerrainType(int terrainCount, GameObject[] terrainShapes, Transform[] spawnBoxes){
		for (int i = 0; i < terrainCount; i++) {
			GameObject tempGO = GameObject.Instantiate (terrainShapes [Random.Range(0, terrainShapes.Length)]);
			Vector3 randInSpawnBox;
			randInSpawnBox = new Vector3 (Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f));
			randInSpawnBox = spawnBoxes[Random.Range(0, spawnBoxes.Length)].TransformPoint (randInSpawnBox * 0.5f);
			// pick seed for each deformation
			var seed = Random.Range(1, 1<<24);
			tdManager.ApplyDeform (tempGO.GetComponent<TerrainDeformer>(), randInSpawnBox, seed);
			//yield return null;
		}
	}

}
