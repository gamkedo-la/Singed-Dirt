using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Reflection;
using System.Collections.Generic;

public static class Extensions {
    /// <summary>
    /// Get the string slice between the two indexes.
    /// Inclusive for start index, exclusive for end index.
    /// </summary>
    public static string Slice(
        this string source,
        int start,
        int end
    ) {
        if (end < 0) {                       // Keep this for negative end support
            end = source.Length + end;
        }
        int len = end - start;               // Calculate length
        return source.Substring(start, len); // Return Substring of length
    }
}

/// <summary>
/// The following sets of enums represent different prefabs under the Resources directory.
/// Each Enum represents a separate subdirectory, and the naming of the subdirectory must
/// match the name of the Enum minus "Kind" (e.g.: ProjectileKind expects there to be a
/// Resources/Projectile/ subdirectory.  Under the subdirectory, the names of the prefabs
/// must match the Enum values (e.g.: For ProjectileKind.cannonBall, there should be a
/// prefab named Resources/Projectile/cannonBall.prefab

/// NOTE: for prefabs that must be spawned over the network, ensure that spawnableEnums is
/// updated appropriately
/// </summary>

public enum ProjectileKind {
    cannonBall = 0,
    acorn,
}

public enum ExplosionKind {
    fire = 0,
}

public enum DeformationKind {
    shotCrater = 0,
}

public enum TankBaseKind {
    standard = 0,
    crocodile,
    squirrel,
}

public enum TankTurretBaseKind {
    standard = 0,
    crocodile,
    squirrel,
}

public enum TankTurretKind {
    standard = 0,
    crocodile,
    squirrel,
}

public enum TankHatKind {
    sunBlack = 0,
    sunBlue,
    sunGreen,
    sunRed,
    sunYellow,
    sunWhite,
	horn,
}

/// <summary>
/// A singleton registry class providing a cache and access methods to retrieve object prefabs based on
/// enum identifiers.
/// </summary>
public class PrefabRegistry: NetworkBehaviour {

    static Type[] spawnableEnums = new Type[] {
        typeof(ProjectileKind),
        typeof(ExplosionKind),
        typeof(DeformationKind)
    };

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

    void Start() {
        Debug.Log("PrefabRegistry Start, isServer" + isServer);
        if (!isServer) {
            foreach(var spawnEnum in spawnableEnums) {
                foreach (var prefabId in Enum.GetValues(spawnEnum)) {
                    // use reflection to generate generic method given spawnEnum type
                    var prefab = typeof(PrefabRegistry)
                        .GetMethod("GetPrefab")
                        .MakeGenericMethod(spawnEnum)
                        .Invoke(this, new object[] { prefabId }) as GameObject;
                    if (prefab != null) {
                        Debug.Log("registering prefab: " + prefabId);
                        ClientScene.RegisterPrefab(prefab);
                    }
                }
            }
        }
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
            Debug.Log("failed to load prefab for: " + name);
            return null;
        }

        // cache and return result
        prefabCache[name] = prefabGO;
        return prefabGO;
    }

    public GameObject GetPrefab<T>(T prefabID) {
        var name = typeof(T).ToString().Slice(0,-4) + "/" + prefabID.ToString();
        return GetPrefab(name);
    }

}
