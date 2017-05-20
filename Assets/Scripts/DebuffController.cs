using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DebuffController : NetworkBehaviour {
    public string effect;
    
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void DebuffTarget(GameObject target) {
        switch (effect) {
            default:

                break;
        }
    }
}
