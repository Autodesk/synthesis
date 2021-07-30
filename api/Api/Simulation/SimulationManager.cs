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

        public static List<Driver> Drivers = new List<Driver>();
        public static List<SimBehaviour> Behaviours = new List<SimBehaviour>();

        public static void Update() {
            Drivers.ForEach(x => x.Update());
            if (OnDriverUpdate != null)
                OnDriverUpdate();
            Behaviours.ForEach(x => x.Update());
            if (OnBehaviourUpdate != null)
                OnBehaviourUpdate();
            // _drivers.ForEach(x => x.Update());
        }

        public static void RegisterSimObject(SimObject so) {
            if (_simulationObject.ContainsKey(so.Name)) // Probably use some GUID
                throw new Exception("Name already exists");
            _simulationObject[so.Name] = so;

            if (OnNewSimulationObject != null)
                OnNewSimulationObject(so);
        }

        public static bool RemoveSimObject(SimObject so) {
            var res = _simulationObject.Remove(so.Name);
            if (res && OnRemoveSimulationObject != null)
                OnRemoveSimulationObject(so);
            return res;
        }
    }
}