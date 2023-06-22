using Synthesis.Runtime;
using SynthesisAPI.EventBus;
using System;
using UnityEngine;

public enum Alliance {
    RED,
    BLUE
}

public class ScoringZone {
    public Alliance alliance;
    public int points;
    public bool destroyObject;
    private GameObject gameObject;
    private Collider collider;
    private MeshRenderer meshRenderer;

    public ScoringZone(GameObject gameObject, Alliance alliance, int points, bool destroyObject) {
        this.gameObject    = gameObject;
        this.alliance      = alliance;
        this.points        = points;
        this.destroyObject = destroyObject;

        // Configure gameobject to have translucent material and have box collider as trigger
        gameObject.name = "Test Scoring Zone";

        // Make scoring zone transparent
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));

        ScoringZoneListener listener = gameObject.AddComponent<ScoringZoneListener>();
        listener.scoringZone         = this;

        collider     = gameObject.GetComponent<Collider>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        collider.isTrigger = true;
    }

    public void SetVisibility(bool visible) {
        gameObject.GetComponent<Renderer>().enabled = visible;
    }
}

public class ScoringZoneListener : MonoBehaviour {
    public ScoringZone scoringZone;
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == gameObject) {
            return;
        } else if (!other.transform.CompareTag("gamepiece")) {
            return;
        } else if (SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT)) {
            // Don't destroy gamepiece if user is moving the zone
            return;
        }

        // Trigger scoring
        EventBus.Push(new OnScoreEvent(other.name, scoringZone));

        if (scoringZone.destroyObject) {
            Destroy(other.gameObject);
        }
    }
}

public class OnScoreEvent : IEvent {
    public string name;
    public ScoringZone zone;

    public OnScoreEvent(string name, ScoringZone zone) {
        this.name = name;
        this.zone = zone;
    }
}
