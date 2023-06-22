using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoStore : MonoBehaviour {
    public GameObject GizmoPrefab;
    public static GameObject GizmoPrefabStatic;

    public void Awake() {
        if (GizmoPrefab == null) {
            throw new System.Exception();
        }

        GizmoPrefabStatic = GizmoPrefab;
    }
}
