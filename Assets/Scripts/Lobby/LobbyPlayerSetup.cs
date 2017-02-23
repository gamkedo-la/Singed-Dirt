using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSetup : MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    // empty game objects marking parent location for each part of the tank
    public GameObject tankBase;
    public GameObject turretBase;
    public GameObject turret;
    public GameObject hat;

    public TankBaseKind tankBaseKind = TankBaseKind.standard;
    public TankTurretBaseKind turretBaseKind = TankTurretBaseKind.standard;
    public TankTurretKind turretKind = TankTurretKind.standard;
    public TankHatKind hatKind = TankHatKind.sunBlue;

    // instantiated prefabs for each part of the tank
    public GameObject tankBasePrefab = null;
    public GameObject turretBasePrefab = null;
    public GameObject turretPrefab = null;
    public GameObject hatPrefab = null;

    public void Start() {
        UpdateAvatar();
    }

    GameObject LocalInstantiate(GameObject prefab, Transform parent) {
		var prefabInstance = (GameObject) GameObject.Instantiate(prefab, parent);
        //prefabInstance.transform.rotation = Quaternion.AngleAxis(-130, Vector3.up);
        //prefabInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        return prefabInstance;
    }

    public void UpdateAvatar() {
        // instantiate tank base
        if (tankBasePrefab != null) DestroyImmediate(tankBasePrefab);
        var prefab = PrefabRegistry.singleton.GetPrefab<TankBaseKind>(tankBaseKind);
		tankBasePrefab = LocalInstantiate(prefab, tankBase.transform);

        // instantiate turret base
        if (turretBasePrefab != null) DestroyImmediate(turretBasePrefab);
        prefab = PrefabRegistry.singleton.GetPrefab<TankTurretBaseKind>(turretBaseKind);
		turretBasePrefab = LocalInstantiate(prefab, turretBase.transform);

        // instantiate turret
        if (turretPrefab != null) DestroyImmediate(turretPrefab);
        prefab = PrefabRegistry.singleton.GetPrefab<TankTurretKind>(turretKind);
		turretPrefab = LocalInstantiate(prefab, turret.transform);

        // instantiate hat
        if (hatPrefab != null) DestroyImmediate(hatPrefab);
        prefab = PrefabRegistry.singleton.GetPrefab<TankHatKind>(hatKind);
		hatPrefab = LocalInstantiate(prefab, hat.transform);

        transform.rotation *= Quaternion.AngleAxis(-130, Vector3.up);

    }
}
