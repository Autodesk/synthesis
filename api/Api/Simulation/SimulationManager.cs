using System;
using System.Linq;
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
        public static event UpdateDelegate OnDriverFixedUpdate;
        public static event UpdateDelegate OnBehaviourFixedUpdate;


        // TODO: Switch to using guids cuz all the signals have the same name
        public static Dictionary<string, LinkedList<Driver>>       Drivers    = new Dictionary<string, LinkedList<Driver>>();
        public static Dictionary<string, LinkedList<SimBehaviour>> Behaviours = new Dictionary<string, LinkedList<SimBehaviour>>();

        public static void Update() {
            Drivers.ForEach(x => x.Value.ForEach(y => y.Update()));
            if (OnDriverUpdate != null)
                OnDriverUpdate();
            Behaviours.ForEach(x => {
                if (_simObjects[x.Key].BehavioursEnabled)
                    x.Value.Where(y => y.Enabled).ForEach(y => y.Update());
            });
            if (OnBehaviourUpdate != null)
                OnBehaviourUpdate();
            // _drivers.ForEach(x => x.Update());
        }

        public static void FixedUpdate() {
            Drivers.ForEach(x => x.Value.ForEach(y => y.FixedUpdate()));
            if (OnDriverFixedUpdate != null)
                OnDriverFixedUpdate();
            Behaviours.ForEach(x => {
                if (_simObjects[x.Key].BehavioursEnabled)
                    x.Value.Where(y => y.Enabled).ForEach(y => y.FixedUpdate());
            });
            if (OnBehaviourFixedUpdate != null)
                OnBehaviourFixedUpdate();
            // _drivers.ForEach(x => x.Update());
        }

        public static void AddDriver(string simObjectName, Driver d)
        {
            if (!Drivers.ContainsKey(simObjectName))
            {
                Drivers[simObjectName] = new LinkedList<Driver>();
            }
            Drivers[simObjectName].AddLast(d);
        }

        public static void AddBehaviour(string simObjectName, SimBehaviour d)
        {
            if (!Behaviours.ContainsKey(simObjectName))
            {
                Behaviours[simObjectName] = new LinkedList<SimBehaviour>();
            }
            Behaviours[simObjectName].AddLast(d);
        }

        public static bool RemoveDriver(string simObjectName, Driver d)
        {
            if (!Drivers.ContainsKey(simObjectName))
                return false;

            var list = Drivers[simObjectName];
            bool removed = false;
            var cursor = list.First;
            while (!removed && cursor != null) {
                if (cursor.Value.Equals(d)) {
                    cursor.Value.OnRemove();
                    list.Remove(cursor);
                    removed = true;
                }
                cursor = cursor.Next;
            }

            return removed;
        }

        public static bool RemoveBehaviour(string simObjectName, SimBehaviour b) {
            if (!Behaviours.ContainsKey(simObjectName))
                return false;

            var list = Behaviours[simObjectName];
            bool removed = false;
            var cursor = list.First;
            while (!removed && cursor != null) {
                if (cursor.Value.Equals(b)) {
                    cursor.Value.OnRemove();
                    list.Remove(cursor);
                    removed = true;
                }
                cursor = cursor.Next;
            }

            return removed;
        }

        public static void RegisterSimObject(SimObject so) {
            if (_simObjects.ContainsKey(so.Name)) // Probably use some GUID
                throw new Exception("Name already exists");
            _simObjects[so.Name] = so;
            Drivers[so.Name] = new LinkedList<Driver>();
            Behaviours[so.Name] = new LinkedList<SimBehaviour>();

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