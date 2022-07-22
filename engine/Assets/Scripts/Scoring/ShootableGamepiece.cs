using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableGamepiece : MonoBehaviour {

    public GamepieceSimObject SimObject;
    private MeshRenderer _mesh;
    private Rigidbody _rb;
    private MeshCollider _collider;

    private bool _requireExit = false;

    void Start() {
        _mesh = gameObject.GetComponentInChildren<MeshRenderer>();
        _rb = gameObject.GetComponent<Rigidbody>();
        _collider = gameObject.GetComponentInChildren<MeshCollider>();
        // ResetGamepiece();
    }

    void Update() {

    }

    public void RequireExit() {
        _requireExit = true;
    }

    public bool currentlyHeld = false;

    // public void OnShoot(Vector3 HorizontalImpulse, Vector3 VerticalImpulse, Vector3 location, Quaternion rotation)
    // {
    //     //configure being shown
    //     //add impulse
    //     transform.position = location + (transform.position - gameObject.GetComponentInChildren<MeshCollider>().transform.position);
    //     transform.GetComponentInChildren<MeshCollider>().gameObject.transform.rotation = rotation;
    //     transform.position += GetComponentInChildren<MeshCollider>().gameObject.transform.forward * 0.4f;//forward offset
    //     //transform.position = location;
        
    //     SetPieceState(true);
    //     rb.AddForce(HorizontalImpulse, ForceMode.Impulse);
    //     rb.AddForce(VerticalImpulse, ForceMode.Impulse);
    //     //Debug.Log($"HorizontalImpulse: {HorizontalImpulse} | VerticalImpulse {VerticalImpulse}");
        
    //     //reusing game objects
    //     scored = false;
    // }

    private bool scored = false;
    private int value = 1;//TEMPORARY VALUES
    
    // private void OnCollisionEnter(Collision collision) {
        
    // }

    private void OnTriggerStay(Collider collider) {
        if (RobotSimObject.CurrentlyPossessedRobot == string.Empty)
            return;

        if (collider.transform.CompareTag("robot") && RobotSimObject.GetCurrentlyPossessedRobot().PickingUpGamepieces) {
            // SetPieceState(false);
            // Shooting.AddGamepiece(this);
            RobotSimObject.GetCurrentlyPossessedRobot().CollectGamepiece(SimObject);
        }
    }

    private void OnTriggerEnter(Collider collider) {
        //Debug.Log("Collision Detected" + collision.transform.tag);
        
        

        //TEMPORARY SCORING
        if (collider.transform.CompareTag("blue zone") && !scored) {
            scored = true;
            Scoring.blueScore += value;
        }
        if (collider.transform.CompareTag("red zone") && !scored) {
            scored = true;
            Scoring.redScore += value;
        }
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.transform.CompareTag("robot"))
            _requireExit = false;
    }
    
    // public void ResetGamepiece()
    // {
    //     SetPieceState(true);
    // }
    // private void SetPieceState(bool enabled)
    // {
    //     currentlyHeld = !enabled;
    //     mesh.enabled = enabled;
    //     rb.isKinematic = !enabled;
    //     collider.enabled = enabled;
    // }
}
