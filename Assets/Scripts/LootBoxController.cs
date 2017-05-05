﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LootBoxController : NetworkBehaviour {

	public float rotationSpeed = 20f;

    // Use this for initialization
    void Start () {
		// link to health
		var health = GetComponent<Health>();
		if (health != null) {
			health.onDeathEvent.AddListener(OnDeath);
		}

		// rotation setup,
		// apply random initial rotation
		// randomize rotation direction
		transform.Rotate(0, UnityEngine.Random.Range(0, 90), 0);
		if (UnityEngine.Random.Range(0,2) > 0) {
			rotationSpeed *= -1f;
		}

	}

	void Update() {
		// slowly rotate box
		transform.Rotate(0, rotationSpeed*Time.deltaTime, 0);
	}

	void OnDeath() {
		Debug.Log("loot destroyed");
		Destroy(gameObject);
	}
}