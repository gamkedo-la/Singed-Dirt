using System;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A singleton registry class providing a cache and access methods to retrieve object prefabs based on
/// enum identifiers.
/// </summary>
public class NetRegistry : NetworkBehaviour {

    static Type[] spawnableEnums = new Type[] {
        typeof(ProjectileKind),
        typeof(ExplosionKind),
        typeof(DeformationKind),
        typeof(AnyOneLinersKind),
        typeof(AcornOneLinersKind),
        typeof(BeetOneLinersKind),
        typeof(CannonOneLinersKind),
        typeof(MissileOneLinersKind),
        typeof(MushboomOneLinersKind),
        typeof(PillarOneLinersKind),
        typeof(SharktoothOneLinersKind),
        typeof(TeleportOneLinersKind),
        typeof(SpawnKind),
        typeof(MenuSoundKind),
        typeof(TankSoundKind),
        typeof(ProjectileSoundKind),
        typeof(MusicKind),
        typeof(MushboomSoundKind)
    };

    // singleton instance
    public static NetRegistry singleton;

    /// <summary>
    /// Client-only method used to register prefab instances into the client scene using the PrefabRegistry
    /// Each prefab in the registry gets added to allow network spawning of those objects
    /// from server to client
    /// </summary>
    void ClientRegisterPrefabs() {
        foreach (var spawnEnum in spawnableEnums) {
            PrefabRegistry.singleton.LoadEnum(spawnEnum);
        }
        foreach (var prefab in PrefabRegistry.singleton.GetAll()) {
            // Debug.Log("registering prefab: " + prefab.name);
            if (null != prefab.GetComponent<NetworkIdentity>()) {
                ClientScene.RegisterPrefab(prefab);
            }
        }
    }

    void Start() {
        if (singleton == null) {
            singleton = this;
            if (!isServer) {
                ClientRegisterPrefabs();
            }
        }
    }
}
