using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LootBoxController : NetworkBehaviour {

    // Use this for initialization
    void Start () {
		// link to health
		var health = GetComponent<Health>();
		if (health != null) {
			health.onDeathEvent.AddListener(OnDeath);
		}
	}

	void OnDeath() {
		Debug.Log("loot destroyed");
		Destroy(gameObject);
	}
}
