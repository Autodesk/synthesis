﻿using Synthesis.FSM;
using Synthesis.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.GUI
{
    class EmulationDriverStation : LinkedMonoBehaviour<MainState>
    {
        enum DriveState
        {
            Auto,
            Teleop,
            Test,
        };

        enum AllianceStation
        {
            Red1,
            Red2,
            Red3,
            Blue1,
            Blue2,
            Blue3,
        };

        DriveState state;
        AllianceStation allianceStation;

        bool isRobotDisabled;
        bool isActiveState;

        public void RobotState(string theState)
        {
            switch (theState)
            {
                case "teleop":
                    state = DriveState.Teleop;
                    break;
                case "auto":
                    state = DriveState.Auto;
                    break;
                case "test":
                    state = DriveState.Test;
                    break;
                default:
                    state = DriveState.Teleop;
                    break;
            }
        }

        public void RobotEnabled()
        {
            isRobotDisabled = false;
        }

        public void RobotDisabled()
        {
            isRobotDisabled = true;
        }

        public void TeamStation(int teamStation)
        {
            switch (teamStation)
            {
                case:
                    allianceStation = AllianceStation.Red1;
                    Debug.Log(allianceStation);
                    break;
                case 1:
                    allianceStation = AllianceStation.Red2;
                    Debug.Log(allianceStation);
                    break;
                case 2:
                    allianceStation = AllianceStation.Red3;
                    Debug.Log(allianceStation);
                    break;
                case 3:
                    allianceStation = AllianceStation.Blue1;
                    Debug.Log(allianceStation);
                    break;
                case 4:
                    allianceStation = AllianceStation.Blue2;
                    Debug.Log(allianceStation);
                    break;
                case 5:
                    allianceStation = AllianceStation.Blue3;
                    Debug.Log(allianceStation);
                    break;
                default:
                    allianceStation = AllianceStation.Red1;
                    break;
            }
        }
    }
}

