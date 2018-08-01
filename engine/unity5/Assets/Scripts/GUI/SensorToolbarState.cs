using Synthesis.Camera;
using Synthesis.DriverPractice;
using Synthesis.FSM;
using Synthesis.Sensors;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
    public class SensorToolbarState : State
    {
        RobotCameraGUI robotCameraGUI;
        SensorManagerGUI sensorManagerGUI;

        GameObject canvas;
        GameObject sensorToolbar;
        //GameObject dropdownItem;

        Dropdown ultrasonicDropdown;

        int numUltrasonics = 0;

        public override void Start()
        {
            robotCameraGUI = StateMachine.SceneGlobal.GetComponent<RobotCameraGUI>();
            sensorManagerGUI = StateMachine.SceneGlobal.GetComponent<SensorManagerGUI>();

            canvas = GameObject.Find("Canvas");
            sensorToolbar = Auxiliary.FindObject(canvas, "SensorToolbar");
            //dropdownItem = Auxiliary.FindObject(sensorToolbar, "ItemTemplate");

            ultrasonicDropdown = Auxiliary.FindObject(sensorToolbar, "UltrasonicDropdown").GetComponent<Dropdown>();

            //initialize dropdowns
            UpdateSensorDropdown(ultrasonicDropdown, null);
        }

        public void UpdateSensorDropdown(Dropdown dropdown, IEnumerable<GameObject> sensors)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string> { "" });

            //Add options for each of the existing sensors
            if (sensors != null) dropdown.AddOptions((from sensor in sensors select sensor.name).ToList());

            //Add option at the end for add new sensor
            dropdown.AddOptions(new List<Dropdown.OptionData> { new Dropdown.OptionData("Add",
                Sprite.Create(Resources.Load("Images/New Icons/Add") as Texture2D, new Rect(0, 0, 24, 24), Vector2.zero)) });

            ultrasonicDropdown.RefreshShownValue();
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

        public void OnUltrasonicDropdownClicked(int i)
        {
            if (i == 0) return;
            if (i - 1 == numUltrasonics) //Add button
            {
                List<GameObject> updatedList = sensorManagerGUI.AddUltrasonic();
                UpdateSensorDropdown(ultrasonicDropdown, updatedList);
                numUltrasonics++;
            }
            else //Edit one of the existing sensors
            {
                sensorManagerGUI.SetUltrasonicAsCurrent(i - 1);
                sensorManagerGUI.StartConfiguration();
            }
            ultrasonicDropdown.value = 0;
        }

        public void OnBeamBreakButtonPressed()
        {
            sensorManagerGUI.AddBeamBreaker();
        }

        public void OnGyroButtonPressed()
        {
            sensorManagerGUI.AddGyro();
        }

        public void OnShowOutputsButtonPressed()
        {
            sensorManagerGUI.ToggleSensorOutput();
        }

    }
}