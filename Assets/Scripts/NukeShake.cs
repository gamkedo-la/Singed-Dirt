using UnityEngine;

public class NukeShake : MonoBehaviour {

    public bool shakeIt;
    public float maxShakeX = 1f,
        maxShakeY = 1f,
        maxShakeZ = 3f,
        scale = 0.05f;

    private NukeScript control;
    private Quaternion origin;

    // Use this for initialization
    void Start() {
        origin = new Quaternion(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w);
    }

    // Update is called once per frame
    void Update() {
        if (shakeIt) {
            TheShake();
        }
    }

    private void TheShake() {
        float shakeX = scale * Random.Range(-maxShakeX, maxShakeX),
            shakeY = scale * Random.Range(-maxShakeY, maxShakeY),
            shakeZ = scale * Random.Range(-maxShakeZ, maxShakeZ);
        transform.localRotation = Quaternion.Euler(shakeX, shakeY, shakeZ);
    }

    public void StopShake() {
        shakeIt = false;
        transform.localRotation = origin;
    }
}
