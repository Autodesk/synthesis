using Synthesis.Camera;
using Synthesis.DriverPractice;
using Synthesis.FSM;
using Synthesis.Sensors;
using Synthesis.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GUI
{
    public class SensorToolbarState : State
    {
        RobotCameraGUI robotCameraGUI;
        SensorManagerGUI sensorManagerGUI;

        public override void Start()
        {
            robotCameraGUI = StateMachine.SceneGlobal.GetComponent<RobotCameraGUI>();
            sensorManagerGUI = StateMachine.SceneGlobal.GetComponent<SensorManagerGUI>();
        }

        /// <summary>
        /// Toggles the state of the camera button in toolbar when clicked
        /// </summary>
        public void OnRobotCameraButtonPressed()
        {
            robotCameraGUI.ToggleCameraWindow();
        }

        /// <summary>
        /// Toggles the state of the sensor button in the toolbar when clicked
        /// </summary>
        //public void OnSensorButtonPressed()
        //{
        //    sensorManagerGUI.ToggleSensorOption();
        //}

        public void OnUltrasonicButtonPressed()
        {
            sensorManagerGUI.AddUltrasonic();
        }

        public void OnBeamBreakButtonPressed()
        {
            sensorManagerGUI.AddBeamBreaker();
        }

        public void OnGyroButtonPressed()
        {
            sensorManagerGUI.AddGyro();
        }

    }
}