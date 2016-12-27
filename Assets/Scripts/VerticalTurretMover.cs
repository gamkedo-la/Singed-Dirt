using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalTurretMover : MonoBehaviour {

	public float rotationSpeed = 5.0f;
	public float aimVertical = 45.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		aimVertical += Input.GetAxis ("Vertical") * Time.deltaTime * rotationSpeed;

	}
}
