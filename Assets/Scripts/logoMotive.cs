using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class logoMotive : MonoBehaviour {

    private Quaternion startRotation;
    public float upSpeed = 1.1f;
    public float rightSpeed = 0.3f;
    public float upAngle = 7.0f;
    public float rightAngle = 4.0f;
    // Use this for initialization
    void Start () {
        startRotation = transform.rotation;
        if (Random.Range(1.0f, 1000000.0f) == 1.0f) {
            weeeee();
        }
    }
    void weeeee() {
        rightSpeed = 6.3f;
        rightAngle = 9.0f;
        upSpeed = 15.1f;
        upAngle = 14.0f;
    }
    
    // Update is called once per frame
    void Update () {
        transform.rotation = startRotation * Quaternion.AngleAxis(Mathf.Cos(Time.time * upSpeed) * upAngle, Vector3.up) *
        Quaternion.AngleAxis(Mathf.Cos(Time.time * rightSpeed) * rightAngle, Vector3.right);
    }
}
