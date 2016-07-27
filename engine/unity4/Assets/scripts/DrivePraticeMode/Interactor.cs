using UnityEngine;
using System.Collections;

/// <summary>
/// Class that is attached to any game object that has been initialized as a ball roller.
/// Used for Driver Practice Mode.
/// </summary>
public class Interactor : MonoBehaviour {

    private GameObject collisionObject;
    private bool collisionDetected = false;
    public string collisionKeyword;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains(collisionKeyword))
        {
            collisionDetected = true;
            collisionObject = collision.gameObject;
            Debug.Log("WOW" + collision.gameObject.name);
            gameObject.transform.GetChild(0).renderer.material.SetColor("_SpecColor", Color.red);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name.Contains(collisionKeyword))
        {
            collisionDetected = false;
            gameObject.transform.GetChild(0).renderer.material.SetColor("_SpecColor", Color.white);
        }
    }

    public GameObject getObject()
    {
        return collisionObject;
    }

    public bool getDetected()
    {
        return collisionDetected;
    }
}
