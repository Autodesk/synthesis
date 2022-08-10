using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.Simulation {
    public static class SimulationManager {
        public delegate void SimObjectEvent(SimObject entity);

        public static event SimObjectEvent OnNewSimulationObject;
        public static event SimObjectEvent OnRemoveSimulationObject;
        
        internal static Dictionary<string, SimObject> _simObjects = new Dictionary<string, SimObject>();
        public static IReadOnlyDictionary<string, SimObject> SimulationObjects
            = new ReadOnlyDictionary<string, SimObject>(_simObjects);

        public delegate void UpdateDelegate();
        public static event UpdateDelegate OnDriverUpdate;
        public static event UpdateDelegate OnBehaviourUpdate;

        // TODO: Switch to using guids cuz all the signals have the same name
        public static Dictionary<string, List<Driver>>       Drivers    = new Dictionary<string, List<Driver>>();
        public static Dictionary<string, List<SimBehaviour>> Behaviours = new Dictionary<string, List<SimBehaviour>>();

        public static void Update() {
            Drivers.ForEach(x => x.Value.ForEach(y => y.Update()));
            if (OnDriverUpdate != null)
                OnDriverUpdate();
            Behaviours.ForEach(x => {
                if (_simObjects[x.Key].BehavioursEnabled)
                    x.Value.ForEach(y => y.Update());
            });
            if (OnBehaviourUpdate != null)
                OnBehaviourUpdate();
            // _drivers.ForEach(x => x.Update());
        }

        public static void AddDriver(string simObjectName, Driver d)
        {
            if (!Drivers.ContainsKey(simObjectName))
            {
                Drivers[simObjectName] = new List<Driver>();
            }
            Drivers[simObjectName].Add(d);
        }

        public static void AddBehaviour(string simObjectName, SimBehaviour d)
        {
            if (!Behaviours.ContainsKey(simObjectName))
            {
                Behaviours[simObjectName] = new List<SimBehaviour>();
            }
            Behaviours[simObjectName].Add(d);
        }

        public static void RemoveDriver(string simObjectName, Driver d)
        {
            if (Drivers.ContainsKey(simObjectName))
            {
                Drivers[simObjectName].RemoveAll(x => x == d);
            }
        }

        public static void RegisterSimObject(SimObject so) {
            if (_simObjects.ContainsKey(so.Name)) // Probably use some GUID
                throw new Exception("Name already exists");
            _simObjects[so.Name] = so;
            Drivers[so.Name] = new List<Driver>();
            Behaviours[so.Name] = new List<SimBehaviour>();

            if (OnNewSimulationObject != null)
                OnNewSimulationObject(so);
        }

        public static bool RemoveSimObject(SimObject so) {
            return RemoveSimObject(so.Name);
        }

        public static bool RemoveSimObject(string so) {
            bool exists = _simObjects.TryGetValue(so, out SimObject s);
            if (!exists)
                return false;
            Drivers.Remove(so);
            Behaviours.Remove(so);
            var res = _simObjects.Remove(so);
            if (res) {
                s.Destroy();
                if (OnRemoveSimulationObject != null) {
                    OnRemoveSimulationObject(s);
                }
            } else {
                Logger.Log("No sim object found by that name", LogLevel.Warning);
            }
            return res;
        }
    }
}