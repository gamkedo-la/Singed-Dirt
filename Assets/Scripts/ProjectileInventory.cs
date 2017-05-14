using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileInventory : NetworkBehaviour {
    int[] ammoCounts;
    int maxShot;

    void Awake() {
        maxShot = System.Enum.GetValues(typeof(ProjectileKind)).Length;
        ammoCounts = new int[maxShot+1];

        // initialize inventory
        ammoCounts[(int) ProjectileKind.cannonBall] = int.MaxValue;
        // uncomment these to get unlimited ammo for the appropriate type for testing
        /*ammoCounts[(int) ProjectileKind.acorn] = int.MaxValue;
        ammoCounts[(int) ProjectileKind.artilleryShell] = int.MaxValue;
        ammoCounts[(int) ProjectileKind.beetMissile] = int.MaxValue;
        ammoCounts[(int) ProjectileKind.mushboom] = int.MaxValue;*/
        ammoCounts[(int) ProjectileKind.pillarShot] = int.MaxValue;
        //ammoCounts[(int) ProjectileKind.sharkToothCluster] = int.MaxValue;*/
    }

    public void Modify(ProjectileKind kind, int amount) {
        if (ammoCounts[(int) kind] != int.MaxValue) {
            if (amount == int.MaxValue) {
                ammoCounts[(int) kind] = amount;
            } else {
                ammoCounts[(int) kind] += amount;
            }
        }
    }

    ProjectileKind FindAvailableShot(ProjectileKind currentShot, int modifier) {
        var nextShot = (maxShot + (int) currentShot + modifier) % maxShot;
        while (nextShot != (int) currentShot) {
            if (ammoCounts[nextShot] > 0) {
                return (ProjectileKind) nextShot;
            }
            nextShot = (maxShot + nextShot + modifier) % maxShot;
        }
        return currentShot;
    }

    public ProjectileKind PrevAvailableShot(ProjectileKind currentShot) {
        return FindAvailableShot(currentShot, -1);
    }
    public ProjectileKind NextAvailableShot(ProjectileKind currentShot) {
        return FindAvailableShot(currentShot, 1);
    }

    public int GetAvailable(ProjectileKind shot) {
        return ammoCounts[(int) shot];
    }

    [ClientRpc]
    public void RpcModify(ProjectileKind kind, int amount) {
        Modify(kind, amount);
    }

    public void ServerModify(ProjectileKind kind, int amount) {
        if (isServer) {
            RpcModify(kind, amount);
        }
    }

}
