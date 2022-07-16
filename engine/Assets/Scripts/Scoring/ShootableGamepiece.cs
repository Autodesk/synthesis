using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableGamepiece : MonoBehaviour
{
    private MeshRenderer mesh;
    private Rigidbody rb;
    private MeshCollider collider;
    void Start()
    {
        mesh = gameObject.GetComponentInChildren<MeshRenderer>();
        rb = gameObject.GetComponent<Rigidbody>();
        collider = gameObject.GetComponentInChildren<MeshCollider>();
        ResetGamepiece();
    }

    void Update()
    {

    }

    public bool currentlyHeld = false;

    public void OnShoot(Vector3 HorizontalImpulse, Vector3 VerticalImpulse, Vector3 location)
    {
        //configure being shown
        //add impulse
        transform.position = location;
        SetPieceState(true);
        GetComponent<Rigidbody>().AddForce(HorizontalImpulse, ForceMode.Impulse);
        GetComponent<Rigidbody>().AddForce(VerticalImpulse, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collision Detected" + collision.transform.tag);
        //check if other is robot
        //add self to queue
        //disable self
        if (collision.transform.CompareTag("robot") && !currentlyHeld)
        {
            SetPieceState(false);
            Shooting.AddGamepiece(this);
        }
    }
    public void ResetGamepiece()
    {
        SetPieceState(true);
    }
    private void SetPieceState(bool enabled)
    {
        currentlyHeld = !enabled;
        mesh.enabled = enabled;
        rb.isKinematic = !enabled;
        collider.enabled = enabled;
    }
}
