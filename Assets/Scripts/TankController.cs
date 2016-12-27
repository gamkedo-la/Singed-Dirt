using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankController : MonoBehaviour {

	// Public
	public GameObject projectilePrefab;
	public Transform shotSource;
	public float shotPower = 30.0f;
	public Text hud;
	public Transform turret;
	public Transform playerCameraSpot;

	// Private
	Rigidbody rb;
	HorizontalTurretMover horizontalTurret;
	VerticalTurretMover verticalTurret;

	// Hidden Public
	[HideInInspector]  // This makes the next variable following this to be public but not show up in the inspector.
	public GameObject liveProjectile;

	// Use this for initialization
	void Start () {
		horizontalTurret = GetComponentInChildren<HorizontalTurretMover> ();
		verticalTurret = GetComponentInChildren<VerticalTurretMover> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			liveProjectile = (GameObject)GameObject.Instantiate (projectilePrefab);
			liveProjectile.transform.position = shotSource.position;
			rb = liveProjectile.GetComponent<Rigidbody> ();
			rb.AddForce (shotSource.forward * shotPower);
		}

		transform.rotation = Quaternion.AngleAxis (horizontalTurret.aimHorizontal, Vector3.up);

		// These two lines stay together
		turret.rotation = Quaternion.AngleAxis (horizontalTurret.aimHorizontal, Vector3.up) *
		Quaternion.AngleAxis (verticalTurret.aimVertical, Vector3.right);

		hud.text = 
		"Heading: " + horizontalTurret.aimHorizontal + "degrees\n" +
		"Elevation: " + verticalTurret.aimVertical + " degrees\n" +
		"Muzzle Velocity: " + shotPower + "m/s";

		// Camera look at code
		playerCameraSpot.position = transform.position - transform.forward * 2.0f + Vector3.up * 1.5f;
		playerCameraSpot.LookAt (transform.position + transform.forward * 15.0f);
	}
}
