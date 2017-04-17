using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour {

    public const int maxHealth = 100;

    [SyncVar(hook = "OnChangeHealth")]
    public int health = maxHealth;

    public RectTransform healthBar;

    public void TakeDamage(int amount)
    {
        if (!isServer) return;
        // Debug.Log("TakeDamage for " + amount);

        health -= amount;
        if (health <= 0)
        {
            health = 0;
            // Debug.Log("death");

    		var manager = TurnManager.GetGameManager();
            manager.ServerHandleTankDeath(gameObject);
            //currentHealth = maxHealth;
            // called on the Server, but invoked on the Clients
            //RpcRespawn();
        }
    }

    void OnChangeHealth (int newHealth) {
        if (healthBar != null) {
            healthBar.sizeDelta = new Vector2(newHealth , healthBar.sizeDelta.y);
        }
        health = newHealth;
    }

    /*
    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            // move back to zero location
            transform.position = Vector3.zero;
        }
    }
    */
}
