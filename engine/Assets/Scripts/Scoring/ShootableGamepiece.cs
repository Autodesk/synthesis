using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableGamepiece : MonoBehaviour {
    public GamepieceSimObject SimObject;
    private MeshRenderer _mesh;
    private Rigidbody _rb;
    private MeshCollider _collider;

    private bool _requireExit = false;

    private void Start() {
        _mesh     = gameObject.GetComponentInChildren<MeshRenderer>();
        _rb       = gameObject.GetComponent<Rigidbody>();
        _collider = gameObject.GetComponentInChildren<MeshCollider>();
    }

    void Update() {
    }

    public void RequireExit() {
        _requireExit = true;
    }

    public bool currentlyHeld = false;

    private bool scored = false;
    private int value   = 1; // TEMPORARY VALUES

    private void OnTriggerStay(Collider collider) {
        if (RobotSimObject.CurrentlyPossessedRobot == string.Empty) {
            return;
        }

        if (collider.transform.CompareTag("robot") && RobotSimObject.GetCurrentlyPossessedRobot().PickingUpGamepieces) {
            RobotSimObject.GetCurrentlyPossessedRobot().CollectGamepiece(SimObject);
        }
    }

    private void OnTriggerEnter(Collider collider) {
        // TEMPORARY SCORING
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
        if (collider.transform.CompareTag("robot")) {
            _requireExit = false;
        }
    }
}
