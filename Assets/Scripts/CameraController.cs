using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour {

	public enum CameraMode {
		overview,
		watchPlayer,
		watchProjectile,
		watchExplosion
	}

	// Public
	public Transform playerLocation;
	public float cameraPositionAbovePlayer = 1.5f;
	public float cameraPositionAboveExplosion = 1.5f;
	public float explosionViewTime = 3.0f;
	public Transform overviewLocation;
	public Transform centerLocation;

	// Private
	CameraMode cameraMode = CameraMode.overview;
	TankController player;
	Vector3 explosionCamVector;
	float timeInExplosionCam = 0.0f;
	bool inProjectileMode = false;
	Rigidbody projectileRB;
	Vector3 chaseCameraSpot;
	Quaternion chaseCameraRot;
	float shakeAmount = 0f;
	Vector3 originalPosition;

	private Camera camera;
	private float zoomSpeed;                      // Reference speed for the smooth damping of the orthographic size.
	public float dampTime = 0.2f;                 // Approximate time for the camera to refocus.
	public float rotationDampTime = 0.2f;         // Approximate time for the camera to refocus.
	public Vector3 moveVelocity;

	// indicates the desired position and rotation of the camera
	Vector3 desiredPosition;
	Quaternion desiredRotation;

	// support camera zoom while aiming
	float zoomIncrement = 0.25f;
	float playerZoom = 2f;
	float maxPlayerZoom = 4.5f;

	// Use this for initialization
	void Start () {
//		player = GameObject.Find("Player").GetComponent<TankController>();
		chaseCameraSpot = transform.position;
		Debug.Log ("CameraController script starting in " + gameObject.name);
		transform.position = overviewLocation.transform.position;
		transform.LookAt(centerLocation.position);
		desiredPosition = transform.position;

        camera = GetComponentInChildren<Camera> ();
	}

	public void SetPlayerCameraFocus (TankController _player){
		player = _player;
		playerLocation = player.passiveCameraSource;
		SetPlayerCameraLookAt (player);
		Debug.Log ("Setting Player Camera Focus");
	}

	public void SetPlayerCameraLookAt (TankController _player) {
		// Camera look at code
		/*
		player.playerCameraSpot.position = player.transform.position - player.transform.forward * playerZoom + Vector3.up * cameraPositionAbovePlayer;
		player.playerCameraSpot.LookAt (player.transform.position + player.transform.forward * 15.0f);
		*/

	}

	void Move() {
		if (shakeAmount > 0.0001f) {
			transform.position += Random.insideUnitSphere * shakeAmount;
		}
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref moveVelocity, dampTime);
		//transform.position = chaseCameraSpot; // this is the interrupt the smoothing while aiming
		//transform.rotation = playerLocation.rotation;
	}

	void Rotate() {
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationDampTime);
	}


	// Update is called once per frame
	void LateUpdate () {
		Move();
		Rotate();
		/*
		if (player == null || playerLocation == null) {
			return;
		}
		if (player.liveProjectile != null) {
			// Camera for following projectile
			if (projectileRB == null){
				projectileRB = player.liveProjectile.GetComponent<Rigidbody>();
			}
			if (projectileRB.velocity.magnitude > 3.0f) {
				chaseCameraSpot = player.liveProjectile.transform.position - projectileRB.velocity.normalized * 7.0f + Vector3.up * 3.0f;
				transform.LookAt(player.liveProjectile.transform.position + projectileRB.velocity.normalized * 7.0f);
			}

			inProjectileMode = true;
		} else {
			if (inProjectileMode) {
				timeInExplosionCam = explosionViewTime;
				explosionCamVector = transform.position;
				ShakeCamera (0.8f, 0.8f);
				inProjectileMode = false;
			} // end if in projectile mode
			if (timeInExplosionCam > 0.0f) {
				transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
				timeInExplosionCam -= Time.deltaTime;
				if (timeInExplosionCam <= 0.0f) {
					//TurnManager.instance.CycleActiveTank ();
				}
//				chaseCameraSpot = transform.position;
//				chaseCameraSpot = explosionCamVector + Vector3.up * 4.0f - player.transform.forward * 4.0f;
//				transform.LookAt (explosionCamVector);
//				chaseCameraRot = Quaternion.LookRotation(explosionCamVector - chaseCameraSpot);
			} else {  // This is standard aiming view
				chaseCameraSpot = playerLocation.position;
				transform.position = chaseCameraSpot; // this is the interrupt the smoothing while aiming
				transform.rotation = playerLocation.rotation;

			} // end if time in explosion cam greater than 0

		} // end if statement for if live projectile doesn't equal null
		*/
	} // end LateUpdate

	void FixedUpdate(){
		/*
		float springK = 0.94f;
		if (inProjectileMode) {
			transform.position = springK * transform.position + (1.0f - springK) * chaseCameraSpot;
		}
		*/
	} // FixedUpdate

	void Update() {
	}

	public void WatchOverview() {
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
		Debug.Log("WatchPlayer: " + tank.name);
		cameraMode = CameraMode.watchPlayer;
		StartCoroutine(WatchPlayerLoop(tank));
	}

	IEnumerator WatchPlayerLoop(TankController tank) {
		while (cameraMode == CameraMode.watchPlayer && tank != null) {
			// allow camera zoom while in player mode;
			if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.Z)) ||
				(Input.GetKey(KeyCode.RightShift) && Input.GetKey (KeyCode.Z))) {
				playerZoom -= zoomIncrement;
				if (playerZoom < 0) playerZoom = 0;

			} else if (Input.GetKey (KeyCode.Z)) {
				playerZoom += zoomIncrement;
				if (playerZoom > maxPlayerZoom) playerZoom = maxPlayerZoom;
			}
			if (tank.hasControl) {
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

	public void WatchProjectile(GameObject projectileGO) {
		Debug.Log("WatchProjectile: " + projectileGO);
		cameraMode = CameraMode.watchProjectile;
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
		Debug.Log("WatchExplosion: " + explosionGO);
		cameraMode = CameraMode.watchExplosion;
		StartCoroutine(WatchExplosionLoop(explosionGO));
	}

	IEnumerator WatchExplosionLoop(GameObject explosionGO) {
		while (cameraMode == CameraMode.watchProjectile && explosionGO != null) {
			var projectileRB = explosionGO.GetComponent<Rigidbody>();

			desiredPosition = explosionGO.transform.position - explosionGO.transform.forward + Vector3.up * cameraPositionAboveExplosion + Vector3.right * 0.5f;
			Vector3 relativePosition = desiredPosition - transform.position;
			desiredRotation = Quaternion.LookRotation(relativePosition);

			yield return null;
		}
	}

	public void ShakeCamera(float amount, float dfactor){
		StartCoroutine(ShakeCameraLoop(amount, dfactor));
	}

	IEnumerator ShakeCameraLoop(float amount, float decreaseFactor) {
		Debug.Log("start shaking camera for amount: " + amount);
		shakeAmount = amount;
		while (shakeAmount > 0.0001f) {
			shakeAmount *= decreaseFactor;
			// continue on next frame
			yield return null;
		}
		shakeAmount = 0f;
		Debug.Log("done shaking camera");
	}

} // end Class
