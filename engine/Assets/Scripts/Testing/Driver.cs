using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Driver : MonoBehaviour
{
    public bool go = false;

    public void FixedUpdate()
    {
        if (go) GetComponent<Rigidbody>().AddRelativeForce(new Vector3(3.0f, 0, 0));
    }
}
