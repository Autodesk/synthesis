using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLedger : MonoBehaviour {
    public static ObjectLedger Instance { get; private set; }

    public Material spawnMat;
    private static List<GameObject> indexedObjects;

    private void Awake() {
        Instance = this;

        indexedObjects = new List<GameObject>(FindObjectsOfType<GameObject>());
    }

    public static GameObject GetObject(string name) {
        return indexedObjects.Find(x => x.name.Equals(name));
    }
}
