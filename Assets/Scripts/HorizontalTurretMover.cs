using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HorizontalTurretMover : MonoBehaviour {

	public float rotationSpeed = 5.0f;
	public float aimHorizontal = 45.0f;
	// Use this for initialization
	void Start () {
		GameObject centerPoint = GameObject.Find ("MapCenterLookAt");
		aimHorizontal = Mathf.Atan2 (centerPoint.transform.position.z - transform.position.z,
			centerPoint.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
	}

	// Update is called once per frame
	void Update () {
//		if (isLocalPlayer == false) {
//			return;
//		}
		if (TurnManager.singleton.GetGameOverState () == false) {
			aimHorizontal += Input.GetAxis ("Horizontal") * Time.deltaTime * rotationSpeed;
		}
	}
}
