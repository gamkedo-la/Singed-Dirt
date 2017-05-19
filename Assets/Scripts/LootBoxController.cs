using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LootBoxController : NetworkBehaviour {

    public float rotationSpeed = 20f;

    ProjectileKind lootKind;
    int lootCount;

    // Use this for initialization
    void Start() {
        // link to health
        var health = GetComponent<Health>();
        if (health != null) {
            health.onDeathEvent.AddListener(OnDeath);
        }

        // rotation setup,
        // apply random initial rotation
        // randomize rotation direction
        transform.Rotate(0, UnityEngine.Random.Range(0, 90), 0);
        if (UnityEngine.Random.Range(0, 2) > 0) {
            rotationSpeed *= -1f;
        }
    }

    public void AssignLoot(
        ProjectileKind kind,
        int count
    ) {
        this.lootKind = kind;
        this.lootCount = count;
    }

    void Update() {
        // slowly rotate box
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    void OnDeath(GameObject from) {
        if (from.GetComponent<TankController>() != null) {
            UxChatController.SendToConsole(
                String.Format("{0} acquired {1} {2}",
                    from.GetComponent<TankController>().playerName,
                    lootCount,
                    NameMapping.ForProjectile(lootKind)));
        }
        var inventory = from.GetComponent<ProjectileInventory>();
        if (inventory != null) {
            inventory.ServerModify(lootKind, lootCount);
        }
        if (lootKind == (ProjectileKind)7) LootSpawnController.singleton.mushboomCount--;
        Destroy(gameObject);
    }
}
