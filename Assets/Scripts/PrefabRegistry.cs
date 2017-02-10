using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// List of all projectile prefabs.
/// NOTE: the names here must match a prefab in the Resources/Projectiles/ directory in order for registry to load.
/// </summary>
public enum ProjectileKind {
    cannonBall = 0,
    acorn,
}

/// <summary>
/// List of all explosion prefabs.
/// NOTE: the names here must match a prefab in the Resources/Explosions/ directory in order for registry to load.
/// </summary>
public enum ExplosionKind {
    fire = 0,
}

/// <summary>
/// List of all explosion prefabs.
/// NOTE: the names here must match a prefab in the Resources/Explosions/ directory in order for registry to load.
/// </summary>
public enum DeformationKind {
    shotCrater = 0,
}

/// <summary>
/// A singleton registry class providing a cache and access methods to retrieve object prefabs based on
/// enum identifiers.
/// </summary>
public class PrefabRegistry: MonoBehaviour {
    // singleton instance
    private static PrefabRegistry _singleton;
    // the prefab cache
    private Dictionary<string, GameObject> prefabCache;

    /// <summary>
    /// initialize
    /// </summary>
    void Awake() {
        prefabCache = new Dictionary<string, GameObject>();
    }

    /// <summary>
    /// public singleton property
    /// </summary>
    public static PrefabRegistry singleton {
        get {
            if (_singleton == null){
                GameObject go = new GameObject();
                _singleton = go.AddComponent<PrefabRegistry>();
            }
            return _singleton;
        }
    }

    /// <summary>
    /// Retrieve a prefab by name of the form #directory#/#prefab_name#
    /// If found, cache result
    /// </summary>
    GameObject GetPrefab(string name) {
        // check cache
        GameObject prefabGO;
        if (prefabCache.TryGetValue(name, out prefabGO)) {
            return prefabGO;
        }

        // otherwise... look up in registry and load
        prefabGO = Resources.Load(name) as GameObject;
        if (prefabGO == null) {
            Debug.Log("failed to load prefab for :" + name);
            return null;
        }

        // cache and return result
        prefabCache[name] = prefabGO;
        return prefabGO;
    }

    /// <summary>
    /// Lookup projectile prefab by ID, return prefab gameobject if found
    /// </summary>
    public GameObject GetProjectile(ProjectileKind prefabID) {
        name = "Projectiles/" + prefabID.ToString();
        return GetPrefab(name);
    }

    /// <summary>
    /// Lookup explosion prefab by ID, return prefab gameobject if found
    /// </summary>
    public GameObject GetExplosion(ExplosionKind prefabID) {
        name = "Explosions/" + prefabID.ToString();
        return GetPrefab(name);
    }

    /// <summary>
    /// Lookup deformation prefab by ID, return prefab gameobject if found
    /// </summary>
    public GameObject GetDeformation(DeformationKind prefabID) {
        name = "Deformations/" + prefabID.ToString();
        return GetPrefab(name);
    }

}
