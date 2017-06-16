using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class NukeScript : MonoBehaviour {

    public UnityEvent onNukeFinished;
    public Terrain groundZero,
        patch;
    public Bloom bloom;
    public CameraController camShake;
    public GameObject smallDustStorm,
        colliseum,
        audienceUpper,
        audienceLower,
        victim;
    public ParticleSystem explosion,
        dustStorm,
        dustCloud,
        heavyDust,
        core,
        initialDebris,
        baseRing,
        secondaryRing,
        frontTopRing,
        backTopRing,
        stem,
        cap,
        frontBlastRing,
        backBlastRing,
        frontDebrisRing;
    public float timer;
    public bool blastKill = false;
    public TankController theOwner;

    private bool sequenceStarted = false,
        bloomIn = false,
        bloomOut = false;

    private void Awake() {
        timer = 0;
    }

    private void Start() {
        onNukeFinished = new UnityEvent();
    }

    private void Update() {
        if (sequenceStarted) {
            timer += Time.deltaTime;
            if (bloomIn) IncreaseBloom();
            else if (bloomOut) DecreaseBloom();
        }
    }

    private void IncreaseBloom() {
        float bloomVal;
        if (timer < 3) {
            bloomVal = Mathf.Min(2f, bloom.bloomIntensity + (float)(2f * Time.deltaTime / 2.5f));
            bloom.bloomIntensity = bloomVal;
        }
        else if (timer < 3.25) {
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

        if (timer >= 12.5) {
            bloomOut = false;
        }
        else if (timer >= 4.25) {
            bloomVal = Mathf.Max(0.01f, bloom.bloomIntensity - (float)(4.99f * Time.deltaTime / 8f));
            bloomVal2 = Mathf.Min(3f, bloom.bloomThreshold + (float)(5.05f * Time.deltaTime / 8f));
            bloom.bloomIntensity = bloomVal;
            bloom.bloomThreshold = bloomVal2;
        }
    }

    public void StartNukeSequence() {
        // FIXME: start the sequence watching the mushMine detonate
        dustStorm.Play();
        MeshRenderer[] victimMeshes = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < victimMeshes.Length; i++) {
            victimMeshes[i].enabled = true;
        }
        Destroy(smallDustStorm);
        StartCoroutine(NukeSequence());
    }

    private IEnumerator NukeSequence() {
        yield return new WaitForSeconds(4f);
        sequenceStarted = true;
        explosion.Play();

        yield return new WaitForSeconds(2f);
        camShake.ChangeShakeAmount(0.75f);
        bloom.bloomIntensity = 0.1f;

        yield return new WaitForSeconds(0.5f);
        bloomIn = true;
        dustCloud.Emit(500);
        camShake.ChangeShakeAmount(7f);
        initialDebris.Play();

        yield return new WaitForSeconds(1f);
        camShake.ChangeShakeAmount(0.75f);

        yield return new WaitForSeconds(0.75f);
        blastKill = true;
        Destroy(explosion.transform.parent.gameObject);
        Destroy(colliseum);
        Destroy(audienceLower);
        Destroy(audienceUpper);
        Destroy(initialDebris.transform.parent.gameObject);

        dustStorm.Stop();
        dustCloud.Play();
        core.Play();
        baseRing.Play();
        secondaryRing.Play();
        frontTopRing.Play();
        backTopRing.Play();
        stem.Play();
        frontBlastRing.Play();
        backBlastRing.Play();
        heavyDust.Play();
        frontDebrisRing.Play();

        yield return new WaitForSeconds(0.5f);
        bloom.bloomIntensity = 3f;
        bloomOut = true;
        camShake.ChangeShakeAmount(1.25f);

        yield return new WaitForSeconds(3f);
        camShake.ChangeShakeAmount(2.25f);
        dustStorm.Emit(500);

        yield return new WaitForSeconds(3f);
        heavyDust.Play();
        cap.Play();
        camShake.ChangeShakeAmount(3f);

        yield return new WaitForSeconds(0.5f);
        camShake.ChangeShakeAmount(4f);
        Destroy(patch.gameObject);

        yield return new WaitForSeconds(2.25f);
        Destroy(frontBlastRing.gameObject);
        camShake.ChangeShakeAmount(3f);

        yield return new WaitForSeconds(1f);
        Destroy(secondaryRing.transform.parent.gameObject);
        dustStorm.Play();

        yield return new WaitForSeconds(2f);
        Destroy(victim);
        camShake.ChangeShakeAmount(2.25f);

        yield return new WaitForSeconds(0.75f);
        camShake.ChangeShakeAmount(1.5f);

        yield return new WaitForSeconds(0.75f);
        Destroy(heavyDust.gameObject);
        Destroy(frontDebrisRing.gameObject);
        camShake.ChangeShakeAmount(0.75f);

        yield return new WaitForSeconds(0.25f);
        Destroy(dustCloud.transform.parent.gameObject);

        yield return new WaitForSeconds(0.5f);
        camShake.ShakeCamera(0.5f, 0.95f);

        yield return new WaitForSeconds(3.5f);
        onNukeFinished.Invoke();
        StopCoroutine(NukeSequence());
    }

}