using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoTester : MonoBehaviour
{
    public GameObject gizmo;
    public GameObject cube;

    public void moveCube() //activates the gizmo to move the cube as a test
    {
        cube.SetActive(true);
        GizmoManager.SpawnGizmo(gizmo, cube.GetComponent<Transform>(), true);
    }
}
