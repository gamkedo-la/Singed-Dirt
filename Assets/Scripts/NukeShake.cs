using UnityEngine;

public class NukeShake : MonoBehaviour {

    public bool shakeIt;
    public float maxShakeX = 1f,
        maxShakeY = 1f,
        maxShakeZ = 10f,
        scale = 0.05f;

    private NukeScript control;
    private Quaternion origin;
    private int flipState = 1;

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
        flipState *= -1;

        float shakeX = origin.x + flipState * scale * (Mathf.PerlinNoise(origin.x, maxShakeX)),
        shakeY = origin.y + flipState * scale * (Mathf.PerlinNoise(origin.y, maxShakeY)),
        shakeZ = origin.z + flipState * scale * (Mathf.PerlinNoise(origin.z, maxShakeZ));

        transform.localRotation = Quaternion.Euler(shakeX, shakeY, shakeZ);
    }

    public void StopShake() {
        shakeIt = false;
        transform.localRotation = origin;
    }
}
