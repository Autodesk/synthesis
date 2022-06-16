using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Synthesis.Physics;
using Synthesis.PreferenceManager;
using Synthesis.Replay;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

public class ReplayRunner : MonoBehaviour {

    public UnityEngine.UI.Slider Slider;

    public const string TOGGLE_PLAY = "input/toggle_play";

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

        EventBus.NewTypeListener<PhysicsFreezeChangeEvent>((EventBus.EventCallback)PhysChange);
        // if () {

        // }
        InputManager.AssignDigitalInput(TOGGLE_PLAY,
            PreferenceManager.ContainsPreference(TOGGLE_PLAY)
                ? (Digital)PreferenceManager.GetPreference<InputData[]>(TOGGLE_PLAY)[0].GetInput()
                : new Digital(Enum.GetName(typeof(KeyCode), KeyCode.Tab)),
            TogglePlay
        );
        // InputManager.AssignDigitalInput()
    }

    private void PhysChange(IEvent e) {
        var physInfo = e as PhysicsFreezeChangeEvent;
    }

    private void TogglePlay(IEvent e) {
        var de = e as DigitalEvent;
        if (de.State == DigitalState.Down) {
            if (PhysicsManager.IsFrozen)
                ReplayManager.MakeCurrentNewestFrame();
            PhysicsManager.IsFrozen = !PhysicsManager.IsFrozen;
            if (PhysicsManager.IsFrozen) {
                Slider.gameObject.SetActive(true);
                Slider.value = 0;
            } else {
                Slider.gameObject.SetActive(false);
            }
        }
    }

    private void Update() {
        // if (Input.GetKeyDown(KeyCode.P)) {
        //     PhysicsManager.IsFrozen = !PhysicsManager.IsFrozen;
        //     if (!PhysicsManager.IsFrozen) {
        //         ReplayManager.InvalidateRecording();
                
        //     } else {
                
                
        //     }
        // }

        if (UnityEngine.Input.GetKeyDown(KeyCode.B)) {
            RobotSimObject.GetCurrentlyPossessedRobot().GroundedNode.GetComponent<Rigidbody>().velocity = Vector3.up * 10;
        }
    }

    private void FixedUpdate() {
        if (!PhysicsManager.IsFrozen)
            ReplayManager.RecordFrame();
    }
}
