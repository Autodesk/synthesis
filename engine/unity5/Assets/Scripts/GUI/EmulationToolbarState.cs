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

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            emuDriverStation = Auxiliary.FindObject(canvas, "EmulationDriverStation");
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

        public void OnStartRobotCodeButtonPressed()
        {

        }
    }
}