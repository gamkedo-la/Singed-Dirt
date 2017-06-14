using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : MonoBehaviour {

    public static FlyCam self;
    public Camera theCamera;
    public HudController hudControl;
    public int zoomSpeed = 1;
    public float moveSpeed = 5f,
        rotateSpeed = 10f;
    public bool isEnabled = false;

    private Vector3 moveVelocity = new Vector3(0f, 0f, 0f);

    private void Awake() {
        if (self == null) self = this;
        else Destroy(gameObject);
    }

    public void Enable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        hudControl.transform.GetComponent<Canvas>().enabled = false;
        hudControl.ToggleModelView(false);
        theCamera.depth = 10;
        isEnabled = true;
    }

    public void Disable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        hudControl.transform.GetComponent<Canvas>().enabled = true;
        hudControl.ToggleModelView(true);
        theCamera.depth = -10;
        isEnabled = false;
    }

    public void ZoomIn() {
        theCamera.fieldOfView -= zoomSpeed;
    }

    public void ZoomOut() {
        theCamera.fieldOfView += zoomSpeed;
    }

    public void MoveForward() {
        Vector3 newPos = transform.localPosition + transform.forward * moveSpeed; 
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime);
    }

    public void MoveBack() {
        Vector3 newPos = transform.localPosition + transform.forward * -moveSpeed;
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime);
    }

    public void MoveLeft() {
        Vector3 newPos = transform.localPosition + transform.right * -moveSpeed;
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime);
    }

    public void MoveRight() {
        Vector3 newPos = transform.localPosition + transform.right * moveSpeed;
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime);
    }

    public void MoveUp() {
        Vector3 newPos = transform.localPosition + transform.up * moveSpeed;
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime);
    }

    public void MoveDown() {
        Vector3 newPos = transform.localPosition + transform.up * -moveSpeed;
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime);
    }

    public void RotateUp() {
        theCamera.transform.Rotate(new Vector3(-rotateSpeed * Time.deltaTime, 0f, 0f));
    }

    public void RotateDown() {
        theCamera.transform.Rotate(new Vector3(rotateSpeed * Time.deltaTime, 0f, 0f));
    }

    public void RotateLeft() {
        theCamera.transform.Rotate(new Vector3(0f, -rotateSpeed * Time.deltaTime, 0f));
    }

    public void RotateRight() {
        theCamera.transform.Rotate(new Vector3(0f, rotateSpeed * Time.deltaTime, 0f));
    }

    public void TiltLeft() {
        theCamera.transform.Rotate(new Vector3(0f, 0f, rotateSpeed * Time.deltaTime));
    }

    public void TiltRight() {
        theCamera.transform.Rotate(new Vector3(0f, 0f, -rotateSpeed * Time.deltaTime));
    }
}