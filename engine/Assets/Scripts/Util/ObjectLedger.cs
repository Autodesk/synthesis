using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLedger : MonoBehaviour
{
    private static ObjectLedger instance;
    public static ObjectLedger Instance {
        get {
            if (instance == null) instance = new ObjectLedger();
            return instance;
        }
    }

    private List<GameObject> indexedObjects;

    private ObjectLedger()
    {
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
