﻿using Synthesis.FSM;
using Synthesis.Input;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.GUI
{
    class EmulationDriverStation : MonoBehaviour
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

        bool isRobotDisabled = false;
        bool isActiveState;
        int teamStation;

        GameObject canvas;
        InputField gameDataInput;

        private void Start()
        {
            canvas = GameObject.Find("Canvas");
            gameDataInput = Auxiliary.FindObject(canvas, "InputField").GetComponent<InputField>();
            GameData();
        }

        private void Update()
        {
            
        }

        public void RobotState(string theState)
        {
            switch (theState)
            {
                case "teleop":
                    state = DriveState.Teleop;
                    GameObject.Find("Teleop").GetComponent<Button>().Select();
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
                case 0:
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

        /// <summary>
        /// A game specific message specified by the user
        /// </summary>
        public void GameData()
        {
            gameDataInput = Auxiliary.FindObject(canvas, "InputField").GetComponent<InputField>();
            gameDataInput.onValueChanged.AddListener(delegate { Debug.Log(gameDataInput.text.ToString()); });
        }
    }
}

