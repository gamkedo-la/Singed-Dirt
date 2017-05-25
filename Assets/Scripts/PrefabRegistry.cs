using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Reflection;
using System.Collections.Generic;

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
        resourceCache = new Dictionary<string, object>();
    }
    /// <summary>
    /// public singleton property
    /// </summary>
    public static PrefabRegistry singleton {
        get { return instance; }
    }

    // the prefab cache
    private Dictionary<string, object> resourceCache;

    public void LoadEnum(Type prefabEnum) {
        foreach (var prefabId in Enum.GetValues(prefabEnum)) {
            // use reflection to generate generic method given spawnEnum type
            var prefab = typeof(PrefabRegistry)
                    .GetMethod("GetPrefab")
                    .MakeGenericMethod(prefabEnum)
                    .Invoke(this, new object[] { prefabId }) as GameObject;
        }
    }

    public IEnumerable<GameObject> GetAll() {
        var goList = new List<GameObject>();
        foreach (var obj in resourceCache.Values) {
            goList.Add((GameObject) obj);
        }
        return goList;
    }

    /// <summary>
    /// Retrieve a resource by name of the form #enum_type#/#enum_value#
    /// If found, cache result
    /// </summary>
    public T GetResource<T>(string name) where T: class {
        // check cache
        object resource = null;
        if (resourceCache.TryGetValue(name, out resource)) {
            return (T) resource;
        }

        // otherwise... look up in registry and load
        if (typeof(T).Equals(typeof(object))) {
            // load generic resource
            resource = (object) Resources.Load(name);
        } else {
            resource = (object) (Resources.Load(name) as T);
        }
        if (resource == null) {
            // Debug.Log("failed to load prefab for: " + name);
            return null;
        }

        // cache and return result
        resourceCache[name] = resource;
        return (T) resource;
    }

    public static string GetResourceName<T>(T prefabID) {
        var typeStr = typeof(T).ToString();
        string name;
        if (typeStr.Substring(typeStr.Length-4) == "Kind") {
            name = typeof(T).ToString().Substring(0,typeStr.Length-4) + "/" + prefabID.ToString();
        } else {
            name = typeStr + "/" + prefabID.ToString();
        }
        return name;
    }

    /// <summary>
    /// Retrieve a prefab by enum value.
    /// If found, cache result
    /// </summary>
    public GameObject GetPrefab<T>(T prefabID) {
        var name = GetResourceName<T>(prefabID);
        return GetResource<GameObject>(name);
    }

}
