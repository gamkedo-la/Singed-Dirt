using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour {

    public const int maxHealth = 100;

    [SyncVar(hook = "OnChangeHealth")]
    public int currentHealth = maxHealth;

    public RectTransform healthBar;

    public void TakeDamage(int amount)
    {
        if (!isServer)
            return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Debug.Log("death");

            //currentHealth = maxHealth;
            // called on the Server, but invoked on the Clients
            //RpcRespawn();
        }
    }

    void OnChangeHealth (int currentHealth )
    {
        healthBar.sizeDelta = new Vector2(currentHealth , healthBar.sizeDelta.y);
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
