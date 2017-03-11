using UnityEngine;

public class PlayerSlim : MonoBehaviour {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public GameObject tankBase;
    public GameObject turretBase;
    public GameObject turret;
    public GameObject hat;

    public TankBaseKind tankBaseKind = TankBaseKind.standard;
    public TankTurretBaseKind turretBaseKind = TankTurretBaseKind.standard;
    public TankTurretKind turretKind = TankTurretKind.standard;
    public TankHatKind hatKind = TankHatKind.sunBlue;

    void Start() {
        PrefabRegistry.singleton.GetPrefab<TankBaseKind>(tankBaseKind);
        // instantiate tank base
        var prefab = PrefabRegistry.singleton.GetPrefab<TankBaseKind>(tankBaseKind);
        // Debug.Log("prefab: " + prefab);
		var instance = (GameObject) GameObject.Instantiate(prefab, tankBase.transform);
        // Debug.Log("instance: " + instance);

        // instantiate turret base
        prefab = PrefabRegistry.singleton.GetPrefab<TankTurretBaseKind>(turretBaseKind);
        // Debug.Log("prefab: " + prefab);
		instance = (GameObject) GameObject.Instantiate(prefab, turretBase.transform);
        // Debug.Log("instance: " + instance);

        // instantiate turret
        prefab = PrefabRegistry.singleton.GetPrefab<TankTurretKind>(turretKind);
        // Debug.Log("prefab: " + prefab);
		instance = (GameObject) GameObject.Instantiate(prefab, turret.transform);
        // Debug.Log("instance: " + instance);

        // instantiate hat
        prefab = PrefabRegistry.singleton.GetPrefab<TankHatKind>(hatKind);
        // Debug.Log("prefab: " + prefab);
		instance = (GameObject) GameObject.Instantiate(prefab, hat.transform);
        // Debug.Log("instance: " + instance);
    }

}
