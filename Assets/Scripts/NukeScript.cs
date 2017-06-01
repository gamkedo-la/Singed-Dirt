﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class NukeScript : MonoBehaviour {

    public UnityEvent onNukeFinished;
    public Terrain groundZero;
    public Bloom bloom;
    public NukeShake camShake;
    public GameObject colliseum,
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

    private bool sequenceStarted = false,
        bloomIn = false,
        bloomOut = false;

    private void Awake() {
        timer = 0;
    }

    private void Start() {
        // StartNukeSequence();
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
        groundZero.enabled = true;
        camShake.transform.GetComponent<Camera>().enabled = true;
        StartCoroutine(NukeSequence());
    }

    private IEnumerator NukeSequence() {
        sequenceStarted = true;
        explosion.Play();
        camShake.shakeIt = true;

        yield return new WaitForSeconds(0.15f);
        bloom.bloomIntensity = 0.1f;

        yield return new WaitForSeconds(1f);
        camShake.scale = 0.1f;

        yield return new WaitForSeconds(0.75f);
        bloomIn = true;
        camShake.scale = 3f;
        initialDebris.Play();

        yield return new WaitForSeconds(0.5f);
        camShake.scale = 0.1f;

        yield return new WaitForSeconds(1.25f);
        Destroy(explosion.transform.parent.gameObject);
        Destroy(colliseum);
        Destroy(initialDebris.transform.parent.gameObject);
        dustStorm.Stop();
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
        dustCloud.Play();

        yield return new WaitForSeconds(0.5f);
        bloom.bloomIntensity = 3f;
        bloomOut = true;

        yield return new WaitForSeconds(3f);
        camShake.scale = 0.2f;

        yield return new WaitForSeconds(3f);
        heavyDust.Play();
        cap.Play();
        camShake.scale = 0.35f;

        yield return new WaitForSeconds(0.5f);
        camShake.scale = 0.75f;

        yield return new WaitForSeconds(2.25f);
        Destroy(frontBlastRing.gameObject);

        yield return new WaitForSeconds(1f);
        Destroy(secondaryRing.transform.parent.gameObject);
        Destroy(victim);
        dustStorm.Play();

        yield return new WaitForSeconds(2f);
        camShake.scale = 0.5f;

        yield return new WaitForSeconds(1.5f);
        Destroy(heavyDust.gameObject);
        Destroy(frontDebrisRing.gameObject);
        camShake.scale = 0.3f;

        yield return new WaitForSeconds(0.25f);
        Destroy(dustCloud.transform.parent.gameObject);
        camShake.scale = 0.2f;

        yield return new WaitForSeconds(1f);
        camShake.scale = 0.1f;

        yield return new WaitForSeconds(1f);
        camShake.scale = 0.05f;

        yield return new WaitForSeconds(1f);
        camShake.shakeIt = false;

        yield return new WaitForSeconds(1f);
        onNukeFinished.Invoke();
    }

}
