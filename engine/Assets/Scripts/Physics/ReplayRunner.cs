using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Synthesis.Physics;
using Synthesis.PreferenceManager;
using Synthesis.Replay;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

public class ReplayRunner : MonoBehaviour {

    public GameObject ContactMarker;

    public const string TOGGLE_PLAY = "input/toggle_play";

    private void Start() {
        ReplayManager.IsRecording = true;

        // DynamicUIManager.ReplaySlider.minValue = -ReplayManager.TimeSpan;
        DynamicUIManager.ReplaySlider.AddOnValueChangedEvent((s, val) => {
            if (PhysicsManager.IsFrozen) {
                var frame = ReplayManager.GetFrameAtTime(val);
                frame?.ApplyFrame();
                ReplayManager.ShowContactsAtTime(val, 1.0f, 1.0f);
            }
        });
        DynamicUIManager.ReplaySlider.RootGameObject.SetActive(false);

        EventBus.NewTypeListener<PhysicsFreezeChangeEvent>(PhysChange);
        
        Digital toggleReplayInput;
        if (PreferenceManager.ContainsPreference(TOGGLE_PLAY)) {
            toggleReplayInput = (Digital)PreferenceManager.GetPreference<InputData[]>(TOGGLE_PLAY)[0].GetInput();
            toggleReplayInput.ContextBitmask = SimulationRunner.RUNNING_SIM_CONTEXT | SimulationRunner.REPLAY_SIM_CONTEXT;
        } else {
            toggleReplayInput = new Digital(Enum.GetName(typeof(KeyCode), KeyCode.Tab),
                context: SimulationRunner.RUNNING_SIM_CONTEXT | SimulationRunner.REPLAY_SIM_CONTEXT
            );
        }
        InputManager.UnassignDigitalInput(TOGGLE_PLAY);
        InputManager.AssignDigitalInput(TOGGLE_PLAY, toggleReplayInput, TogglePlay);

        ReplayManager.SetupDesyncTracker();
        ReplayManager.SetupContactUI(CreateContactMarker, EraseContactMarkers);
    }

    private List<GameObject> _contactMarkers = new List<GameObject>();
    private void EraseContactMarkers() {
        _contactMarkers.ForEach(x => Destroy(x));
        _contactMarkers.Clear();
    }

    private void CreateContactMarker(ContactReport report, float opacity = 1f) {
        var marker = Instantiate(ContactMarker, report.point, Quaternion.identity);
        var mat = marker.GetComponent<Renderer>();
        var color = mat.material.GetColor("TRANSPARENT_COLOR");
        color.a = opacity;
        mat.material.SetColor("TRANSPARENT_COLOR", color);
        _contactMarkers.Add(marker);
        marker.AddComponent<ContactMarkerHandler>();
    }

    private void PhysChange(IEvent e) {
        var physInfo = e as PhysicsFreezeChangeEvent;
    }

    private void TogglePlay(IEvent e) {
        var de = e as DigitalEvent;
        if (de.State == DigitalState.Down) {
            // if (PhysicsManager.IsFrozen)
            //     ReplayManager.MakeCurrentNewestFrame();
            PhysicsManager.IsFrozen = !PhysicsManager.IsFrozen;
            if (PhysicsManager.IsFrozen) {
                ReplayManager.NewestFrame.ApplyFrame();
                SimulationRunner.AddContext(SimulationRunner.REPLAY_SIM_CONTEXT);
                DynamicUIManager.ReplaySlider.SetValue(0);
                DynamicUIManager.ReplaySlider.RootGameObject.SetActive(true);
            } else {
                ReplayManager.CurrentFrame.ApplyFrame();
                ReplayManager.InvalidateRecording();
                SimulationRunner.RemoveContext(SimulationRunner.REPLAY_SIM_CONTEXT);
                DynamicUIManager.ReplaySlider.RootGameObject.SetActive(false);
                if (ReplayManager.EraseContactMarkers != null)
                    ReplayManager.EraseContactMarkers();
                ReplayManager.MakeCurrentNewestFrame();
            }
        }
    }

    private void FixedUpdate() {
        if (!PhysicsManager.IsFrozen)
            ReplayManager.RecordFrame();
    }
}

public class ContactMarkerHandler : MonoBehaviour {

        public const int LEFT_BOUNDS = 30;
        public const int RIGHT_BOUNDS = 30;
        public const int TOP_BOUNDS = 100;
        public const int BOTTOM_BOUNDS = 150;

        private Renderer? _r;

        private void Start() {
            _r = GetComponent<Renderer>();
        }

        private void FixedUpdate() {
            var sp = Camera.main.WorldToScreenPoint(transform.position);
            // Hate it but whatever
            if (sp.x < LEFT_BOUNDS || sp.x > Screen.currentResolution.width - RIGHT_BOUNDS
                || sp.y < BOTTOM_BOUNDS || sp.y > Screen.currentResolution.height - TOP_BOUNDS) {
                    _r!.enabled = false;
            } else {
                _r!.enabled = true;
            }
        }
    }
