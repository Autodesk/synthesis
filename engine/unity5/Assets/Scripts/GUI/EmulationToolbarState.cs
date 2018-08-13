using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Input;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
    public class EmulationToolbarState : State
    {
        EmulationDriverStation emulationDriverStation;

        public override void Start()
        {
            emulationDriverStation = StateMachine.SceneGlobal.GetComponent<EmulationDriverStation>();
        }

        public void OnSelectRobotCodeButtonPressed()
        {
            SSHClient.SCPFileSender();
        }

        public void OnDriverStationButtonPressed()
        {
            emulationDriverStation.OpenDriverStation();
        }

        public void OnStartRobotCodeButtonPressed()
        {
            emulationDriverStation.ToggleRobotCodeButton();
            SSHClient.StartRobotCode();
            //Serialization.RestartThreads("10.140.148.66");
        }
    }
}