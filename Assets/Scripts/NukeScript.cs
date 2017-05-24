using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class NukeScript : MonoBehaviour {

    public float timer,
        startSequenceAt = 1;
    public Transform nukeCam;
    public Bloom bloom;

    private void Awake() {
        timer = 0;
    }

    private void Update() {
        timer += Time.deltaTime;
        Debug.Log(timer.ToString());
    }

}
