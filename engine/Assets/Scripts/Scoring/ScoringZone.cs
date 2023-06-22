using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.Runtime;
using SynthesisAPI.EventBus;
using UnityEngine;

public enum Alliance
{
    Red,
    Blue
}

public class ScoringZone
{
    public string Name;

    private Alliance _alliance;
    public Alliance Alliance
    {
        get => _alliance;
        set {
            _alliance = value;
            _renderer.material.color = value == Alliance.Red ? Color.red : Color.blue;
        }
    }
    public int Points;
    public bool DestroyObject;
    public GameObject GameObject;
    private Collider _collider;
    private MeshRenderer _meshRenderer;
    private Renderer _renderer;

    public ScoringZone(GameObject gameObject, string name, Alliance alliance, int points, bool destroyObject)
    {
        this.Name = name;
        this.GameObject = gameObject;
        
        // configure gameobject to have box collider as trigger
        gameObject.name = name;
        
        // make scoring zone transparent
        _renderer = gameObject.GetComponent<Renderer>();
        // renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));
        _renderer.material.color = alliance == Alliance.Red ? Color.red : Color.blue;

        ScoringZoneListener listener = gameObject.AddComponent<ScoringZoneListener>();
        listener.scoringZone = this;
        
        _collider = gameObject.GetComponent<Collider>();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();

        _collider.isTrigger = true;
        
        Alliance = alliance;
        Points = points;
        DestroyObject = destroyObject;
    }

    public void SetVisibility(bool visible)
    {
        GameObject.GetComponent<Renderer>().enabled = visible;
    }
}

public class ScoringZoneListener : MonoBehaviour
{
    public ScoringZone scoringZone;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == gameObject) return;
        if (!other.transform.CompareTag("gamepiece")) return;

        // don't destroy gamepiece if user is moving the zone
        if (SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT)) return;
        
        // trigger scoring
        EventBus.Push(new OnScoreEvent(other.name, scoringZone));
        
        if (scoringZone.DestroyObject) Destroy(other.gameObject);
    }
}

public class OnScoreEvent : IEvent
{
    public string name;
    public ScoringZone zone;

    /// <summary>
    /// OnScoreEvent pushed when gamepiece collides with scoring zone
    /// </summary>
    /// <param name="name">Name of gamepiece object</param>
    /// <param name="zone">Scoring Zone</param>
    public OnScoreEvent(string name, ScoringZone zone)
    {
        this.name = name;
        this.zone = zone;
    }
}