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

namespace Assets.Scripts.GUI
{
    public class EmulationToolbarState : State
    {
        GameObject canvas;
        GameObject emuDriverStation;
        public GameObject stopButton;
        public GameObject runButton;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            emuDriverStation = Auxiliary.FindObject(canvas, "EmulationDriverStation");
            runButton = Auxiliary.FindObject(canvas, "RunRobotCodeButton");
            stopButton = Auxiliary.FindObject(canvas, "StopCodeButton");
        }


        public void OnSelectRobotCodeButtonPressed()
        {

        }

        public void OnDriverStationButtonPressed()
        {
            if (emuDriverStation.activeSelf == true)
            {
                emuDriverStation.SetActive(false);
                InputControl.freeze = false;
            }
            else
            {
                emuDriverStation.SetActive(true);
                InputControl.freeze = true;
            }
        }

        public void OnRunRobotCodeButtonPressed()
        {
            if (stopButton.activeSelf)
            {
                stopButton.SetActive(false);
                runButton.SetActive(true);
            }
            else
            {
                runButton.SetActive(false);
                stopButton.SetActive(true);
            }
        }

    }
}