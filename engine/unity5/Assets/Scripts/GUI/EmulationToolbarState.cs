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
        GameObject canvas;
        GameObject emuDriverStation;

        public GameObject stopButton;
        public GameObject runButton;

        bool isRunCode = false;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            runButton = Auxiliary.FindObject(canvas, "StartRobotCodeButton");
            stopButton = Auxiliary.FindObject(canvas, "StopCodeButton");
        }


        public void OnSelectRobotCodeButtonPressed()
        {

        }
    }
}