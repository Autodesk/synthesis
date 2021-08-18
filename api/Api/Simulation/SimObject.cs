using SynthesisAPI.Utilities;

namespace SynthesisAPI.Simulation {
    public class SimObject {
        private string _name;
        public string Name {
            get => _name;
        }
        private ControllableState _state;

        public void Init(string name, ControllableState state) {
            _name = name;
            _state = state;
        }
    }
}