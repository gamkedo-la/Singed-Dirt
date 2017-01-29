using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSelectSceneManager : MonoBehaviour {

	public static PlayerSelectSceneManager instance;
	List<GameObject> activePlayers;
	bool startCheck = false;

	// Use this for initialization
	void Awake () {
		instance = this;
		activePlayers = new List<GameObject>();
	}
	
	public void PlayerReportingReady(GameObject player){
		activePlayers.Add (player);
		if (startCheck == false) {
			startCheck = true;
		}
	}

	public void PlayerReportingDone(GameObject player){
		activePlayers.Remove (player);
	}

	void Update(){
		if (startCheck) {
			if (activePlayers.Count == 0) {
				SceneManager.LoadScene ("Game");
			}
		}
	}
}
