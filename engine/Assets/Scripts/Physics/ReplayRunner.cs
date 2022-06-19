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
    public GameObject ContactMarker;

    public const string TOGGLE_PLAY = "input/toggle_play";

    private void Start() {
        ReplayManager.IsRecording = true;
        Slider.minValue = -ReplayManager.TimeSpan;
        Slider.onValueChanged.AddListener(x => {
            if (PhysicsManager.IsFrozen) {
                var frame = ReplayManager.GetFrameAtTime(x);
                frame?.ApplyFrame();
                ReplayManager.ShowContactsAtTime(x);
            }
        });
        Slider.gameObject.SetActive(false);

        EventBus.NewTypeListener<PhysicsFreezeChangeEvent>(PhysChange);
        // if () {

        // }
        InputManager.AssignDigitalInput(TOGGLE_PLAY,
            PreferenceManager.ContainsPreference(TOGGLE_PLAY)
                ? (Digital)PreferenceManager.GetPreference<InputData[]>(TOGGLE_PLAY)[0].GetInput()
                : new Digital(Enum.GetName(typeof(KeyCode), KeyCode.Tab)),
            TogglePlay
        );

        ReplayManager.SetupDesyncTracker();
        ReplayManager.SetupContactUI(CreateContactMarker, EraseContactMarkers);
    }

    private List<GameObject> _contactMarkers = new List<GameObject>();
    private void EraseContactMarkers() {
        _contactMarkers.ForEach(x => Destroy(x));
        _contactMarkers.Clear();
    }

    private void CreateContactMarker(ContactReport report, float opacity = 1f) {
        report.Points.ForEach(x => {
            var marker = Instantiate(ContactMarker, x.point, Quaternion.identity);
            var mat = marker.GetComponent<Renderer>();
            var color = mat.material.GetColor("TRANSPARENT_COLOR");
            color.a = opacity;
            mat.material.SetColor("TRANSPARENT_COLOR", color);
            _contactMarkers.Add(marker);
            marker.AddComponent<ContactMarkerHandler>();
        });
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
                Slider.value = 0;
                Slider.gameObject.SetActive(true);
            } else {
                Slider.gameObject.SetActive(false);
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
