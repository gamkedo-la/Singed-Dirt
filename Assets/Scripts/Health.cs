using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections;

public class Health : NetworkBehaviour {

    [System.Serializable]
    public class OnValueChangeEvent : UnityEvent<int> { };

    [System.Serializable]
    public class OnDeathEvent: UnityEvent<GameObject> { };

    public OnValueChangeEvent onValueChangeEvent;
    public OnDeathEvent onDeathEvent;

    void Awake() {
        onValueChangeEvent = new OnValueChangeEvent();
        onDeathEvent = new OnDeathEvent();
    }

    public const int maxHealth = 100;

    [SyncVar(hook = "OnChangeHealth")]
    public int health = maxHealth;

    public RectTransform healthBar;

    public void TakeDamage(int amount, GameObject from)
    {
        if (!isServer) return;
        // Debug.Log("Health.TakeDamage for " + amount + " from : " + from);

        health -= amount;
        if (health <= 0) {
            health = 0;
            // Debug.Log("death");
            // invoke death
            onDeathEvent.Invoke(from);
        }
    }

    void OnChangeHealth (int newHealth) {
        if (healthBar != null) {
            var healthScale = (float) newHealth/(float)maxHealth;
            healthBar.localScale = new Vector3(healthScale,1f,1f);
        }
        health = newHealth;
        onValueChangeEvent.Invoke(newHealth);
    }

}
