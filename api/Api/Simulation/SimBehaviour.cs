using System;
using System.Collections.Generic;
using System.Linq;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.Simulation {
    public abstract class SimBehaviour {

        protected List<string> _reservedInput = new List<string>();
        public IReadOnlyCollection<string> ReservedInput => _reservedInput.AsReadOnly();

        private bool _enabled = false;
        public bool Enabled {
            get => _enabled;
            set => _enabled = value;
            //     if (_enabled != value) {
            //         _enabled = value;
            //         if (value) {
            //             if (GetType().FullName != typeof(SimBehaviour).FullName)
            //                 SimulationManager.Behaviours.Add(this);
            //                 // SimulationManager.OnBehaviourUpdate += this.Update;
            //         }
            //         else {
            //             if (GetType().FullName != typeof(SimBehaviour).FullName)
            //                 SimulationManager.Behaviours.Remove(this);
            //                 // SimulationManager.OnBehaviourUpdate -= this.Update;
            //         }
            //     }
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

        public void InitInputs(params (string key, Analog defaultInput)[] inputs) {
            inputs.ForEach(x => {
                _reservedInput.Add(x.key);
                InputManager.InputManager.AssignValueInput(x.key, x.defaultInput, true);
            });
        }

        ~SimBehaviour() {
            Enabled = false;
        }
        
        public abstract void Update();
    }
}