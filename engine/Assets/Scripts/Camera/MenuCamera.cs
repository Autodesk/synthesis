using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour {
    // TODO UPDATE NAMESPACE

    private void Start() {
        transform.localPosition = transform.up * 0.5f + transform.forward * -1 + offset;
    }

    Vector3 lookPoint = new Vector3(0, 1, 0);
    Vector3 offset    = new Vector3(0, 1, 15);

    private void Update() {
        transform.RotateAround(lookPoint, Vector3.down, Time.deltaTime * 3f);
        transform.LookAt(lookPoint);
    }
}
