using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookWhereYoureGoing : MonoBehaviour {
    
    public GameObject rotateThis;
    public Rigidbody myRigidBody;
    public float lerpSpeed = 1.0f;
	
	// Update is called once per frame
	void Update () {
        if ((rotateThis == null) || (myRigidBody == null)) {
            return;
        }

        // lerp it so it is not wobbly
        if (myRigidBody.velocity != Vector3.zero) {
            rotateThis.transform.rotation = Quaternion.Slerp(
                rotateThis.transform.rotation,
                Quaternion.LookRotation(myRigidBody.velocity),
                Time.deltaTime * lerpSpeed
            );
        }
	}
}
