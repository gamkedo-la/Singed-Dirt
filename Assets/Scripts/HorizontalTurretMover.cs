using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalTurretMover : MonoBehaviour {

	public float rotationSpeed = 5.0f;

	public float aimHorizontal = 45.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		aimHorizontal += Input.GetAxis ("Horizontal") * Time.deltaTime * rotationSpeed;

	}
}
