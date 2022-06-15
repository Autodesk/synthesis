using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using SynthesisAPI.EventBus;

namespace Synthesis.Physics {
    /// <summary>
    /// Controls physics within unity
    /// </summary>
    public static class PhysicsManager {
        private static Dictionary<int, IPhysicsOverridable> _physObjects = new Dictionary<int, IPhysicsOverridable>();

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
            return true;
        }

        public static bool Unregister<T>(T overridable) where T: class, IPhysicsOverridable {
            if (!_physObjects.ContainsKey(overridable.GetHashCode()))
                return false;
            _physObjects.Remove(overridable.GetHashCode());
            return true;
        }

        public static List<IPhysicsOverridable> GetAllOverridable()
            => new List<IPhysicsOverridable>(_physObjects.Values);
    }

    public class PhysicsFreezeChangeEvent: IEvent {
        public bool IsFrozen;
    }

    public interface IPhysicsOverridable {
        public bool isFrozen();
        public void Freeze();
        public void Unfreeze();
        public List<Rigidbody> GetAllRigidbodies();
    }
}