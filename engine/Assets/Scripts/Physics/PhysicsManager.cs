using Synthesis.Replay;
using Synthesis.Runtime;
using SynthesisAPI.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable

namespace Synthesis.Physics {
    /// <summary>
    /// Controls physics within unity
    /// </summary>
    public static class PhysicsManager {
        private static Dictionary<int, IPhysicsOverridable> _physObjects = new Dictionary<int, IPhysicsOverridable>();
        private static Dictionary<int, List<ContactRecorder>> _contactRecorders =
            new Dictionary<int, List<ContactRecorder>>();

        private static Dictionary<Rigidbody, RigidbodyFrameData> _storedValues =
            new Dictionary<Rigidbody, RigidbodyFrameData>();

        private static bool _storeOnFreeze = true;

        private static bool _isFrozen;
        private static int _frozenCounter;
        public static bool IsFrozen {
            get => _isFrozen;
            set {
                if (value) {
                    ++_frozenCounter;
                } else {
                    --_frozenCounter;
                }

                if (_frozenCounter < 0) {
                    // My guy
                    _frozenCounter = 0;
                }

                var shouldFreeze = _frozenCounter != 0;
                if (shouldFreeze != _isFrozen) {
                    _isFrozen = shouldFreeze;

                    if (_isFrozen) {
                        SimulationRunner.RemoveContext(SimulationRunner.RUNNING_SIM_CONTEXT);
                        SimulationRunner.AddContext(SimulationRunner.PAUSED_SIM_CONTEXT);
                        _physObjects.ForEach(x => {
                            if (_storeOnFreeze) {
                                x.Value.GetAllRigidbodies().ForEach(rb => {
                                    var data          = new RigidbodyFrameData { Velocity = rb.velocity,
                                        AngularVelocity                          = rb.angularVelocity };
                                    _storedValues[rb] = data;
                                });
                            }
                            x.Value.Freeze();
                        });
                    } else {
                        SimulationRunner.RemoveContext(SimulationRunner.PAUSED_SIM_CONTEXT);
                        SimulationRunner.AddContext(SimulationRunner.RUNNING_SIM_CONTEXT);

                        _physObjects.ForEach(x => {
                            x.Value.Unfreeze();
                            if (_storeOnFreeze) {
                                _storedValues.ForEach((rb, f) => {
                                    try {
                                        rb.velocity        = f.Velocity;
                                        rb.angularVelocity = f.AngularVelocity;
                                    } catch (Exception e) {
                                        // Ignored
                                    }
                                });
                            }

                            _storedValues.Clear();
                            _storeOnFreeze = true;
                        });
                    }

                    EventBus.Push(new PhysicsFreezeChangeEvent { IsFrozen = _isFrozen });
                }
            }
        }

        public static void FixedUpdate() {
            RobotSimObject.SpawnedRobots.ForEach(x => { x.UpdateWheels(); });
        }

        public static bool Register<T>(T overridable)
            where T : class, IPhysicsOverridable {
            if (_physObjects.ContainsKey(overridable.GetHashCode())) {
                return false;
            }

            _physObjects[overridable.GetHashCode()] = overridable;
            if (_isFrozen) {
                overridable.Freeze();
            } else {
                overridable.Unfreeze();
            }

            AddContactRecorders(overridable);
            return true;
        }

        public static void AddContactRecorders<T>(T overridable)
            where T : class, IPhysicsOverridable {
            List<ContactRecorder> recorders              = new List<ContactRecorder>();
            _contactRecorders[overridable.GetHashCode()] = recorders;
        }

        public static bool Unregister<T>(T overridable)
            where T : class, IPhysicsOverridable {
            if (!_physObjects.ContainsKey(overridable.GetHashCode())) {
                return false;
            }

            _physObjects.Remove(overridable.GetHashCode());
            return true;
        }

        public static void DisableLoadFromStoredDataOnce() {
            _storeOnFreeze = false;
            _storedValues.Clear();
        }

        public static List<IPhysicsOverridable> GetAllOverridable() => new List<IPhysicsOverridable>(
            _physObjects.Values);

        public static List<ContactRecorder>? GetContactRecorders<T>(T overridable)
            where T : class, IPhysicsOverridable {
            if (!_contactRecorders.ContainsKey(overridable.GetHashCode())) {
                return null;
            }

            return _contactRecorders[overridable.GetHashCode()];
        }

        public static void Reset() {
            _physObjects      = new Dictionary<int, IPhysicsOverridable>();
            _contactRecorders = new Dictionary<int, List<ContactRecorder>>();
            _frozenCounter    = 0;
            _isFrozen         = false;
        }
    }

    public class ContactRecorder : MonoBehaviour {
        public void OnCollisionEnter(Collision c) {
            if (GetComponent<Rigidbody>().isKinematic && c.rigidbody.isKinematic) {
                return;
            }

            ReplayManager.PushContactReport(new ContactReport(this, c));
        }
    }

    // TODO: Setup for json serialization?
    public struct ContactReport {
        public float TimeStamp;
        public string RigidbodyA;
        public string RigidbodyB;
        public Vector3 RelativeVelocity;
        public Vector3 Impulse;
        public Vector3 point;
        public Vector3 normal;
        public float seperation;

        public ContactReport(ContactRecorder r, Collision c) {
            // TODO: In newer versions of Unity they change these name for some reason. Hi future developer
            RigidbodyA       = r.gameObject.name;
            RigidbodyB       = c.rigidbody == null ? string.Empty : c.rigidbody.name;
            TimeStamp        = Time.realtimeSinceStartup - ReplayManager.DesyncTime;
            RelativeVelocity = c.relativeVelocity;
            Impulse          = c.impulse;

            // Averages contact points
            Vector3 averageContactPoint = Vector3.zero;
            Vector3 averageNormal       = Vector3.zero;
            float averageSeperation     = 0;
            c.contacts.ForEach(x => {
                averageContactPoint += x.point;
                averageNormal += x.normal;
                averageSeperation += x.separation;
            });

            point      = averageContactPoint / c.contactCount;
            normal     = Vector3.Normalize(averageNormal);
            seperation = averageSeperation / c.contactCount;
        }
    }

    public class PhysicsFreezeChangeEvent : IEvent {
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
