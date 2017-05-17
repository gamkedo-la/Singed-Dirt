using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using TyVoronoi;

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

	public bool useVoronoi = true;
	public bool rollingTerrain = false;
	[Range (0.0001f,0.001f)]
	public float noiseFrequency = 0.001f;
	[Range (0.1f,0.25f)]
	public float noiseAmplitude = 0.1f;
	[Range (1f,10f)]
	public float maxDrift = 5f;

	Edge[] edgeList;
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
		return (1.0f + noise) * 0.3f;
	}

    // ------------------------------------------------------
    // SERVER->CLIENT METHODS

	/// <summary>
	/// The actual work of terrain generation happens on the client, initialized w/ seed value passed from server
	/// </summary>
	[ClientRpc]
	void RpcSpawnTerrain (int seed) {
		// Debug.Log("RpcSpawnTerrain with seed: " + seed);

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

		Debug.Log("RpcSpawnTerrain done");
	}

    // ------------------------------------------------------
    // STATE MACHINES

	/// <summary>
	/// handle terrain deformation spawning
	/// </summary>
	IEnumerator SpawnTerrainTypes(){
		// determine spawn generator(s) to use
		ISpawnGenerator shortGenerator;
		ISpawnGenerator midGenerator;
		ISpawnGenerator tallGenerator;
		if (useVoronoi) {
			// compute voronoi graph around spawn points
			var maxX = 200f;
			var maxZ = 200f;
	        var terrain = Terrain.activeTerrain;
	        if (terrain != null) {
	            maxX = terrain.terrainData.size.x;
	            maxZ = terrain.terrainData.size.z;
	        }
			shortGenerator = new VoronoiSpawnGenerator(
				TurnManager.singleton.spawnPoints.ToArray(),
				15f, 10f, maxX, maxZ);
			tallGenerator = shortGenerator;
			var voronoiBisectorGenerator = new VoronoiBisectorSpawnGenerator(
				TurnManager.singleton.spawnPoints.ToArray(),
				maxX, maxZ);
			midGenerator = voronoiBisectorGenerator;
			// override midTerrainSpawnCount to equal # of voronoi edges
			midTerrainSpawnCount = voronoiBisectorGenerator.voronoi.edgeList.Count;
			Debug.Log("mid count: " + midTerrainSpawnCount);
		} else {
			shortGenerator = new SpawnBoxSpawnGenerator(shortSpawnBox);
			midGenerator = new SpawnBoxSpawnGenerator(midSpawnBox);
			tallGenerator = new SpawnBoxSpawnGenerator(tallSpawnBox);
		}

		if (shortTerrainSpawnCount > 0) {
			SpawnTerrainType (shortTerrainSpawnCount, shortTerrainShapes, shortGenerator);
		}
		if (midTerrainSpawnCount > 0) {
			SpawnTerrainType (midTerrainSpawnCount, midTerrainShapes, midGenerator);
		}
		if (tallTerrainSpawnCount > 0) {
			SpawnTerrainType (tallTerrainSpawnCount, tallTerrainShapes, tallGenerator);
		}
		yield return null;
		Debug.Log("SpawnTerrainTypes done");
	}

	//IEnumerator SpawnTerrainType(int terrainCount, GameObject[] terrainShapes, Transform[] spawnBoxes){
	void SpawnTerrainType(int terrainCount, GameObject[] terrainShapes, ISpawnGenerator spawnGenerator){
		// generate spawn points
		var spawnPoints = spawnGenerator.Generate(terrainCount);

		for (int i = 0; i < terrainCount; i++) {
			GameObject tempGO = GameObject.Instantiate (terrainShapes [Random.Range(0, terrainShapes.Length)]);
			var spawnLocation = spawnPoints[i];
			// pick seed for each deformation
			var seed = Random.Range(1, 1<<24);
			tdManager.ApplyDeform (tempGO.GetComponent<TerrainDeformer>(), spawnLocation, seed);
		}
	}

}
