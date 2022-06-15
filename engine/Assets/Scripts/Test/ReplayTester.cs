using System.Collections;
using System.Collections.Generic;
using Synthesis.Physics;
using UnityEngine;

public class ReplayTester : MonoBehaviour {
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Y)) {
            PhysicsManager.IsFrozen = !PhysicsManager.IsFrozen;
        }
    }
}
