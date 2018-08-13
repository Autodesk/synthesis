﻿using Synthesis.Camera;
using Synthesis.FSM;
using Synthesis.Sensors;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
    public class SensorToolbarState : State
    {
        RobotCameraGUI robotCameraGUI;
        SensorManagerGUI sensorManagerGUI;

        GameObject canvas;
        GameObject tabs;
        GameObject sensorToolbar;

        GameObject helpMenu;
        GameObject overlay;
        Text helpBodyText;

        Dropdown ultrasonicDropdown;
        Dropdown beamBreakerDropdown;
        Dropdown gyroDropdown;

        int numUltrasonics = 0;
        int numBeamBreakers = 0;
        int numGyros = 0;

        public override void Start()
        {
            robotCameraGUI = StateMachine.SceneGlobal.GetComponent<RobotCameraGUI>();
            sensorManagerGUI = StateMachine.SceneGlobal.GetComponent<SensorManagerGUI>();

            canvas = GameObject.Find("Canvas");
            tabs = Auxiliary.FindObject(canvas, "Tabs");
            sensorToolbar = Auxiliary.FindObject(canvas, "SensorToolbar");

            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
            helpBodyText = Auxiliary.FindObject(canvas, "BodyText").GetComponent<Text>();

            ultrasonicDropdown = Auxiliary.FindObject(sensorToolbar, "UltrasonicDropdown").GetComponent<Dropdown>();
            beamBreakerDropdown = Auxiliary.FindObject(sensorToolbar, "BeamBreakDropdown").GetComponent<Dropdown>();
            gyroDropdown = Auxiliary.FindObject(sensorToolbar, "GyroDropdown").GetComponent<Dropdown>();

            //initialize dropdowns
            UpdateSensorDropdown(ultrasonicDropdown, sensorManagerGUI.sensorManager.ultrasonicList);
            UpdateSensorDropdown(beamBreakerDropdown, sensorManagerGUI.sensorManager.beamBreakerList);
            UpdateSensorDropdown(gyroDropdown, sensorManagerGUI.sensorManager.gyroList);

            numUltrasonics = sensorManagerGUI.sensorManager.ultrasonicList.Count();
            numBeamBreakers = sensorManagerGUI.sensorManager.beamBreakerList.Count();
            numGyros = sensorManagerGUI.sensorManager.gyroList.Count();

            UpdateOutputButton();

            Button helpButton = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(CloseHelpMenu);
        }

        private void UpdateOutputButton()
        {
            if (sensorManagerGUI.sensorManager.GetActiveSensors().Count() == 0 && Auxiliary.FindObject(sensorToolbar, "ShowOutputsButton").activeSelf)
            {
                Auxiliary.FindObject(sensorToolbar, "ShowOutputsButton").SetActive(false);
                sensorToolbar.transform.Find("HelpButton").Translate(new Vector3(-100, 0, 0));
            }
            else if (sensorManagerGUI.sensorManager.GetActiveSensors().Count() > 0 && !Auxiliary.FindObject(sensorToolbar, "ShowOutputsButton").activeSelf)
            {
                Auxiliary.FindObject(sensorToolbar, "ShowOutputsButton").SetActive(true);
                sensorToolbar.transform.Find("HelpButton").Translate(new Vector3(100, 0, 0));
            }
        }

        /// <summary>
        /// Refresh the options in a specified sensor dropdown (usually after a sensor has been added or deleted)
        /// </summary>
        /// <param name="dropdown"></param>
        /// <param name="sensors"></param>
        private void UpdateSensorDropdown(Dropdown dropdown, IEnumerable<GameObject> sensors)
        {
            dropdown.ClearOptions();

            //Add blank option at the top (that isn't visible). Don't delete this or it won't work
            dropdown.AddOptions(new List<string> { "" });

            //Add options for each of the existing sensors
            if (sensors != null) dropdown.AddOptions((from sensor in sensors select sensor.name).ToList());

            //Add option at the end for add new sensor
            dropdown.AddOptions(new List<Dropdown.OptionData> { new Dropdown.OptionData("Add",
                Sprite.Create(Resources.Load("Images/New Icons/Add") as Texture2D, new Rect(0, 0, 24, 24), Vector2.zero)) });

            ultrasonicDropdown.RefreshShownValue();

            UpdateOutputButton();
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

                if (PlayerPrefs.GetInt("analytics") == 1)
                {
                    Analytics.CustomEvent("Added Ultrasonic Sensor", new Dictionary<string, object> //for analytics tracking
                    {
                    });
                }
            }
            else //Edit one of the existing sensors
            {
                sensorManagerGUI.SetUltrasonicAsCurrent(i - 1);
                sensorManagerGUI.StartConfiguration();
            }
            ultrasonicDropdown.value = 0;
        }

        public void OnBeamBreakDropdownClicked(int i)
        {
            if (i == 0) return;
            if (i - 1 == numBeamBreakers) //Add button
            {
                List<GameObject> updatedList = sensorManagerGUI.AddBeamBreaker();
                UpdateSensorDropdown(beamBreakerDropdown, updatedList);
                numBeamBreakers++;

                if (PlayerPrefs.GetInt("analytics") == 1)
                {
                    Analytics.CustomEvent("Added Beam Breaker", new Dictionary<string, object> //for analytics tracking
                    {
                    });
                }
            }
            else //Edit one of the existing sensors
            {
                sensorManagerGUI.SetBeamBreakerAsCurrent(i - 1);
                sensorManagerGUI.StartConfiguration();
            }
            beamBreakerDropdown.value = 0;
        }

        public void OnGyroDropdownClicked(int i)
        {
            if (i == 0) return;
            if (i - 1 == numGyros) //Add button
            {
                List<GameObject> updatedList = sensorManagerGUI.AddGyro();
                UpdateSensorDropdown(gyroDropdown, updatedList);
                numGyros++;

                if (PlayerPrefs.GetInt("analytics") == 1)
                {
                    Analytics.CustomEvent("Added Gyro", new Dictionary<string, object> //for analytics tracking
                    {
                    });
                }
            }
            else //Edit one of the existing sensors
            {
                sensorManagerGUI.SetGyroAsCurrent(i - 1);
                sensorManagerGUI.StartConfiguration();
            }
            gyroDropdown.value = 0;
        }

        public void OnShowOutputsButtonPressed()
        {
            sensorManagerGUI.ToggleSensorOutput();
        }

        /// <summary>
        /// Update a specified dropdown after a sensor is deleted (called in SensorManagerGUI)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ultrasonicList"></param>
        /// <param name="beamBreakerList"></param>
        /// <param name="gyroList"></param>
        public void RemoveSensorFromDropdown(string type, List<GameObject> ultrasonicList, List<GameObject> beamBreakerList, List<GameObject> gyroList)
        {
            switch (type)
            {
                case "Ultrasonic":
                    numUltrasonics--;
                    UpdateSensorDropdown(ultrasonicDropdown, ultrasonicList);
                    break;
                case "Beam Break":
                    numBeamBreakers--;
                    UpdateSensorDropdown(beamBreakerDropdown, beamBreakerList);
                    break;
                case "Gyro":
                    numGyros--;
                    UpdateSensorDropdown(gyroDropdown, gyroList);
                    break;
            }
        }

        public void OnHelpButtonPressed()
        {
            helpMenu.SetActive(true);

            helpBodyText.GetComponent<Text>().text = "\n\nRobot Cameras: Configure cameras for your robot" +
                "\n\nSensor Dropdowns: CLICK on an existing sensor to configure OR" +
                "\nAdd: Adds a selected sensor to robot and configuring tools";

            Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text = "SensorToolbar";
            overlay.SetActive(true);
            tabs.transform.Translate(new Vector3(300, 0, 0));
            foreach (Transform t in sensorToolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(300, 0, 0));
                else t.gameObject.SetActive(false);
            }
        }

        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-300, 0, 0));
            foreach (Transform t in sensorToolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-300, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }
    }
}