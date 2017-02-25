using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateModel : MonoBehaviour {

	public float rotateSpeed = 10.0f;
	Transform modelPosition;

	void Start(){
		modelPosition = GameObject.Find ("ModelPosition").transform;
	}
	
	public void RotateLeft(){
		modelPosition.Rotate (Vector3.up * Time.deltaTime * rotateSpeed);

	}

	public void RotateRight(){
		modelPosition.Rotate (Vector3.up * Time.deltaTime * rotateSpeed * -1);
	}
}
