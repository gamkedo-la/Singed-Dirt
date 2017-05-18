using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnInfoController: UxListElement {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text playerNameText;
    public RectTransform healthBar;

    int maxHealth;

    void Start() {
    }

    public void AssignPlayer(TankController player) {
        playerNameText.text = player.playerName;
        maxHealth = Health.maxHealth;
        var health = player.GetComponent<Health>();
        if (health != null) {
            health.onValueChangeEvent.AddListener(OnHealthChange);
        }
    }

    public void OnHealthChange(int newHealth) {
        if (healthBar != null) {
            var healthScale = (float) newHealth/(float)maxHealth;
            healthBar.localScale = new Vector3(healthScale,1f,1f);
        }
    }

    public void SetPlayerIsActive(bool isActive) {
        if (isActive) {
            transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            GetComponent<Image>().color = ParseHex.ToColor("AF4202FF");
        } else {
            transform.localScale = new Vector3(1f, 1f, 1f);
            GetComponent<Image>().color = ParseHex.ToColor("6B4E29FF");
        }
    }

}
