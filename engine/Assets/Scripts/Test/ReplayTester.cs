using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Synthesis.Physics;
using Synthesis.Replay;
using UnityEngine;

public class ReplayTester : MonoBehaviour {

    public UnityEngine.UI.Slider Slider;

    private void Start() {
        ReplayManager.IsRecording = true;
        Slider.minValue = -ReplayManager.TimeSpan;
        Slider.onValueChanged.AddListener(x => {
            if (PhysicsManager.IsFrozen) {
                var frame = ReplayManager.GetFrameAtTime(x);
                frame?.ApplyFrame();
            }
        });
        Slider.gameObject.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            PhysicsManager.IsFrozen = !PhysicsManager.IsFrozen;
            if (!PhysicsManager.IsFrozen) {
                ReplayManager.InvalidateRecording();
                Slider.gameObject.SetActive(false);
            } else {
                Slider.gameObject.SetActive(true);
                Slider.value = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            RobotSimObject.GetCurrentlyPossessedRobot().GroundedNode.GetComponent<Rigidbody>().velocity = Vector3.up * 10;
        }
    }

    private void FixedUpdate() {
        if (!PhysicsManager.IsFrozen)
            ReplayManager.RecordFrame();
    }
}
