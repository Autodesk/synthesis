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
//using UnityEngine.EventSystems;

namespace Assets.Scripts.GUI
{
    public class SensorToolbarState : State
    {
        RobotCameraGUI robotCameraGUI;
        SensorManagerGUI sensorManagerGUI;

        GameObject canvas;
        GameObject sensorToolbar;
        //GameObject dropdownItem;

        //Dropdown ultrasonicDropdown;

        int numUltrasonics = 0;

        //GameObject gamepieceDropdownButton;
        //GameObject gamepieceDropdownExtension;
        //List<GameObject> gamepieceDropdownElements;
        //GameObject gamepieceDropdownPrefab;
        //Transform dropdownLocation;
        //bool dropdown = false;
        //bool buffer = false;

        public override void Start()
        {
            robotCameraGUI = StateMachine.SceneGlobal.GetComponent<RobotCameraGUI>();
            sensorManagerGUI = StateMachine.SceneGlobal.GetComponent<SensorManagerGUI>();

            canvas = GameObject.Find("Canvas");
            sensorToolbar = Auxiliary.FindObject(canvas, "SensorToolbar");
            //dropdownItem = Auxiliary.FindObject(sensorToolbar, "ItemTemplate");

            //ultrasonicDropdown = Auxiliary.FindObject(sensorToolbar, "UltrasonicDropdown").GetComponent<Dropdown>();

            //initialize dropdowns
            //UpdateSensorDropdown(ultrasonicDropdown, null);

            //gamepieceDropdownButton = Auxiliary.FindObject(dpmToolbar, "GamepieceDropdownButton");
            //gamepieceDropdownExtension = Auxiliary.FindObject(gamepieceDropdownButton, "Scroll View");
            //gamepieceDropdownExtension.SetActive(false);
            //gamepieceDropdownPrefab = Resources.Load("Prefabs/GamepieceDropdownElement") as GameObject; //one element
            //dropdownLocation = Auxiliary.FindObject(gamepieceDropdownButton, "DropdownLocation").transform;
        }

        public void OnTestDropdownClicked(int mode)
        {
            switch (mode)
            {
                //case 1:
                //    camera.SwitchCameraState(new DynamicCamera.DriverStationState(camera));
                //    DynamicCamera.ControlEnabled = true;
                //    break;
                //case 2:
                //    camera.SwitchCameraState(new DynamicCamera.OrbitState(camera));
                //    DynamicCamera.ControlEnabled = true;
                //    break;
                //case 3:
                //    camera.SwitchCameraState(new DynamicCamera.FreeroamState(camera));
                //    DynamicCamera.ControlEnabled = true;
                //    break;
                //case 4:
                //    camera.SwitchCameraState(new DynamicCamera.OverviewState(camera));
                //    DynamicCamera.ControlEnabled = true;
                //    break;
            }
        }

        //public void UpdateSensorDropdown(Dropdown dropdown, IEnumerable<GameObject> sensors)
        //{
        //dropdown.ClearOptions();

        ////Add options for each of the existing sensors
        ////var evens = from num in numbers where num % 2 == 0 select num;
        //if (sensors != null) dropdown.AddOptions((from sensor in sensors select sensor.name).ToList());

        ////Add option at the end for add new sensor
        //dropdown.AddOptions(new List<string> { "Add" });

        //ultrasonicDropdown.RefreshShownValue();

        //    if (dpmRobot == null) dpmRobot = mainState.ActiveRobot.GetDriverPractice();
        //    if (dropdown && buffer)
        //        if (Input.GetMouseButtonUp(0))
        //        {
        //            dropdown = false;
        //            buffer = false;
        //            HideGamepieceDropdown();
        //        }
        //    if (!buffer && dropdown)
        //        if (Input.GetMouseButtonDown(0))
        //        {
        //            buffer = true;
        //        }
        //}

        //public void OnGamepieceDropdownButtonPressed()
        //{
        //    HideGamepieceDropdown();
        //    if (FieldDataHandler.gamepieces.Count > 1)
        //    {
        //        dropdown = true;
        //        for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
        //        {
        //            int id = i;

        //            if (id != gamepieceIndex)
        //            {
        //                GameObject gamepieceDropdownElement = GameObject.Instantiate(gamepieceDropdownPrefab);
        //                gamepieceDropdownElement.name = "Gamepiece " + id.ToString() + ": " + FieldDataHandler.gamepieces[id].name;
        //                gamepieceDropdownElement.transform.parent = dropdownLocation;

        //                Auxiliary.FindObject(gamepieceDropdownElement, "Name").GetComponent<Text>().text = FieldDataHandler.gamepieces[id].name;

        //                Button change = Auxiliary.FindObject(gamepieceDropdownElement, "Change").GetComponent<Button>();
        //                change.onClick.AddListener(delegate { gamepieceIndex = id; SetGamepieceDropdownName(); HideGamepieceDropdown(); dropdown = false; buffer = false; });

        //                gamepieceDropdownElements.Add(gamepieceDropdownElement);
        //            }
        //        }
        //        gamepieceDropdownExtension.SetActive(true);
        //    }
        //}

        //private void HideGamepieceDropdown()
        //{
        //    if (gamepieceDropdownElements == null)
        //        gamepieceDropdownElements = new List<GameObject>();

        //    while (gamepieceDropdownElements.Count > 0)
        //    {
        //        GameObject.Destroy(gamepieceDropdownElements[0]);
        //        gamepieceDropdownElements.RemoveAt(0);
        //    }
        //    gamepieceDropdownExtension.SetActive(false);
        //}

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
            Debug.Log("--------------- i");
            //if (i == numUltrasonics) //Add button clicked
            //{
            //    List<GameObject> updatedList = sensorManagerGUI.AddUltrasonic();
            //    UpdateSensorDropdown(ultrasonicDropdown, updatedList);
            //}
            //else //One of the existing sensors was selected
            //{
            //    sensorManagerGUI.SetUltrasonicAsCurrent(i);
            //    sensorManagerGUI.StartConfiguration();
            //    numUltrasonics++;
            //}
        }

        public void OnBeamBreakButtonPressed()
        {
            //sensorManagerGUI.AddBeamBreaker();
            //ultrasonicDropdown.RefreshShownValue();
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