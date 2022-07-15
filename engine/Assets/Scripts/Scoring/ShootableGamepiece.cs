using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableGamepiece : MonoBehaviour
{

    void Start()
    {
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
    private void OnCollisionEnter(Collision collision)
    {
        //check if other is robot
        //add self to queue
        //disable self
        if (collision.transform.gameObject.CompareTag("robot"))
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
        gameObject.GetComponent<MeshRenderer>().enabled = enabled;
        gameObject.GetComponent<Rigidbody>().isKinematic = !enabled;
        gameObject.GetComponent<Collider>().enabled = enabled;
    }
}
