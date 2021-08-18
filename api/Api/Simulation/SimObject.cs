using SynthesisAPI.Utilities;

namespace SynthesisAPI.Simulation {
    public class SimObject {
        private string _name;
        public string Name {
            get => _name;
        }
        private ControllableState _state;

        public ControllableState State {
            get => _state;
        }

        // This was Init. No idea why but it might need to be
        public SimObject(string name, ControllableState state) {
            _name = name;
            _state = state;
        }

        public override int GetHashCode()
            => _name.GetHashCode() * 482901849;
    }
}