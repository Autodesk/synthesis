using System.Collections.Generic;
using SynthesisAPI.Controller;
using Mirabuf.Signal;

namespace SynthesisAPI.Simulation {
    public class SimObject {
        protected string _name;
        public string Name {
            get => _name;
        }

        protected ControllableState _state;
        public ControllableState State {
            get => _state;
        }

        protected bool _behavioursEnabled = true;
        public bool BehavioursEnabled {
            get => _behavioursEnabled;
            set {
                _behavioursEnabled = value;
                SimulationManager.Behaviours[_name].ForEach(b => {
                    b.Enabled = _behavioursEnabled;
                });
            }
        }

        public SimObject(string name, Signals signalLayout) {
            _name = name;
            _state = new ControllableState(signalLayout);
        }

        // This was Init. No idea why but it might need to be
        public SimObject(string name, ControllableState state) {
            _name = name;
            _state = state;
        }

        public virtual void Destroy() { }

        public List<(string key, string displayName)> GetAllReservedInputs() {
            var res = new List<(string key, string displayName)>();
            SimulationManager.Behaviours[_name].ForEach(x => x.ReservedInput.ForEach(y => res.Add(y)));
            return res;
        }

        public override int GetHashCode()
            => _name.GetHashCode() * 482901849;
    }
}