using System.Collections.Generic;
using Synthesis.Physics;
using Synthesis.Runtime;
using SynthesisAPI.EventBus;
using UnityEngine;
public enum Alliance
{
    Red,
    Blue
}

public class ScoringZone : IPhysicsOverridable
{
    public string Name;

    private Alliance _alliance;
    public Alliance Alliance
    {
        get => _alliance;
        set {
            _alliance = value;
            _meshRenderer.material.color = value == Alliance.Red ? Color.red : Color.blue;
        }
    }
    public int Points;
    public bool DestroyObject;
    public GameObject GameObject;
    private Collider _collider;
    private MeshRenderer _meshRenderer;

    private bool _isFrozen;

    public ScoringZone(GameObject gameObject, string name, Alliance alliance, int points, bool destroyObject)
    {
        this.Name = name;
        this.GameObject = gameObject;
        
        // configure gameobject to have box collider as trigger
        GameObject.name = name;
        
        ScoringZoneListener listener = GameObject.AddComponent<ScoringZoneListener>();
        listener.ScoringZone = this;
        
        _collider = GameObject.GetComponent<Collider>();
        _meshRenderer = GameObject.GetComponent<MeshRenderer>();
        _meshRenderer.material.color = alliance == Alliance.Red ? Color.red : Color.blue;

        _collider.isTrigger = true;
        
        Alliance = alliance;
        Points = points;
        DestroyObject = destroyObject;

        PhysicsManager.Register(this);
    }

    public void SetVisibility(bool visible) {
        _meshRenderer.enabled = visible;
    }

    public bool isFrozen() => _isFrozen;
    public void Freeze() {
        _isFrozen = true;
        _collider.isTrigger = false;
    }
    public void Unfreeze() {
        _isFrozen = false;
        _collider.isTrigger = true;
    }

    public List<Rigidbody> GetAllRigidbodies()
        => new List<Rigidbody>{};
    public GameObject GetRootGameObject()
        => GameObject;
}

public class ScoringZoneListener : MonoBehaviour
{
    public ScoringZone ScoringZone;
    
    private SortedDictionary<int, Collider> _inScoringZone = new SortedDictionary<int, Collider>();
    
    private void OnTriggerEnter(Collider other) {
        if (ScoringZone.isFrozen()) return;
        if (other.gameObject == gameObject) return;
        if (!other.transform.CompareTag("gamepiece")) return;
        if (_inScoringZone.ContainsKey(other.GetHashCode())) return;

        // don't destroy gamepiece if user is moving the zone
        if (SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT)) return;
        
        // trigger scoring
        EventBus.Push(new OnScoreEvent(other.name, ScoringZone));
        _inScoringZone.Add(other.GetHashCode(), other);
        
        if (ScoringZone.DestroyObject) Destroy(other.gameObject);
    }

    private void OnTriggerExit(Collider other) {
        if (ScoringZone.isFrozen()) return;
        if (other.gameObject == gameObject) return;
        if (!other.transform.CompareTag("gamepiece")) return;

        if (_inScoringZone.ContainsKey(other.GetHashCode()))
            _inScoringZone.Remove(other.GetHashCode());
    }

    private void OnDestroy() {
        PhysicsManager.Unregister(ScoringZone);
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