namespace SynthesisAPI.Simulation {
    public abstract class Driver {
        protected string _name;
        public string Name {
            get => _name;
        }
        // What will the driver read to find out what to do
        protected string[] _inputs;
        // What will the driver write so others can know what has been done
        protected string[] _outputs;
        protected SimObject _simObject;

        public SimObject _SimObject {
            get => _simObject;
        }

        public ControllableState State {
            get => _simObject.State;
        }

        public Driver(string name, string[] inputs, string[] outputs, SimObject simObject) {
            _name = name;
            _inputs = inputs;
            _outputs = outputs;
            _simObject = simObject;

            // if (GetType().FullName != typeof(Driver).FullName) // Idk if this is necessary
            //     SimulationManager.OnDriverUpdate += this.Update;
        }

        ~Driver() {
            // if (GetType().FullName != typeof(Driver).FullName) // Idk if this is necessary
            //     SimulationManager.OnDriverUpdate -= this.Update;
        }

        public virtual void OnRemove() { }
        public abstract void Update();
        public virtual void FixedUpdate() { }

        public override int GetHashCode()
            => _inputs.GetHashCode() * 374124789
               + _outputs.GetHashCode() * 875920184
               + _simObject.GetHashCode() * 395018496;

        public override bool Equals(object obj) {
            if (ReferenceEquals(obj, null))
                return false;
            return obj.GetHashCode() == GetHashCode();
        }
    }
}