using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MouseListener : MonoBehaviour
{
    bool mouseDown = false;
    BRigidBody rigidBody;
    
    void Start()
    {
        rigidBody = GetComponent<BRigidBody>();

        if (rigidBody == null)
            rigidBody = GetComponentInParent<BRigidBody>();
    }

    void OnMouseDown()
    {
        if (rigidBody == null)
            return;

        //rigidBody.linearDamping = 10f;
        mouseDown = true;

        Debug.Log("Mouse down!");
    }

    void Update()
    {
        if (!mouseDown)
            return;

        rigidBody.AddForce(new Vector3(100f, 100f, 100f));
        
        if (!Input.GetMouseButton(1))
        {
            mouseDown = false;

            rigidBody.linearDamping = 0f;
        }
    }
}
