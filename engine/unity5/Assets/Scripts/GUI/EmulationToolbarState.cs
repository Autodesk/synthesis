using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GUI
{
    public class EmulationToolbarState : State
    {
        EmulationDriverStation emuDriverStation;

        public override void Start()
        {

        }

        public void OnSelectRobotCodeButtonPressed()
        {

        }

        public void OnDriverStationButtonPressed()
        {
            emuDriverStation.ToggleEmuDriverStation();
        }

        public void OnStartRobotCodeButtonPressed()
        {

        }
    }
}