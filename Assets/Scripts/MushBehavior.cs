using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushBehavior : MonoBehaviour {

    public Rigidbody mushRigidBody;
    public ParticleSystem nukeGreen;
    public TankController owner;

    private bool scaleIt = false;
    private Vector3 scaleOrigin;
    private int stage = 0;
    private float scaleBy,
        timeToScale = 0.5f,
        scaleTime = 0f;

    // Update is called once per frame
    void Update() {
        if (scaleIt) {
            if (scaleTime <= 1) {
                scaleTime += Time.deltaTime / timeToScale;
                transform.localScale = Vector3.Lerp(scaleOrigin, scaleOrigin * scaleBy, scaleTime);
            }
            else scaleIt = false;
        }
    }

    void OnDeath(GameObject from) {
        UxChatController.SendToConsole("" + from.GetComponent<TankController>().playerName +
            " has destroyed a MushBoom!");
    }

    public void PlantIt() {
        Destroy(transform.FindChild("molTrail").gameObject);
        StartCoroutine(StopRolling());
    }

    public void SetTheScale(float scaleMulti) {
        scaleTime = 0f;
        timeToScale = 0.5f;
        scaleOrigin = transform.localScale;
        scaleBy = scaleMulti;
        scaleIt = true;
    }

    private IEnumerator StopRolling() {
        nukeGreen.transform.localScale *= 1.25f;
        nukeGreen.Emit(150);
        mushRigidBody.isKinematic = true;

        yield return new WaitForSeconds(0.5f);
        SetTheScale(1.25f);
        UxChatController.SendToConsole("" + owner.playerName + " has planted a MushBoom!");
        StopCoroutine(StopRolling());
    }

    private void GrowTheShroom() {
        // particle effect

    }

}
