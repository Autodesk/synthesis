using SynthesisAPI.Utilities;
using System.Collections.Generic;

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

        // This was Init. No idea why but it might need to be
        public SimObject(string name, ControllableState state)
        {
            _name = name;
            _state = state;
        }

        public virtual void Destroy() { }

        public List<string> GetAllReservedInputs() {
            var res = new List<string>();
            SimulationManager.Behaviours[_name].ForEach(x => x.ReservedInput.ForEach(y => res.Add(y)));
            return res;
        }

        public override int GetHashCode()
            => _name.GetHashCode() * 482901849;
    }
}