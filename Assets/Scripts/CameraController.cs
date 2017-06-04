using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class CameraController : MonoBehaviour {

	public enum CameraMode {
		overview,
		watchPlayer,
		watchLaunch,
		watchProjectile,
		watchExplosion,
		watchNuke
	}

	// Public
	public Transform playerLocation;
	// public float cameraPositionAbovePlayer = 1.5f;
	public float cameraPositionAboveExplosion = 1.5f;
	// public float explosionViewTime = 3.0f;
	public Transform overviewLocation;
	public Transform centerLocation;
    public Transform startFocus;

	// Private
	CameraMode cameraMode = CameraMode.overview;
	TankController player;
	// Vector3 explosionCamVector;
	// float timeInExplosionCam = 0.0f;
	// bool inProjectileMode = false;
	// Rigidbody projectileRB;
	Vector3 chaseCameraSpot;
	// Quaternion chaseCameraRot;
	float shakeAmount = 0f;
	// Vector3 originalPosition;

	private Camera gameCamera;
	private Camera nukeCamera;
	// private float zoomSpeed;                      // Reference speed for the smooth damping of the orthographic size.
	private float dampTime = 0.075f;                 // Approximate time for the camera to refocus.
	private float rotationDampTime = 0.075f;         // Approximate time for the camera to refocus.
	public Vector3 moveVelocity;

	// indicates the desired position and rotation of the camera
	Vector3 desiredPosition;
	Quaternion desiredRotation;

	// support camera zoom while aiming
	float zoomIncrement = 0.25f;
	float playerZoom = 2f;
	float maxPlayerZoom = 4.5f;
    private float minExplosionCamHeight = 5.0f;

	private bool isLaunchViewFalling = false;

    // Use this for initialization
    void Start () {
//		player = GameObject.Find("Player").GetComponent<TankController>();
		chaseCameraSpot = transform.position;
		// Debug.Log ("CameraController script starting in " + gameObject.name);
		transform.position = overviewLocation.transform.position;
		transform.LookAt(startFocus.position);
		desiredPosition = transform.position;

        gameCamera = GetComponentInChildren<Camera> ();
		var cameraGO = GameObject.FindWithTag("nukeCamera");
		if (cameraGO != null) {
			nukeCamera = cameraGO.GetComponent<Camera>();
		}
	}

	public void SetPlayerCameraFocus (TankController _player){
		player = _player;
		playerLocation = player.passiveCameraSource;
		SetPlayerCameraLookAt (player);
		// Debug.Log ("Setting Player Camera Focus");
	}

	public void SetPlayerCameraLookAt (TankController _player) {
		// Camera look at code

		// player.playerCameraSpot.position = player.transform.position - player.transform.forward * playerZoom + Vector3.up * cameraPositionAbovePlayer;
		// player.playerCameraSpot.LookAt (player.transform.position + player.transform.forward * 15.0f);
	}

	void Move() {
		if (shakeAmount > 0.0001f) {
			transform.position += Random.insideUnitSphere * shakeAmount;
		}

		if (Terrain.activeTerrain != null) {
	        float terrainY = Terrain.activeTerrain.SampleHeight(desiredPosition);
	        float minCamHeight = terrainY + 17.0f;
	        desiredPosition.y = Mathf.Max(desiredPosition.y, minCamHeight);
		}
        //Debug.Log("MinCamHeight " + minCamHeight + " desiredy " + desiredPosition.y);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref moveVelocity, dampTime);
		// transform.position = desiredPosition;
	}

	void Rotate() {
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationDampTime);
		// transform.rotation = desiredRotation;
	}

	// Update is called once per frame
	void LateUpdate () {
		if (cameraMode != CameraMode.watchNuke) {
			Move();
			Rotate();
		}
	} // end LateUpdate

	public void WatchOverview() {
		SwitchCamera(gameCamera);
		cameraMode = CameraMode.overview;
		StartCoroutine(WatchOverviewLoop());
	}
	IEnumerator WatchOverviewLoop() {
		desiredPosition = overviewLocation.transform.position;
		while (cameraMode == CameraMode.overview) {
			Vector3 relativePosition = desiredPosition - transform.position;
			desiredRotation = Quaternion.LookRotation(relativePosition);
			yield return null;
		}
	}

	// FIXME: ensure state loops can't overlap on each other (e.g.: two calls to WatchPlayer on top of each other)
	public void WatchPlayer(TankController tank) {
		SwitchCamera(gameCamera);
		// Debug.Log("WatchPlayer: " + tank.name);
		cameraMode = CameraMode.watchPlayer;
		StartCoroutine(WatchPlayerLoop(tank));
	}

	IEnumerator WatchPlayerLoop(TankController tank) {
		while (cameraMode == CameraMode.watchPlayer && tank != null) {
			var inputExclusive = EventSystem.current.currentSelectedGameObject != null &&
			    EventSystem.current.currentSelectedGameObject.tag == "inputexclusive";
			// allow camera zoom while in player mode;
			if (!inputExclusive &&
				((Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.Z)) ||
				 (Input.GetKey(KeyCode.RightShift) && Input.GetKey (KeyCode.Z)))) {
				playerZoom -= zoomIncrement;
				if (playerZoom < 0) playerZoom = 0;

			} else if (!inputExclusive && Input.GetKey (KeyCode.Z)) {
				playerZoom += zoomIncrement;
				if (playerZoom > maxPlayerZoom) playerZoom = maxPlayerZoom;
			}
			if (tank.hasControl && tank.isLocalPlayer) {
				desiredPosition = tank.chaseCameraSource.position;
				desiredRotation = tank.chaseCameraSource.rotation;
				/*
				desiredPosition = tank.model.transform.position - tank.model.transform.forward * playerZoom + Vector3.up * cameraPositionAbovePlayer;
				Vector3 relativePosition = tank.model.transform.position + tank.model.transform.forward * 15.0f - transform.position;
				desiredRotation = Quaternion.LookRotation(relativePosition);
				*/
			} else {
				desiredPosition = tank.passiveCameraSource.position;
				desiredRotation = tank.passiveCameraSource.rotation;
			}
			yield return null;
		}
	}

	// #TODO need to make the shot watch smoother
	public void WatchLaunch(GameObject projectileGO, GameObject playerGO){
		SwitchCamera(gameCamera);
		cameraMode = CameraMode.watchLaunch;
		// Debug.Log("Starting watch launch");
		isLaunchViewFalling = false;
		// Debug.Log("DEBUG playerGO " + playerGO.name);
		StartCoroutine(WatchLaunchLoop(projectileGO, playerGO.transform.Find("TankModel(Clone)").gameObject));
	}

	IEnumerator WatchLaunchLoop(GameObject projectileGO, GameObject playerGO){
		while(cameraMode == CameraMode.watchLaunch && projectileGO != null){
			Rigidbody projectileRB = projectileGO.GetComponent<Rigidbody>();
			if (projectileRB.velocity.y > 0.0f && isLaunchViewFalling == false) {
				// Debug.Log("PlayerGO inside of WatchLaunchLoop is " + playerGO.name);
				desiredPosition = projectileGO.transform.position - playerGO.transform.right * 15.0f - playerGO.transform.forward * 8.0f +
				Vector3.up * 10.0f;
			} else if (projectileRB.velocity.y < -0.5f){
				// Debug.Log("Shot began fall");
				isLaunchViewFalling = true;
			}

			Vector3 relativePosition = projectileGO.transform.position - desiredPosition;
			desiredRotation = Quaternion.LookRotation(relativePosition);
			yield return null;
		}
	}

	public void WatchProjectile(GameObject projectileGO) {
		SwitchCamera(gameCamera);
		// Debug.Log("WatchProjectile: " + projectileGO);
		cameraMode = CameraMode.watchProjectile;
		// Debug.Log("watch projectile coroutine looper counter");
		StartCoroutine(WatchProjectileLoop(projectileGO));
	}

	IEnumerator WatchProjectileLoop(GameObject projectileGO) {
		while (cameraMode == CameraMode.watchProjectile && projectileGO != null) {
			var projectileRB = projectileGO.GetComponent<Rigidbody>();
			if (projectileRB.velocity.magnitude > 3.0f) {
				desiredPosition = projectileGO.transform.position - projectileRB.velocity.normalized * 7.0f + Vector3.up * 3.0f;
				Vector3 relativePosition = projectileGO.transform.position + projectileRB.velocity.normalized * 7.0f - transform.position;
				desiredRotation = Quaternion.LookRotation(relativePosition);
			}
			yield return null;
		}
	}

	public void WatchExplosion(GameObject explosionGO) {
		SwitchCamera(gameCamera);
		Debug.Log("WatchExplosion: " + explosionGO);
		cameraMode = CameraMode.watchExplosion;
		StartCoroutine(WatchExplosionLoop(explosionGO));
	}

	IEnumerator WatchExplosionLoop(GameObject explosionGO) {
		float blastStartTime = Time.time;
		Quaternion startRotation = transform.rotation;
		Vector3 rollbackVector = -transform.forward;
		while (cameraMode == CameraMode.watchExplosion && explosionGO != null) {
			Vector3 planarExplosionForward = explosionGO.transform.forward;
			planarExplosionForward.y = 0.0f;
			planarExplosionForward.Normalize();
			float timeSinceStartedShowing = Time.time - blastStartTime;

			desiredPosition = explosionGO.transform.position - planarExplosionForward + Vector3.up * cameraPositionAboveExplosion + Vector3.right * 0.5f;
			float groundHeightAtDesiredPosition = Terrain.activeTerrain.SampleHeight(desiredPosition);
			desiredPosition.y = Mathf.Max(desiredPosition.y, groundHeightAtDesiredPosition + minExplosionCamHeight);
			desiredPosition += rollbackVector * timeSinceStartedShowing * 2.0f;
			desiredRotation = startRotation * Quaternion.AngleAxis(timeSinceStartedShowing * 5.0f, Vector3.right);
			// Vector3 relativePosition = desiredPosition - transform.position;
			// desiredRotation = Quaternion.LookRotation(relativePosition);

			yield return null;
		}
	}

	void SwitchCamera(Camera camera) {
		if (gameCamera != null) {
			gameCamera.enabled = (camera == gameCamera);
		}
		if (nukeCamera != null) {
			nukeCamera.enabled = (camera == nukeCamera);
		}
	}

	public void WatchNuke() {
		Debug.Log("WatchNuke");
		cameraMode = CameraMode.watchNuke;
		SwitchCamera(nukeCamera);
		//StartCoroutine(WatchExplosionLoop(explosionGO));
	}

	public void ShakeCamera(float amount, float dfactor){
		StartCoroutine(ShakeCameraLoop(amount, dfactor));
	}

	IEnumerator ShakeCameraLoop(float amount, float decreaseFactor) {
		// Debug.Log("start shaking camera for amount: " + amount);
		shakeAmount = amount;
		while (shakeAmount > 0.0001f) {
			shakeAmount *= decreaseFactor;
			// continue on next frame
			yield return null;
		}
		shakeAmount = 0f;
		// Debug.Log("done shaking camera");
	}

} // end Class
