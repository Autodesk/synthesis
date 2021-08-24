using System;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.Simulation {
    public abstract class SimBehaviour {

        private bool _enabled = false;
        public bool Enabled {
            get => _enabled;
            set {
                _enabled = value;
                if (_enabled != value) {
                    _enabled = value;
                    if (value) {
                        if (GetType().FullName != typeof(SimBehaviour).FullName)
                            SimulationManager.Behaviours.Add(this);
                            // SimulationManager.OnBehaviourUpdate += this.Update;
                    }
                    else {
                        if (GetType().FullName != typeof(SimBehaviour).FullName)
                            SimulationManager.Behaviours.Remove(this);
                            // SimulationManager.OnBehaviourUpdate -= this.Update;
                    }
                }
            }
        }

        public String SimObjectId { get; protected set; }
        
        public SimBehaviour(string simObjectId) {
            Enabled = true;
            SimObjectId = simObjectId;
        }

        ~SimBehaviour() {
            Enabled = false;
        }
        
        public abstract void Update();
    }
}