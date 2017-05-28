using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class NukeScript : MonoBehaviour {

    public Transform nukeCam;
    public Bloom bloom;
    public GameObject dustStorm,
        colliseum,
        explosion;
    public ParticleSystem dustCloud,
        core,
        baseRing,
        secondaryRing,
        frontTopRing,
        backTopRing,
        stem,
        cap;

    public float timer;

    private bool bloomIn = false,
        bloomOut = false;

    private void Awake() {
        timer = 0;
    }

    private void Start() {
        StartCoroutine(NukeSequence());
    }

    private void Update() {
        timer += Time.deltaTime;
        if (bloomIn) IncreaseBloom();
        else if (bloomOut) DecreaseBloom();
    }


    private void IncreaseBloom() {
        float bloomVal;
        if (timer < 2.5) {
            bloomVal = Mathf.Min(2f, bloom.bloomIntensity + (float)(2f * Time.deltaTime / 2.5f));
            bloom.bloomIntensity = bloomVal;
        }
        else if (timer < 2.75) {
            bloomVal = Mathf.Min(4f, bloom.bloomIntensity + (float)(1f * Time.deltaTime / 0.25f));
            bloom.bloomIntensity = bloomVal;
        }
        else {
            bloomIn = false;
        }
    }

    private void DecreaseBloom() {
        float bloomVal,
            bloomVal2;

        if (timer >= 11.75) {
            bloomOut = false;
        }
        else if (timer >= 3.75) {
            bloomVal = Mathf.Max(0.01f, bloom.bloomIntensity - (float)(4.99f * Time.deltaTime / 8f));
            bloomVal2 = Mathf.Min(3f, bloom.bloomThreshold + (float)(5.05f * Time.deltaTime / 8f));
            bloom.bloomIntensity = bloomVal;
            bloom.bloomThreshold = bloomVal2;
        }
    }

    private IEnumerator NukeSequence() {
        yield return new WaitForSeconds(0.15f);
        bloom.bloomIntensity = 0.1f;

        yield return new WaitForSeconds(1.25f);
        Destroy(dustStorm);
        bloomIn = true;

        yield return new WaitForSeconds(1.75f);
        Destroy(explosion);
        Destroy(colliseum);
        core.Play();
        baseRing.Play();
        secondaryRing.Play();
        frontTopRing.Play();
        backTopRing.Play();
        stem.Play();
        dustCloud.Play();

        yield return new WaitForSeconds(0.5f);
        bloom.bloomIntensity = 3f;
        bloomOut = true;

        yield return new WaitForSeconds(6f);
        cap.Play();
    }

}
