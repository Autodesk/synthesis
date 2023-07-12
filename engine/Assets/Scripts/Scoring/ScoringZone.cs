using System.Collections.Generic;
using System.Security.Policy;
using Synthesis.Physics;
using Synthesis.Runtime;
using SynthesisAPI.EventBus;
using UnityEngine;

public enum Alliance {
    Red,
    Blue
}

public class ScoringZone : IPhysicsOverridable {
    private ScoringZoneData _zoneData;
    public ScoringZoneData ZoneData {
        get => _zoneData;
        set {
            _zoneData                   = value;
            GameObject.name             = _zoneData.Name;
            GameObject.tag              = _zoneData.Alliance == Alliance.Red ? "red zone" : "blue zone";
            GameObject.transform.parent = FieldSimObject.CurrentField.FieldObject.transform.Find(_zoneData.Parent);
            Alliance                    = _zoneData.Alliance;
            GameObject.transform.localPosition =
                new Vector3(_zoneData.LocalPosition.x, _zoneData.LocalPosition.y, _zoneData.LocalPosition.z);
            GameObject.transform.localRotation = new Quaternion(_zoneData.LocalRotation.x, _zoneData.LocalRotation.y,
                _zoneData.LocalRotation.z, _zoneData.LocalRotation.w);
            GameObject.transform.localScale =
                new Vector3(_zoneData.LocalScale.x, _zoneData.LocalScale.y, _zoneData.LocalScale.z);
        }
    }

    public string Name => _zoneData.Name;

    public Alliance Alliance {
        get => _zoneData.Alliance;
        set {
            _zoneData.Alliance           = value;
            _meshRenderer.material.color = value == Alliance.Red ? Color.red : Color.blue;
        }
    }

    public int Points            => _zoneData.Points;
    public bool DestroyGamepiece => _zoneData.DestroyGamepiece;
    public bool PersistentPoints => _zoneData.PersistentPoints;
    public GameObject GameObject;
    private Collider _collider;
    private MeshRenderer _meshRenderer;

    private bool _isFrozen;

    public ScoringZone(GameObject gameObject, string name, Alliance alliance, int points, bool destroyGamepiece,
        bool persistentPoints) {
        _zoneData  = new() { Name = name, PersistentPoints = persistentPoints, DestroyGamepiece = destroyGamepiece,
             Points = points };
        GameObject = gameObject;
        GameObject.layer = 2; // ignore raycast layer

        // configure gameobject to have box collider as trigger
        GameObject.name = name;

        ScoringZoneListener listener = GameObject.AddComponent<ScoringZoneListener>();
        listener.ScoringZone         = this;

        _collider                    = GameObject.GetComponent<Collider>();
        _meshRenderer                = GameObject.GetComponent<MeshRenderer>();
        _meshRenderer.material.color = alliance == Alliance.Red ? Color.red : Color.blue;

        _collider.isTrigger = true;

        PhysicsManager.Register(this);
    }

    public ScoringZone(ScoringZoneData data) {
        GameObject                   = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.layer             = 2; // ignore raycast layer
        ScoringZoneListener listener = GameObject.AddComponent<ScoringZoneListener>();
        listener.ScoringZone         = this;
        _collider                    = GameObject.GetComponent<Collider>();
        _meshRenderer                = GameObject.GetComponent<MeshRenderer>();
        _collider.isTrigger          = true;
        ZoneData                     = data;

        PhysicsManager.Register(this);
    }

    public void SetVisibility(bool visible) {
        _meshRenderer.enabled = visible;
    }

    public bool isFrozen() => _isFrozen;

    public void Freeze() {
        _isFrozen           = true;
        _collider.isTrigger = false;
    }

    public void Unfreeze() {
        _isFrozen           = false;
        _collider.isTrigger = true;
    }

    public List<Rigidbody> GetAllRigidbodies() => new List<Rigidbody> {};
    public GameObject GetRootGameObject()      => GameObject;
}

public class ScoringZoneListener : MonoBehaviour {
    public ScoringZone ScoringZone;

    private SortedDictionary<int, Collider> _inScoringZone = new SortedDictionary<int, Collider>();

    private void OnTriggerEnter(Collider other) {
        if (ScoringZone.isFrozen())
            return;
        if (other.gameObject == gameObject)
            return;
        if (!other.transform.CompareTag("gamepiece"))
            return;
        if (_inScoringZone.ContainsKey(other.GetHashCode()))
            return;

        // don't destroy gamepiece if user is moving the zone
        if (SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT))
            return;

        // trigger scoring
        EventBus.Push(new OnScoreUpdateEvent(other.name, ScoringZone));
        _inScoringZone.Add(other.GetHashCode(), other);

        if (ScoringZone.DestroyGamepiece)
            Destroy(other.gameObject);
    }

    private void OnTriggerExit(Collider other) {
        if (ScoringZone.isFrozen())
            return;
        if (other.gameObject == gameObject)
            return;
        if (!other.transform.CompareTag("gamepiece"))
            return;

        if (_inScoringZone.ContainsKey(other.GetHashCode())) {
            if (!ScoringZone.PersistentPoints)
                EventBus.Push(new OnScoreUpdateEvent(other.name, ScoringZone, false));
            _inScoringZone.Remove(other.GetHashCode());
        }
    }

    private void OnDestroy() {
        PhysicsManager.Unregister(ScoringZone);
    }
}

public class OnScoreUpdateEvent : IEvent {
    public string Name;
    public ScoringZone Zone;
    public bool IncreaseScore;

    /// <summary>
    /// OnScoreEvent pushed when gamepiece collides with scoring zone
    /// </summary>
    /// <param name="name">Name of gamepiece object</param>
    /// <param name="zone">Scoring Zone</param>
    public OnScoreUpdateEvent(string name, ScoringZone zone, bool increaseScore = true) {
        Name          = name;
        Zone          = zone;
        IncreaseScore = increaseScore;
    }
}