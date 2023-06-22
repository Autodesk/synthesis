using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoCamera : MonoBehaviour {
    Camera cam;
    // configures some values of Gizmo Camera to be the same as the main cam
    void Awake() {
        cam               = gameObject.GetComponent<Camera>();
        cam.fieldOfView   = Camera.main.fieldOfView;
        cam.focalLength   = Camera.main.focalLength;
        cam.sensorSize    = Camera.main.sensorSize;
        cam.lensShift     = Camera.main.lensShift;
        cam.rect          = Camera.main.rect;
        cam.farClipPlane  = Camera.main.farClipPlane;
        cam.nearClipPlane = Camera.main.nearClipPlane;
    }
}
