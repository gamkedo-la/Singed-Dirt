using System;
using UnityEngine;
using UnityEngine.UI;

public class AmmoInfoController: UxListElement {
    // ------------------------------------------------------
    // UI REFERENCE VARIABLES
    [Header("UI Reference")]
    public Text ammoNameText;
    public Text ammoCountText;
    public GameObject model;

    // highlight A54202FF
    // normal 6B4E29FF

    public void AssignAmmo(ProjectileKind projectileKind, int count) {
        ammoNameText.text = NameMapping.ForProjectile(projectileKind);
        if (count > 99) {
            ammoCountText.text = "99+";
        } else {
            ammoCountText.text = count.ToString();
        }
    }

    public void SetIsActive(bool isActive) {
        if (isActive) {
            transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            GetComponent<Image>().color = ParseHex.ToColor("AF4202FF");
        } else {
            transform.localScale = new Vector3(1f, 1f, 1f);
            GetComponent<Image>().color = ParseHex.ToColor("6B4E29FF");
        }
    }

}
