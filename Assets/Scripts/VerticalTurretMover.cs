using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VerticalTurretMover : MonoBehaviour {

	public float rotationSpeed = 5.0f;
	public float aimVertical = 45.0f;

	
	// Update is called once per frame
	void Update () {
//		if (isLocalPlayer == false) {
//			return;
//		}
		if (TurnManager.instance.GetGameOverState () == false) {
			aimVertical += Input.GetAxis ("Vertical") * Time.deltaTime * rotationSpeed;
		}
	}
}
