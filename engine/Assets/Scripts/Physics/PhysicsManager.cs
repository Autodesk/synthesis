using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using SynthesisAPI.EventBus;
using Synthesis.Replay;

#nullable enable

namespace Synthesis.Physics {
    /// <summary>
    /// Controls physics within unity
    /// </summary>
    public static class PhysicsManager {
        private static Dictionary<int, IPhysicsOverridable> _physObjects = new Dictionary<int, IPhysicsOverridable>();
        private static Dictionary<int, List<ContactRecorder>> _contactRecorders = new Dictionary<int, List<ContactRecorder>>();

        private static bool _isFrozen = false;
        public static bool IsFrozen {
            get => _isFrozen;
            set {
                if (_isFrozen != value) {
                    _isFrozen = value;
                    if (_isFrozen) {
                        UnityEngine.Physics.autoSimulation = false;
                        _physObjects.ForEach(x => {
                            x.Value.Freeze();
                        });
                    } else {
                        UnityEngine.Physics.autoSimulation = true;
                        _physObjects.ForEach(x => {
                            x.Value.Unfreeze();
                        });
                    }
                    EventBus.Push(new PhysicsFreezeChangeEvent { IsFrozen = _isFrozen });
                }
            }
        }
        
        public static bool Register<T>(T overridable) where T: class, IPhysicsOverridable {
            if (_physObjects.ContainsKey(overridable.GetHashCode()))
                return false;
            _physObjects[overridable.GetHashCode()] = overridable;
            AddContactRecorders(overridable);
            return true;
        }

        public static void AddContactRecorders<T>(T overridable) where T: class, IPhysicsOverridable {
            List<ContactRecorder> recorders = new List<ContactRecorder>();
            var rbs = overridable.GetRootGameObject().GetComponentsInChildren<Rigidbody>();
            rbs.ForEach(x => {
                var recorder = x.gameObject.AddComponent<ContactRecorder>();
                recorders.Add(recorder);
            });
            _contactRecorders[overridable.GetHashCode()] = recorders;
        }

        public static bool Unregister<T>(T overridable) where T: class, IPhysicsOverridable {
            if (!_physObjects.ContainsKey(overridable.GetHashCode()))
                return false;
            _physObjects.Remove(overridable.GetHashCode());
            return true;
        }

        public static List<IPhysicsOverridable> GetAllOverridable()
            => new List<IPhysicsOverridable>(_physObjects.Values);

        public static List<ContactRecorder>? GetContactRecorders<T>(T overridable) where T: class, IPhysicsOverridable {
            if (!_contactRecorders.ContainsKey(overridable.GetHashCode()))
                return null;
            return _contactRecorders[overridable.GetHashCode()];
        }
    }

    public class ContactRecorder : MonoBehaviour {
        public List<ContactReport> Reports = new List<ContactReport>();

        public void OnCollisionEnter(Collision c) {
            if (GetComponent<Rigidbody>().isKinematic && c.rigidbody.isKinematic)
                return;
            // c.contacts.ForEach(x => Reports.Add(new ContactReport(this, c)));
            c.contacts.ForEach(x => ReplayManager.PushContactReport(new ContactReport(this, c)));
        }
    }

    // TODO: Setup for json serialization?
    public struct ContactReport {
        public float TimeStamp;
        public string RigidbodyA;
        public string RigidbodyB;
        public Vector3 RelativeVelocity;
        public Vector3 Impulse;
        public ContactPoint[] Points;

        public ContactReport(ContactRecorder r, Collision c) {
            // TODO: In newer versions of Unity they change these name for some reason. Hi future developer
            RigidbodyA = r.gameObject.name;
            RigidbodyB = c.rigidbody == null ? string.Empty : c.rigidbody.name;
            Points = c.contacts;
            TimeStamp = Time.realtimeSinceStartup - ReplayManager.DesyncTime;
            RelativeVelocity = c.relativeVelocity;
            Impulse = c.impulse;
        }
    }

    public class PhysicsFreezeChangeEvent: IEvent {
        public bool IsFrozen;
    }

    public interface IPhysicsOverridable {
        public bool isFrozen();
        public void Freeze();
        public void Unfreeze();
        public List<Rigidbody> GetAllRigidbodies();
        public GameObject GetRootGameObject();
    }
}