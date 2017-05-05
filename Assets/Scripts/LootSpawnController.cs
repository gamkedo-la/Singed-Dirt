using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

struct Exclusion {
    public Vector3 point;
    public float range;
    public Exclusion(
        Vector3 point,
        float range
    ) {
        this.point = point;
        this.range = range;
    }
    public bool ExcludeSpawn(Vector3 spawnPoint) {
        return (spawnPoint-point).magnitude < range;
    }
}

/// <summary>
/// Class used to control spawning of loot boxes in world
/// </summary>
public class LootSpawnController : NetworkBehaviour {
	public static LootSpawnController singleton;

    public float minSpacing = 10f;
    public int maxPerSpawn = 5;

    ISpawnGenerator locationGenerator;
    float startWidth = 256f;
    float startHeight = 256f;

    List<Exclusion> exclusions;

    void Awake() {
        singleton = this;
        exclusions = new List<Exclusion>();
    }

    void Start() {
        var terrain = Terrain.activeTerrain;
        if (terrain != null) {
            startWidth = terrain.terrainData.size.x;
            startHeight = terrain.terrainData.size.z;
        }
        locationGenerator = new RandomSpawnGenerator(minSpacing, startWidth, startHeight);
    }

    Vector3 GroundPosition(Vector3 position) {
		var groundPosition = new Vector3(
			position.x,
			Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y,
			position.z
		);
		return groundPosition;
	}

    public void AddExclusion(Vector3 position, float range) {
        // normalize position
        exclusions.Add(new Exclusion(new Vector3(position.x, 0, position.z), range));
    }

    public void ServerSpawnN(int n) {
        // generate spawn locations
        var locations = locationGenerator.Generate(n);

        // iterate through spawn locations
        foreach (var location in locations) {
            // check exclusions
            var excluded = false;
            foreach (var exclusion in exclusions) {
                if (exclusion.ExcludeSpawn(location)) {
                    excluded = true;
                    break;
                }
            }
            if (excluded) {
                // too close
                Debug.Log("spawn excluded, too close to player");
                continue;
            }

            // get final ground position
            var finalPosition = GroundPosition(location) + new Vector3(0,1,0);

            // spawn lootbox
            var lootboxPrefab = PrefabRegistry.singleton.GetPrefab<SpawnKind>(SpawnKind.lootbox);
    		GameObject lootboxGo = Instantiate(lootboxPrefab, finalPosition, Quaternion.identity) as GameObject;
    		NetworkServer.Spawn(lootboxGo);
        }

    }

}
