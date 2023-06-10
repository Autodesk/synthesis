using System;
using System.Collections.Generic;
using System.Linq;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.Simulation {
    public abstract class SimBehaviour {

        protected List<(string key, string displayName)> _reservedInput = new List<(string key, string displayName)>();
        public IReadOnlyCollection<(string key, string displayName)> ReservedInput => _reservedInput.AsReadOnly();

        private bool _enabled = false;
        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled != value) {
                    _enabled = value;
                    if (_enabled)
                        OnEnable();
                    else
                        OnDisable();
                }
            }
        }

        public string SimObjectId { get; protected set; }
        
        /// <summary>
        /// Constructor for a SimObject
        /// </summary>
        /// <param name="simObjectId">ID of the SimObject within the SimulationManager</param>
        /// <param name="inputs">A list of reserved inputs by the behaviour, along with a input to assign to each</param>
        public SimBehaviour(string simObjectId) {
            Enabled = true;
            SimObjectId = simObjectId;
        }

        public void InitInputs(params (string key, string displayName, Analog defaultInput)[] inputs) {
            inputs.ForEach(x => {
                _reservedInput.Add((x.key, x.displayName));
                InputManager.InputManager.AssignValueInput(x.key, x.defaultInput, true);
            });
        }

        ~SimBehaviour() {
            Enabled = false;
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        public abstract void Update();
    }
}