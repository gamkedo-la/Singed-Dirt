using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnInfoController: UxListElement {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text playerNameText;
    public RectTransform healthBar;
    public Image turnIndicatorIcon;

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
        turnIndicatorIcon.enabled = isActive;
    }

}
