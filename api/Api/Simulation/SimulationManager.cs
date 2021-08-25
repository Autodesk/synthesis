using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SynthesisAPI.EnvironmentManager;

namespace SynthesisAPI.Simulation {
    public static class SimulationManager {
        public delegate void SimObjectEvent(SimObject entity);

        public static event SimObjectEvent OnNewSimulationObject;
        public static event SimObjectEvent OnRemoveSimulationObject;
        
        private static Dictionary<string, SimObject> _simulationObject = new Dictionary<string, SimObject>();
        public static IReadOnlyDictionary<string, SimObject> SimulationObjects
            = new ReadOnlyDictionary<string, SimObject>(_simulationObject);

        public delegate void UpdateDelegate();
        public static event UpdateDelegate OnDriverUpdate;
        public static event UpdateDelegate OnBehaviourUpdate;

        public static Dictionary<string, List<Driver>>       Drivers    = new Dictionary<string, List<Driver>>();
        public static Dictionary<string, List<SimBehaviour>> Behaviours = new Dictionary<string, List<SimBehaviour>>();

        public static void Update() {
            Drivers.ForEach(x => x.Value.ForEach(y => y.Update()));
            if (OnDriverUpdate != null)
                OnDriverUpdate();
            Behaviours.ForEach(x => x.Value.ForEach(y => y.Update()));
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


        public static void RegisterSimObject(SimObject so) {
            if (_simulationObject.ContainsKey(so.Name)) // Probably use some GUID
                throw new Exception("Name already exists");
            _simulationObject[so.Name] = so;
            Drivers[so.Name] = new List<Driver>();
            Behaviours[so.Name] = new List<SimBehaviour>();

            if (OnNewSimulationObject != null)
                OnNewSimulationObject(so);
        }

        public static bool RemoveSimObject(SimObject so) {
            Drivers.Remove(so.Name);
            Behaviours.Remove(so.Name);
            var res = _simulationObject.Remove(so.Name);
            if (res && OnRemoveSimulationObject != null)
                OnRemoveSimulationObject(so);
            return res;
        }
    }
}