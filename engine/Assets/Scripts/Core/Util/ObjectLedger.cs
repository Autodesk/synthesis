using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLedger : MonoBehaviour
{
    public static ObjectLedger Instance { get; private set; }

    public Material spawnMat;
    private List<GameObject> indexedObjects;

    private void Awake()
    {
        Instance = this;

        indexedObjects = new List<GameObject>(FindObjectsOfType<GameObject>());

        foreach (GameObject obj in indexedObjects)
        {
            print(obj.name);
        }
    }

    public GameObject GetObject(string name)
    {
        return indexedObjects.Find(x => x.name.Equals(name));
    }
}
