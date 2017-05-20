using System;
using System.Linq;
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
        return (spawnPoint - point).magnitude < range;
    }
}

/// <summary>
/// Class used to control spawning of loot boxes in world
/// </summary>
public class LootSpawnController : NetworkBehaviour {
    public static LootSpawnController singleton;

    [Range(5f, 25f)]
    public float minSpacing = 10f;
    [Range(5, 30)]
    public int numInitialSpawn = 10;
    [Range(1, 10)]
    public int maxPerRound = 5;
    [Range(10, 50)]
    public int maxLootBoxes = 30;
    [Range(1, 5)]
    public int minAmmoCount = 1;
    [Range(1, 25)]
    public int maxAmmoCount = 5;

    public ProjectileKind[] excludedProjectiles = new ProjectileKind[] {
        ProjectileKind.sharkToothBomblet
    };

    public int mushboomCount = 0,
        minMushbooms = 2;
    public float maxPercentMushbooms = 0.2f;

    ISpawnGenerator locationGenerator;
    float startWidth = 256f;
    float startHeight = 256f;
    List<Exclusion> exclusions;
    int activeLootBoxes;

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

    public void ServerSpawnInit() {
        ServerSpawnN(numInitialSpawn);
    }

    public void ServerSpawnRound() {
        var num = UnityEngine.Random.Range(0, maxPerRound);
        minMushbooms = Math.Min((int)(maxLootBoxes * maxPercentMushbooms), 2 * TurnManager.singleton.currentRound);
        ServerSpawnN(num);
    }

    public void ServerSpawnN(int n) {
        // only spawn up to max loot boxes
        n = Mathf.Min(maxLootBoxes - activeLootBoxes, n);
        if (n <= 0) return;

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
            var finalPosition = GroundPosition(location) + new Vector3(0, 1, 0);

            // spawn lootbox
            var lootboxPrefab = PrefabRegistry.singleton.GetPrefab<SpawnKind>(SpawnKind.lootbox);
            GameObject lootboxGo = Instantiate(lootboxPrefab, finalPosition, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(lootboxGo);

            // actually assign loot
            ProjectileKind ammoKind;
            while (true) {
                ammoKind = FindAmmoKind();
                if (!excludedProjectiles.Contains(ammoKind)) {
                    break;
                }
            }
            var ammoAmount = UnityEngine.Random.Range(minAmmoCount, maxAmmoCount + 1);
            switch (ammoKind) {
                case ((ProjectileKind)6):
                    ammoAmount = 1;
                    break;
                case ((ProjectileKind)7):
                    mushboomCount++;
                    ammoAmount = 1;
                    break;
            }
            lootboxGo.GetComponent<LootBoxController>().AssignLoot(ammoKind, ammoAmount);
            var health = lootboxGo.GetComponent<Health>();
            if (health != null) {
                health.onDeathEvent.AddListener(OnLootBoxDestroy);
            }

            // increment # of active loot boxes that are tracked
            activeLootBoxes++;
        }
    }

    private ProjectileKind FindAmmoKind() {
        if (mushboomCount < minMushbooms) {
            return (ProjectileKind)7;
        }
        int range = System.Enum.GetValues(typeof(ProjectileKind)).Length;
        return (ProjectileKind)UnityEngine.Random.Range(1, range--);
    }

    void OnLootBoxDestroy(GameObject from) {
        // decr # of active loot boxes that are tracked
        activeLootBoxes--;
    }

}
