using UnityEngine;

public class AsplosionScript : MonoBehaviour {

    public bool moveIt = false;
    public Vector3 moveToPos;
    public float startToMove,
        timeToMove = 0;

    public bool scaleIt = false;
    public Vector3 scaleTo;
    public float startToScale,
        timeToScale = 0;

    public bool rotateIt = false;
    public Vector3 rotateVelocity;
    public float startToRotate,
        timeToRotate = 0;

    private NukeScript control;
    private Vector3 startPos,
        startScale;
    private float stopMoveBy,
        stopScaleBy,
        stopRotateBy,
        moveTime = 0,
        scaleTime = 0;

    private void Awake() {
        control = transform.GetComponentInParent<NukeScript>();
    }

    private void Start() {
        stopMoveBy = startToMove + timeToMove;
        stopScaleBy = startToScale + timeToScale;
        stopRotateBy = startToRotate + timeToRotate;

        startPos = transform.localPosition;
        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update() {
        if (moveIt && control.timer >= startToMove) {
            moveTime += Time.deltaTime / timeToMove;
            transform.localPosition = Vector3.Lerp(startPos, moveToPos, moveTime);
        }
        if (control.timer >= stopMoveBy) moveIt = false;

        if (scaleIt && control.timer >= startToScale) {
            scaleTime += Time.deltaTime / timeToScale;
            transform.localScale = Vector3.Lerp(startScale, scaleTo, scaleTime);
        }
        if (control.timer >= stopScaleBy) scaleIt = false;

        if (rotateIt && control.timer >= startToRotate) {
            transform.Rotate(rotateVelocity * Time.deltaTime);
        }
        if (control.timer >= stopRotateBy) rotateIt = false;
    }

}