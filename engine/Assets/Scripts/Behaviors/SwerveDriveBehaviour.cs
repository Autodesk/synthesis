using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using SynthesisAPI.Simulation;
using UnityEngine;

namespace Synthesis {
    public class SwerveDriveBehaviour : SimBehaviour {

        private string[] _azimuthSignals;
        private string[] _driveSignals;

        public SwerveDriveBehaviour(string simObjectId, string[] azimuthSignals, string[] driveSignals) : base(simObjectId) {
            _azimuthSignals = azimuthSignals;
            _driveSignals = driveSignals;

            _azimuthSignals.ForEach(x => SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[$"{x}_mode"].Value = Value.ForString("Position"));
        }

        public override void Update() {
            for (int i = 0; i < _azimuthSignals.Length; i++) {
                SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[_azimuthSignals[i]].Value = Value.ForNumber(90 * i);
            }
        }
    }
}
