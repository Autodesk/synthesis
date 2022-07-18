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

    public void OnShoot(Vector3 HorizontalImpulse, Vector3 VerticalImpulse, Vector3 location, Quaternion rotation)
    {
        //configure being shown
        //add impulse
        transform.position = location + (transform.position - gameObject.GetComponentInChildren<MeshCollider>().transform.position);
        transform.GetComponentInChildren<MeshCollider>().gameObject.transform.rotation = rotation;
        transform.position += GetComponentInChildren<MeshCollider>().gameObject.transform.forward * 0.4f;//forward offset
        //transform.position = location;
        
        SetPieceState(true);
        rb.AddForce(HorizontalImpulse, ForceMode.Impulse);
        rb.AddForce(VerticalImpulse, ForceMode.Impulse);
        //Debug.Log($"HorizontalImpulse: {HorizontalImpulse} | VerticalImpulse {VerticalImpulse}");
        
        //reusing game objects
        scored = false;
    }

    private bool scored = false;
    private int value = 1;//TEMPORARY VALUES
    
    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Collision Detected" + collision.transform.tag);
        
        if (collision.transform.CompareTag("robot") && !currentlyHeld)
        {
            SetPieceState(false);
            Shooting.AddGamepiece(this);
        }

        //TEMPORARY SCORING
        if (collision.transform.CompareTag("blue zone") && !scored)
        {
            scored = true;
            Scoring.blueScore += value;
        }
        if (collision.transform.CompareTag("red zone") && !scored)
        {
            scored = true;
            Scoring.redScore += value;
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
