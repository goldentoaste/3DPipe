using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCam : MonoBehaviour {
    public float xSensitivity;
    public float ySensitivity;
    private float xRotation;
    private float yRotation;
    [HideInInspector]
    public float yCurrentRotation;
    private float yRotationV;
    public float smoothTime;
    [HideInInspector]
    public float xCurrentRotation;
    private float xRotationV;
    public float yMin;
    public float yMax;

    public float moveSpeed;
    // Start is called before the first frame update
    void Start () {

    }

    // Update is called once per frame
    void Update () {

        if (Input.GetMouseButton (0)) {
            xRotation += Input.GetAxis ("Mouse X") * xSensitivity;
            yRotation += -Input.GetAxis ("Mouse Y") * ySensitivity;

            yRotation = Mathf.Clamp (yRotation, yMin, yMax);
            transform.rotation = Quaternion.Euler (yCurrentRotation, xCurrentRotation, 0f);
            yCurrentRotation = Mathf.SmoothDamp (yCurrentRotation, yRotation, ref yRotationV, smoothTime);
            xCurrentRotation = Mathf.SmoothDamp (xCurrentRotation, xRotation, ref xRotationV, smoothTime);
        }

        CharacterController controller = GetComponent<CharacterController> ();
        Vector3 movement = new Vector3 (Input.GetAxisRaw ("Horizontal"), Input.GetKey (KeyCode.Space) ? 1 : (Input.GetKey (KeyCode.LeftShift) ? -1 : 0), Input.GetAxisRaw ("Vertical"));
        movement = transform.rotation * movement.normalized;
        controller.Move (movement * moveSpeed * Time.deltaTime);
    }
}