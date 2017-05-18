using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoListController: UxListController {
    public GameObject ammoInfoPrefab;
    Dictionary<ProjectileKind, AmmoInfoController> ammoRegistry;
    ProjectileKind lastSelectedShot;

    void Awake() {
        ammoRegistry = new Dictionary<ProjectileKind, AmmoInfoController>();
        lastSelectedShot = ProjectileKind.cannonBall;
    }

    public void AssignInventory(ProjectileInventory shotInventory) {
        // rebuild inventory list each time
        foreach(ProjectileKind projectileKind in Enum.GetValues(typeof(ProjectileKind))) {
            if (ammoRegistry.ContainsKey(projectileKind)) {
                Destroy(ammoRegistry[projectileKind].gameObject);
                Remove(ammoRegistry[projectileKind]);
                ammoRegistry.Remove(projectileKind);
            }
        }

        // loop through all shot types
        foreach(ProjectileKind projectileKind in Enum.GetValues(typeof(ProjectileKind))) {
            // if we have available shots...
            var available = shotInventory.GetAvailable(projectileKind);
            if (available > 0) {
                AmmoInfoController ammoInfo;
                // create new ammo info slot if required
                if (!ammoRegistry.TryGetValue(projectileKind, out ammoInfo)) {
                    var ammoInfoGo = (GameObject) Instantiate(ammoInfoPrefab);
                    ammoInfo = ammoInfoGo.GetComponent<AmmoInfoController>();
                    ammoRegistry[projectileKind] = ammoInfo;
                    Add(ammoInfo);
                }
                // update ammo count
                ammoInfo.AssignAmmo(projectileKind, available);
            }
        }
        SetSelected(lastSelectedShot);
    }

    public void SetSelected(ProjectileKind selectedShot) {
        lastSelectedShot = selectedShot;
        foreach (var projectileKind in ammoRegistry.Keys) {
            ammoRegistry[projectileKind].SetIsActive(selectedShot == projectileKind);
        }
    }

}
