using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushBehavior : MonoBehaviour {

    public Rigidbody mushRigidBody;
    public ParticleSystem nukeGreen,
        explosion;
    public ProjectileController control;
    public int lifeSpan = 5;
    public Vector3 growthSpurt;
    public Health health;

    private bool scaleIt = false,
        isDead = false;
    private Vector3 scaleOrigin,
        scaleTo;
    private int stage = 0,
        startingTurn,
        countdown;
    private float timeToScale = 0.5f,
        scaleTime = 0f;
    private bool doIt = true;

    private void Start() {
        health.onDeathEvent.AddListener(OnDeath);
        scaleTo = growthSpurt;
        StartCoroutine(GrowTheShroom(0.5f));
        startingTurn = TurnManager.singleton.numberOfTurns;
        countdown = lifeSpan;
    }

    // Update is called once per frame
    void Update() {
        if (scaleIt) {
            if (scaleTime <= 1) {
                scaleTime += Time.deltaTime / timeToScale;
                gameObject.transform.localScale = Vector3.Lerp(scaleOrigin, scaleTo, scaleTime);
            }
            else scaleIt = false;
        }
        if (doIt) {
            if (countdown < 1) {
                doIt = false;
                FinishThem();
            }
            else if (countdown > lifeSpan - (TurnManager.singleton.numberOfTurns - startingTurn)) {
                countdown--;
                stage++;
                AdvanceStage();
                // you'll want to add a delay for the finish them, if less than 1 is correct, but it iterates immediately

            }
        }
    }

    void OnDeath(GameObject from) {
        if (!isDead) {
            transform.FindChild("Model").gameObject.SetActive(false);
            UxChatController.SendToConsole("" + from.GetComponent<TankController>().playerName +
               " has destroyed a MushBoom!");
            isDead = true;
            Destroy(gameObject);
        }
    }

    private void AdvanceStage() {
        switch (stage) {
            case 1:
            case 2:
                control.effectRadius += 5;
                break;
            case 3:
            case 4:
                control.effectRadius += 10;
                break;
            case 5:
                control.effectRadius += 20;
                break;
        }

        scaleTo = transform.localScale + growthSpurt;
        int percentReady = stage * 19;
        if (percentReady < 90) UxChatController.SendToConsole("MushBoom Status: " + percentReady.ToString() + "% of critical mass!");
        else UxChatController.SendToConsole("MushBoom Status: Detonation Imminent!");

        StartCoroutine(GrowTheShroom(0.5f));
    }

    private void FinishThem() {
        control.DisableCollisions(5f);
        scaleTo *= 3f;
        UxChatController.SendToConsole("MushBoom Status: Detonating!");
        StartCoroutine(GrowTheShroom(0.5f));
    }

    private IEnumerator GrowTheShroom(float timeDelay) {
        explosion.Play();
        scaleTime = 0f;
        timeToScale = 0.5f;

        yield return new WaitForSeconds(timeDelay);
        scaleIt = true;
    }

}
