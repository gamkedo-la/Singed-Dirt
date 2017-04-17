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
/// A singleton registry class providing a cache and access methods to retrieve object prefabs based on
/// enum identifiers.
/// Each Enum represents a separate subdirectory, and the naming of the subdirectory must
/// match the name of the Enum minus "Kind" (e.g.: ProjectileKind expects there to be a
/// Resources/Projectile/ subdirectory.  Under the subdirectory, the names of the prefabs
/// must match the Enum values (e.g.: For ProjectileKind.cannonBall, there should be a
/// prefab named Resources/Projectile/cannonBall.prefab
/// </summary>
public class PrefabRegistry {

    // singleton instance setup
    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    private static readonly PrefabRegistry instance = new PrefabRegistry();
    static PrefabRegistry() {}
    private PrefabRegistry() {
        prefabCache = new Dictionary<string, GameObject>();
    }
    /// <summary>
    /// public singleton property
    /// </summary>
    public static PrefabRegistry singleton {
        get { return instance; }
    }

    // the prefab cache
    private Dictionary<string, GameObject> prefabCache;

    public void LoadEnum(Type prefabEnum) {
        foreach (var prefabId in Enum.GetValues(prefabEnum)) {
            // use reflection to generate generic method given spawnEnum type
            var prefab = typeof(PrefabRegistry)
                    .GetMethod("GetPrefab")
                    .MakeGenericMethod(prefabEnum)
                    .Invoke(this, new object[] { prefabId }) as GameObject;
        // Debug.Log("loaded: " + prefabId);
        }
    }

    public IEnumerable<GameObject> GetAll() {
        return prefabCache.Values;
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
            // Debug.Log("failed to load prefab for: " + name);
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
