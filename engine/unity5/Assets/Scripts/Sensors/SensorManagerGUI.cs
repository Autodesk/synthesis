using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.FSM;
using UnityEngine.Analytics;
using Synthesis.GUI;
using Synthesis.Camera;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Configuration;
using Assets.Scripts.GUI;
using Synthesis.Robot;

namespace Synthesis.Sensors
{
    /// <summary>
    /// This class handles every sensor-related GUI element in Unity
    /// </summary>
    class SensorManagerGUI : LinkedMonoBehaviour<MainState>
    {
        private StateMachine tabStateMachine;

        Toolkit toolkit;
        GameObject canvas;
        SensorBase currentSensor;
        public SensorManager sensorManager;
        DynamicCamera.CameraState preConfigState;
        DynamicCamera dynamicCamera;
        RobotCameraGUI robotCameraGUI;

        GameObject sensorOptionPanel;
        GameObject sensorTypePanel;
        GameObject cancelOptionButton;
        GameObject addSensorButton;
        GameObject selectExistingButton;
        GameObject sensorOptionToolTip;

        GameObject addUltrasonicButton;
        GameObject addBeamBreakerButton;
        GameObject addGyroButton;
        GameObject cancelTypeButton;
        

        GameObject sensorAnglePanel;
        GameObject xAngleEntry;
        GameObject yAngleEntry;
        GameObject zAngleEntry;
        GameObject showAngleButton;
        GameObject editAngleButton;

        bool changingAngle = false;
        bool changingAngleX = false;
        bool changingAngleY = false;
        bool changingAngleZ = false;
        float angleIncrement = 1f;
        int angleSign;

        GameObject sensorRangePanel;
        GameObject editRangeButton;
        GameObject showRangeButton;
        GameObject RangeEntry;
        Text rangeUnit;

        bool changingRange = false;
        float rangeIncrement = .01f;
        int rangeSign;

        GameObject showSensorButton;
        GameObject sensorConfigurationModeButton;
        GameObject configureSensorPanel;
        GameObject deleteSensorButton;
        GameObject hideSensorButton;
        GameObject sensorConfigHeader;

        GameObject lockPositionButton;
        GameObject lockAngleButton;
        GameObject lockRangeButton;

        GameObject sensorOutputPanel;

        private bool isChoosingOption;

        private bool isAddingSensor;
        private bool isAddingUltrasonic;
        private bool isAddingBeamBreaker;
        private bool isAddingGyro;

        private bool isSelectingSensor;
        private bool isEditingAngle;
        private bool isEditingRange;
        private bool isHidingOutput;

        //A list of all output panels instantiated
        private List<GameObject> sensorOutputPanels = new List<GameObject>();

        private void Start()
        {
            FindElements();
        }

        private void Update()
        {
            //Find the dynamic camera
            if (dynamicCamera == null)
            {
                dynamicCamera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
            }
            //When the current sensor is ready to be configured, call its UpdateTransformFunction and update its angle, range & node text in the corresponding panels
            if (currentSensor != null && currentSensor.IsChangingPosition)
            {
                currentSensor.UpdateTransform();
                UpdateSensorAnglePanel();
                UpdateSensorRangePanel();
            }
            showSensorButton.SetActive(sensorManager.GetActiveSensors().Count > 0 && isHidingOutput);

            //Allows users to save their configuration using enter
            if (isEditingAngle && UnityEngine.Input.GetKeyDown(KeyCode.Return)) ToggleEditAngle();
            if (isEditingRange && UnityEngine.Input.GetKeyDown(KeyCode.Return)) ToggleEditRange();

            //If an increment button is held, increment range or angle
            if (changingRange)
            {
                float temp = currentSensor.GetSensorRange() + rangeIncrement * rangeSign;
                currentSensor.SetSensorRange(temp >= 0 ? temp : 0);
                UpdateSensorRangePanel();
            }
            else if (currentSensor != null && currentSensor.IsChangingRange)
            {
                SyncSensorRange();
            }
            if (changingAngle)
            {
                if (changingAngleX)
                {
                    currentSensor.RotateTransform(angleIncrement * angleSign, 0, 0);
                }
                else if (changingAngleY)
                {
                    currentSensor.RotateTransform(0, angleIncrement * angleSign, 0);
                }
                else if (changingAngleZ)
                {
                    currentSensor.RotateTransform(0, 0, angleIncrement * angleSign);
                }
                UpdateSensorAnglePanel();
            }
            else if (currentSensor != null && currentSensor.IsChangingAngle)
            {
                SyncSensorAngle();
            }
        }

        /// <summary>
        /// Find ALL the GUI stuff needed for the sensor GUI to work
        /// </summary>
        private void FindElements()
        {
            tabStateMachine = Auxiliary.FindGameObject("Tabs").GetComponent<StateMachine>();

            canvas = GameObject.Find("Canvas");
            sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
            dynamicCamera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
            toolkit = GetComponent<Toolkit>();

            sensorOptionPanel = Auxiliary.FindObject(canvas, "SensorOptionPanel");
            sensorTypePanel = Auxiliary.FindObject(canvas, "SensorTypePanel");

            //For sensor option panel
            addSensorButton = Auxiliary.FindObject(sensorOptionPanel, "AddNewSensor");
            selectExistingButton = Auxiliary.FindObject(sensorOptionPanel, "ConfigureExistingSensor");
            cancelOptionButton = Auxiliary.FindObject(sensorOptionPanel, "CancelButton");
            sensorOptionToolTip = Auxiliary.FindObject(sensorOptionPanel, "ToolTipPanel");

            //For choosing sensor type
            addUltrasonicButton = Auxiliary.FindObject(sensorTypePanel, "AddUltrasonic");
            addBeamBreakerButton = Auxiliary.FindObject(sensorTypePanel, "AddBeamBreaker");
            addGyroButton = Auxiliary.FindObject(sensorTypePanel, "AddGyro");
            cancelTypeButton = Auxiliary.FindObject(sensorTypePanel, "CancelButton");

            //For Sensor position and attachment configuration
            configureSensorPanel = Auxiliary.FindObject(canvas, "SensorConfigurationPanel");
            sensorConfigurationModeButton = Auxiliary.FindObject(configureSensorPanel, "ConfigurationMode");
            deleteSensorButton = Auxiliary.FindObject(configureSensorPanel, "DeleteSensorButton");
            hideSensorButton = Auxiliary.FindObject(configureSensorPanel, "HideSensorButton");
            sensorConfigHeader = Auxiliary.FindObject(configureSensorPanel, "SensorConfigHeader");

            //For Sensor angle configuration
            sensorAnglePanel = Auxiliary.FindObject(canvas, "SensorAnglePanel");
            xAngleEntry = Auxiliary.FindObject(sensorAnglePanel, "xAngleEntry");
            yAngleEntry = Auxiliary.FindObject(sensorAnglePanel, "yAngleEntry");
            zAngleEntry = Auxiliary.FindObject(sensorAnglePanel, "zAngleEntry");
            showAngleButton = Auxiliary.FindObject(configureSensorPanel, "ShowSensorAngleButton");
            editAngleButton = Auxiliary.FindObject(sensorAnglePanel, "EditButton");

            //For range configuration
            sensorRangePanel = Auxiliary.FindObject(canvas, "RangePanel");
            RangeEntry = Auxiliary.FindObject(sensorRangePanel, "RangeEntry");
            showRangeButton = Auxiliary.FindObject(configureSensorPanel, "ShowSensorRangeButton");
            editRangeButton = Auxiliary.FindObject(sensorRangePanel, "EditButton");
            rangeUnit = Auxiliary.FindObject(sensorRangePanel, "RangeUnit").GetComponent<Text>();
            
            showSensorButton = Auxiliary.FindObject(canvas, "ShowOutputButton");
            sensorOutputPanel = Auxiliary.FindObject(canvas, "SensorOutputBorder");
            robotCameraGUI = GetComponent<RobotCameraGUI>();

        }

        #region Sensor Option Panel
        /// <summary>
        /// Method for "Add/Configure Sensor" button. End configuration ends all window related to sensor addition/configuration
        /// </summary>
        public void ToggleSensorOption()
        {
            //Deal with UI conflicts between robot camera & sensors
            robotCameraGUI.EndProcesses();
            toolkit.EndProcesses(true);
            isChoosingOption = !isChoosingOption;
            sensorOptionPanel.SetActive(isChoosingOption);
            if (isChoosingOption)
            {
                preConfigState = dynamicCamera.cameraState;
                dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera));
                ShowAllSensors();
            }
            else
            {
                EndProcesses();
            }
        }

        /// <summary>
        /// Activate the state of choosing anchor node for new sensors to attach
        /// </summary>
        public void ToggleAddSensor()
        {
            isAddingSensor = !isAddingSensor;
            selectExistingButton.SetActive(!isAddingSensor);
            cancelOptionButton.SetActive(isAddingSensor);
            cancelOptionButton.transform.position = selectExistingButton.transform.position;
            sensorOptionToolTip.SetActive(isAddingSensor);

            if (isAddingSensor)
            {
                addSensorButton.GetComponentInChildren<Text>().text = "Confirm";
            }
            else
            {
                addSensorButton.GetComponentInChildren<Text>().text = "Add New Sensor";
                //Activate sensor type panel if a valid node is selected
                sensorTypePanel.SetActive(true);
                sensorOptionPanel.SetActive(false);
                CancelOptionSelection();
            }
        }

        /// <summary>
        /// Activate the state of selecting existing sensors on the robot
        /// </summary>
        public void ToggleSelectExisting()
        {
            isSelectingSensor = !isSelectingSensor;
            sensorManager.SelectingSensor = isSelectingSensor;
            addSensorButton.SetActive(!isSelectingSensor);
            cancelOptionButton.SetActive(isSelectingSensor);
            cancelOptionButton.transform.position = addSensorButton.transform.position;
            sensorOptionToolTip.SetActive(isSelectingSensor);

            if (isSelectingSensor)
            {
                if (sensorManager.GetActiveSensors().Count > 0)
                {
                    selectExistingButton.GetComponentInChildren<Text>().text = "Confirm";
                    sensorOptionToolTip.GetComponentInChildren<Text>().text = "Select an existing sensor for configuration and Confirm";
                    UserMessageManager.Dispatch("Please select a sensor for configuration", 3f);
                }
                else
                {
                    UserMessageManager.Dispatch("No sensor on current robot", 3f);
                    CancelOptionSelection();
                }
            }
            else
            {
                selectExistingButton.GetComponentInChildren<Text>().text = "Select Existing Sensor";
                //Update selectedSensor
                SyncSensorSelection();
                //If a valid sensor is selected, start configuring it
                if (currentSensor != null)
                {
                    sensorOptionPanel.SetActive(false);
                    StartConfiguration();
                    //Clean up SelectedSensor and reset the panel
                    CancelOptionSelection();
                }
                //Stay at sensorOptionPanel
                else
                {
                    UserMessageManager.Dispatch("No sensor selected!", 3f);
                }
            }
        }

        /// <summary>
        /// Similar function as the SyncNodeFunction
        /// </summary>
        public void SyncSensorSelection()
        {
            if (sensorManager.SelectedSensor == null)
            {
                currentSensor = null;
            }
            else
            {
                currentSensor = sensorManager.SelectedSensor.GetComponent<SensorBase>();
                sensorManager.ClearSelectedSensor();
            }
        }

        /// <summary>
        /// Exit the state of selecting node attachment and go back to sensorOptionPanel
        /// </summary>
        public void CancelOptionSelection()
        {
            if (isAddingSensor)
            {
                isAddingSensor = false;
            }
            else if (isSelectingSensor)
            {
                isSelectingSensor = false;
                sensorManager.ClearSelectedSensor();
            }
            selectExistingButton.SetActive(true);
            selectExistingButton.GetComponentInChildren<Text>().text = "Select Existing Sensor";

            addSensorButton.SetActive(true);
            addSensorButton.GetComponentInChildren<Text>().text = "Add New Sensor";

            cancelOptionButton.SetActive(false);
            sensorManager.SelectingSensor = false;

            sensorOptionToolTip.SetActive(false);
        }

        #endregion

        #region Choose Sensor Type Panel
        /// <summary>
        /// Add a new ultrasonic sensor on the selectedNode and then start configuring it
        /// </summary>
        public void ToggleAddUltrasonic()
        {
            isAddingUltrasonic = !isAddingUltrasonic;
            addBeamBreakerButton.SetActive(!isAddingUltrasonic);
            addGyroButton.SetActive(!isAddingUltrasonic);
            cancelTypeButton.SetActive(isAddingUltrasonic);
            cancelTypeButton.transform.position = addBeamBreakerButton.transform.position;
            if (isAddingUltrasonic)
            {
                addUltrasonicButton.GetComponentInChildren<Text>().text = "Confirm";

                //Add a sensor
                AddUltrasonic();
                if (PlayerPrefs.GetInt("analytics") == 1)
                {
                    Analytics.CustomEvent("Added Ultrasonic Sensor", new Dictionary<string, object> //for analytics tracking
                    {
                    });
                }
            }
            else
            {
                addUltrasonicButton.GetComponentInChildren<Text>().text = "Add Ultrasonic";

                StartConfiguration();
                sensorTypePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Adds a new beam breaker sensor on the selectedNode and then start configuring it
        /// </summary>
        public void ToggleAddBeamBreaker()
        {
            isAddingBeamBreaker = !isAddingBeamBreaker;
            addUltrasonicButton.SetActive(!isAddingBeamBreaker);
            addGyroButton.SetActive(!isAddingBeamBreaker);
            cancelTypeButton.SetActive(isAddingBeamBreaker);
            cancelTypeButton.transform.position = addGyroButton.transform.position;
            if (isAddingBeamBreaker)
            {
                addBeamBreakerButton.GetComponentInChildren<Text>().text = "Confirm";

                AddBeamBreaker();

                if (PlayerPrefs.GetInt("analytics") == 1)
                {
                    Analytics.CustomEvent("Added Beam Breaker", new Dictionary<string, object> //for analytics tracking
                    {
                    });
                }
            }
            else
            {
                addBeamBreakerButton.GetComponentInChildren<Text>().text = "Add Beam Breaker";
                StartConfiguration();
                sensorTypePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Adds a new gyro sensor on the selectedNode and then start configuring it
        /// </summary>
        public void ToggleAddGyro()
        {
            isAddingGyro = !isAddingGyro;
            addUltrasonicButton.SetActive(!isAddingGyro);
            addBeamBreakerButton.SetActive(!isAddingGyro);
            cancelTypeButton.SetActive(isAddingGyro);
            cancelTypeButton.transform.position = addBeamBreakerButton.transform.position;
            if (isAddingGyro)
            {
                addGyroButton.GetComponentInChildren<Text>().text = "Confirm";
                //AddGyro();

                if (PlayerPrefs.GetInt("analytics") == 1)
                {
                    Analytics.CustomEvent("Added Gyro", new Dictionary<string, object> //for analytics tracking
                    {
                    });
                }
            }
            else
            {
                addGyroButton.GetComponentInChildren<Text>().text = "Add Gyro";
                StartConfiguration();
                sensorTypePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Cancel the current sensor type selection and destroy the sensor game object
        /// </summary>
        public void CancelTypeSelection()
        {
            if (isAddingBeamBreaker)
            {
                isAddingBeamBreaker = false;
                addBeamBreakerButton.GetComponentInChildren<Text>().text = "Add Beam Breaker";
            }
            else if (isAddingUltrasonic)
            {
                isAddingUltrasonic = false;
                addUltrasonicButton.GetComponentInChildren<Text>().text = "Add Ultrasonic";
            }
            else if (isAddingGyro)
            {
                isAddingGyro = false;
                addGyroButton.GetComponentInChildren<Text>().text = "Add Gyro";
            }
            addUltrasonicButton.SetActive(true);
            addBeamBreakerButton.SetActive(true);
            addGyroButton.SetActive(true);

            //Remove the current sensor - can't use this in EndProcesses because of this part
            if (currentSensor != null)
            {
                //sensorManager.RemoveSensor(currentSensor.gameObject); //works if this never gets called
                //Shift the panel up for the current sensor destroyed
                ShiftOutputPanels();
                Destroy(currentSensor.gameObject);
                currentSensor = null;
            }
            cancelTypeButton.SetActive(false);
        }

        public void SetUltrasonicAsCurrent(int i)
        {
            currentSensor = sensorManager.ultrasonicList[i].GetComponent<SensorBase>();
        }

        public void SetBeamBreakerAsCurrent(int i)
        {
            currentSensor = sensorManager.beamBreakerList[i].GetComponent<SensorBase>();
        }

        public void SetGyroAsCurrent(int i)
        {
            currentSensor = sensorManager.gyroList[i].GetComponent<SensorBase>();
        }

        /// <summary>
        /// Add an ultrasonic sensor to the selected node and display its output
        /// </summary>
        public List<GameObject> AddUltrasonic()
        {
            currentSensor = sensorManager.AddUltrasonic();
            DisplayOutput();
            StartConfiguration();
            return sensorManager.ultrasonicList;
        }

        /// <summary>
        /// Add a beam breaker sensor to the selected node and display its output
        /// </summary>
        public List<GameObject> AddBeamBreaker()
        {
            currentSensor = sensorManager.AddBeamBreaker();
            DisplayOutput();
            StartConfiguration();
            return sensorManager.beamBreakerList;
        }

        /// <summary>
        /// Add a gyro to the selected node and display its output
        /// </summary>
        public List<GameObject> AddGyro()
        {
            currentSensor = sensorManager.AddGyro();
            DisplayOutput();
            StartConfiguration();
            return sensorManager.gyroList;
        }

        #endregion

        #region Configuration GUI

        /// <summary>
        /// Start the sensor configuration
        /// </summary>
        public void StartConfiguration()
        {
            if (configureSensorPanel.activeSelf) configureSensorPanel.SetActive(false);
            if (currentSensor.sensorType.Equals("Gyro"))
            {
                configureSensorPanel = Auxiliary.FindObject(canvas, "GyroConfigurationPanel");
                sensorConfigurationModeButton = Auxiliary.FindObject(configureSensorPanel, "ConfigurationMode");
                deleteSensorButton = Auxiliary.FindObject(configureSensorPanel, "DeleteSensorButton");
                hideSensorButton = Auxiliary.FindObject(configureSensorPanel, "HideSensorButton");
                sensorConfigHeader = Auxiliary.FindObject(configureSensorPanel, "SensorConfigHeader");
            }
            else if (configureSensorPanel.name.Equals("GyroConfigurationPanel"))
            {
                configureSensorPanel = Auxiliary.FindObject(canvas, "SensorConfigurationPanel");
                sensorConfigurationModeButton = Auxiliary.FindObject(configureSensorPanel, "ConfigurationMode");
                deleteSensorButton = Auxiliary.FindObject(configureSensorPanel, "DeleteSensorButton");
                hideSensorButton = Auxiliary.FindObject(configureSensorPanel, "HideSensorButton");
                sensorConfigHeader = Auxiliary.FindObject(configureSensorPanel, "SensorConfigHeader");
            }
            HideInvisibleSensors();
            currentSensor.ChangeVisibility(true);
            SyncHideSensorButton();
            configureSensorPanel.SetActive(true);
            sensorConfigHeader.GetComponentInChildren<Text>().text = currentSensor.name;
            dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera, currentSensor.gameObject));
        }

        public void ToggleConfiguration()
        {
            if (configureSensorPanel.activeSelf)
            {
                configureSensorPanel.SetActive(false);
                if (currentSensor.transform.Find("IndicatorMoveArrows") != null) Destroy(currentSensor.transform.Find("IndicatorMoveArrows").gameObject);
            }
            else
            {
                StartConfiguration();
            }
        }

        /// <summary>
        /// Going into the state of selecting a new node and confirming it
        /// </summary>
        public void ToggleChangeNode()
        {
            StateMachine.SceneGlobal.PushState(new DefineSensorAttachmentState(currentSensor));
        }

        /// <summary>
        /// Update the local angle of the current Sensor to the Sensor angle panel
        /// </summary>
        public void UpdateSensorAnglePanel()
        {
            xAngleEntry.GetComponent<InputField>().text = currentSensor.transform.localEulerAngles.x.ToString();
            yAngleEntry.GetComponent<InputField>().text = currentSensor.transform.localEulerAngles.y.ToString();
            zAngleEntry.GetComponent<InputField>().text = currentSensor.transform.localEulerAngles.z.ToString();
        }

        /// <summary>
        /// Take the angle input and set the rotation of the current Sensor
        /// </summary>
        public void SyncSensorAngle()
        {
            float xTemp = 0;
            float yTemp = 0;
            float zTemp = 0;
            if (float.TryParse(xAngleEntry.GetComponent<InputField>().text, out xTemp) &&
                float.TryParse(yAngleEntry.GetComponent<InputField>().text, out yTemp) &&
                float.TryParse(zAngleEntry.GetComponent<InputField>().text, out zTemp))
            {
                currentSensor.transform.localRotation = Quaternion.Euler(new Vector3(xTemp, yTemp, zTemp));
            }
        }

        /// <summary>
        /// Control the button that toggles Sensor angle panel
        /// </summary>
        public void ToggleSensorAnglePanel()
        {
            currentSensor.IsChangingAngle = !currentSensor.IsChangingAngle;
            sensorAnglePanel.SetActive(currentSensor.IsChangingAngle);
            isEditingAngle = currentSensor.IsChangingAngle;

            //if (currentSensor.IsChangingAngle)
            //{
            //    showAngleButton.GetComponentInChildren<Text>().text = "Hide Sensor Angle";
            //}
            //else
            //{
            //    showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Sensor Angle";
            //}
        }

        /// <summary>
        /// Toggle angle edit mode and update angle from the input
        /// </summary>
        public void ToggleEditAngle()
        {
            isEditingAngle = !isEditingAngle;
            if (isEditingAngle)
            {
                editAngleButton.GetComponentInChildren<Text>().text = "Done";
            }
            else
            {
                editAngleButton.GetComponentInChildren<Text>().text = "Edit";
                SyncSensorAngle();
                isEditingAngle = false;
            }
        }

        public void ChangeSensorAngleX(int sign)
        {
            angleSign = sign;
            changingAngleX = true;
            changingAngle = true;
        }

        public void ChangeSensorAngleY(int sign)
        {
            angleSign = sign;
            changingAngleY = true;
            changingAngle = true;
        }

        public void ChangeSensorAngleZ(int sign)
        {
            angleSign = sign;
            changingAngleZ = true;
            changingAngle = true;
        }

        public void StopChangingSensorAngle()
        {
            changingAngleX = false;
            changingAngleY = false;
            changingAngleZ = false;
            changingAngle = false;
        }

        /// <summary>
        /// Update the local range of the current Sensor to the Sensor range panel
        /// </summary>
        public void UpdateSensorRangePanel()
        {
            //if (State.IsMetric) rangeUnit.text = "Range (meters)";
            //else rangeUnit.text = "Range (feet)";
            if (!isEditingRange)
            {
                RangeEntry.GetComponent<InputField>().text = currentSensor.GetSensorRange().ToString();
            }
        }

        /// <summary>
        /// Take the range input and set the range of the current Sensor
        /// </summary>
        public void SyncSensorRange()
        {
            float temp = 0;
            if ((float.TryParse(RangeEntry.GetComponent<InputField>().text, out temp) && temp >= 0))
            {
                currentSensor.SetSensorRange(temp, true);
            }
        }

        public void ChangeSensorRange(int sign)
        {
            rangeSign = sign;
            changingRange = true;
        }

        public void StopChangingSensorRange()
        {
            changingRange = false;
        }

        /// <summary>
        /// Control the button that toggles Sensor range panel
        /// </summary>
        public void ToggleSensorRangePanel()
        {
            RangeEntry.GetComponent<InputField>().text = currentSensor.GetSensorRange().ToString();

            currentSensor.IsChangingRange = !currentSensor.IsChangingRange;
            //isEditingRange = !isEditingRange;

            sensorRangePanel.SetActive(currentSensor.IsChangingRange);

            //if (!currentSensor.IsChangingRange) SyncSensorRange();

            //lockPositionButton.SetActive(currentSensor.IsChangingRange);
            //lockAngleButton.SetActive(currentSensor.IsChangingRange);

            //if (currentSensor.IsChangingRange)
            //{
            //showRangeButton.GetComponentInChildren<Text>().text = "Hide Sensor Range";
            //}
            //else
            //{
            //showRangeButton.GetComponentInChildren<Text>().text = "Show/Edit Sensor Range";
            //}
        }

        /// <summary>
        /// Toggle range edit mode and update range from the input
        /// </summary>
        public void ToggleEditRange()
        {
            isEditingRange = !isEditingRange;
            if (isEditingRange)
            {
                editRangeButton.GetComponentInChildren<Text>().text = "Done";
            }
            else
            {
                editRangeButton.GetComponentInChildren<Text>().text = "Edit";
                SyncSensorRange();
                isEditingRange = false;
            }
        }

        /// <summary>
        /// Toggle the state of changing the sensor position with move arrows
        /// </summary>
        public void ToggleChangePosition()
        {
            StateMachine.SceneGlobal.PushState(new SensorSpawnState(currentSensor.gameObject));
        }

        /// <summary>
        /// Reset configuration window to its default state
        /// </summary>
        public void ResetConfigurationWindow()
        {
            //showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Sensor Angle";
            //showRangeButton.GetComponentInChildren<Text>().text = "Show/Edit Sensor Range";
            //sensorConfigurationModeButton.GetComponentInChildren<Text>().text = "Configure Height";

            sensorAnglePanel.SetActive(false);
            sensorRangePanel.SetActive(false);
            configureSensorPanel.SetActive(false);
        }

        /// <summary>
        /// Delete the current sensor from the robot
        /// </summary>
        public void DeleteSensor()
        {
            //Don't change the order of following lines or it won't work
            string type = currentSensor.sensorType;

            Destroy(currentSensor.gameObject);
            sensorManager.RemoveSensor(currentSensor.gameObject, type);
            ShiftOutputPanels();
            currentSensor = null;

            EndProcesses();
            tabStateMachine.FindState<SensorToolbarState>().RemoveSensorFromDropdown(type,
                sensorManager.ultrasonicList, sensorManager.beamBreakerList, sensorManager.gyroList);
        }

        public void RemoveSensorsFromRobot(SimulatorRobot robot)
        {
            List<GameObject> sensorsOnRobot = sensorManager.GetSensorsFromRobot(robot);
            foreach (GameObject removingSensors in sensorsOnRobot)
            {
                string type = removingSensors.GetComponent<SensorBase>().sensorType;

                Destroy(removingSensors);
                sensorManager.RemoveSensor(removingSensors, type);
                ShiftOutputPanels();
                if(currentSensor != null && currentSensor.Equals(removingSensors.GetComponent<SensorBase>())) { currentSensor = null; EndProcesses(); }
                tabStateMachine.FindState<SensorToolbarState>().RemoveSensorFromDropdown(type,
                    sensorManager.ultrasonicList, sensorManager.beamBreakerList, sensorManager.gyroList);
            }
        }
        /// <summary>
        /// Toggle between showing current sensor and hiding it
        /// </summary>
        public void ToggleHideSensor()
        {
            currentSensor.ChangeVisibility(!currentSensor.IsVisible);
            Auxiliary.FindObject(configureSensorPanel, "VisibilityButton").GetComponentInChildren<Text>().text = currentSensor.IsVisible ? "Hide" : "Show";
        }

        /// <summary>
        /// Sync the text of hide button so that it actually reflect the state of the sensor
        /// </summary>
        public void SyncHideSensorButton()
        {
            if (currentSensor.IsVisible)
            {
                //hideSensorButton.GetComponentInChildren<Text>().text = "Hide Sensor";
            }
            else
            {
                //hideSensorButton.GetComponentInChildren<Text>().text = "Show Sensor";
            }
        }
        #endregion

        #region Sensor Output
        /// <summary>
        /// Create an output panel for the current sensor if there is not one for it. Output update is handled by each sensor (in SensorBase/other sensor scripts)
        /// </summary>
        public void DisplayOutput()
        {
            if (GameObject.Find(currentSensor.name + "_Panel") == null)
            {
                int index = sensorManager.GetSensorIndex(currentSensor.gameObject);
                GameObject panel = Instantiate(sensorManager.OutputPanel, Vector3.zero, Quaternion.identity, sensorOutputPanel.transform);
                panel.transform.parent = sensorOutputPanel.transform;
                panel.transform.localPosition = new Vector3(0, -42 - (index) * 56, 0);
                panel.name = currentSensor.name + "_Panel";
                sensorOutputPanels.Add(panel);
                ShowSensorOutput();
            }
        }

        /// <summary>
        /// Shift the position of the output panel according to the sensor index in the active sensor list
        /// first panel pos: -99, -55, 0, output panel header height 27, panel height 83 w/first panel
        /// </summary>
        public void ShiftOutputPanels()
        {
            ShowSensorOutput();
            //currentSensor = null;
            if (sensorManager.GetInactiveSensors().Count != 0)
            {
                foreach (GameObject sensor in sensorManager.GetInactiveSensors())
                {
                    //Have to catch this exception because the GameObject is not destroyed immediately
                    try
                    {
                        GameObject uselessPanel = GameObject.Find(sensor.name + "_Panel");
                        if (sensorOutputPanels.Contains(uselessPanel))
                        {
                            sensorOutputPanels.Remove(uselessPanel);
                            Destroy(uselessPanel);
                        }
                    }
                    catch (MissingReferenceException e)
                    {
                        continue;
                    }
                }
            }
            //Shift position of remaining panels
            if (sensorOutputPanels.Count > 0)
            {
                foreach (GameObject panel in sensorOutputPanels)
                {
                    string sensorName = panel.name.Substring(0, panel.name.IndexOf("_Panel"));
                    GameObject sensor = GameObject.Find(sensorName);
                    panel.transform.localPosition = new Vector3(0, -42 - (sensorManager.GetSensorIndex(sensor)) * 56, 0);
                }
            }
            else
            {
                showSensorButton.SetActive(false);
                sensorOutputPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Show sensor outputs
        /// </summary>
        public void ShowSensorOutput()
        {
            sensorOutputPanel.SetActive(true);
        }

        /// <summary>
        /// Hide sensor outputs
        /// </summary>
        public void HideSensorOutput()
        {
            sensorOutputPanel.SetActive(false);
            //isHidingOutput = true;
            //showSensorButton.SetActive(true);
            //showSensorButton.transform.position = sensorOutputPanel.transform.position;
        }

        /// <summary>
        /// Toggle between showing sensor outputs and hiding them
        /// </summary>
        public void ToggleSensorOutput()
        {
            if(sensorManager.GetActiveSensors().Count > 0) sensorOutputPanel.SetActive(!sensorOutputPanel.activeSelf);
            Auxiliary.FindObject(Auxiliary.FindObject(canvas, "SensorToolbar"), "ShowOutputsButton").GetComponentInChildren<Text>().text = sensorOutputPanel.activeSelf ? "Hide Outputs" : "Show Outputs";
        }
        #endregion

        #region Sensor Display
        /// <summary>
        /// Show all sensors temporarily
        /// </summary>
        public void ShowAllSensors()
        {
            foreach (GameObject sensor in sensorManager.GetActiveSensors())
            {
                sensor.GetComponent<SensorBase>().SetTemporaryVisible();
            }
        }

        /// <summary>
        /// Hide sensors that are set to invisible
        /// </summary>
        public void HideInvisibleSensors()
        {
            foreach (GameObject sensor in sensorManager.GetActiveSensors())
            {
                sensor.GetComponent<SensorBase>().SyncVisibility();
            }
        }
        #endregion

        /// <summary>
        /// Close all window related to adding/configuring sensor, also called in SimUI
        /// </summary>
        public void EndProcesses()
        {
            isChoosingOption = false;
            if (currentSensor != null)
            {
                currentSensor.GetComponentInChildren<MoveArrows>(true).gameObject.SetActive(false);
                currentSensor.ResetConfigurationState();
                currentSensor = null;
            }
            sensorOptionPanel.SetActive(false);
            sensorTypePanel.SetActive(false);
            //CancelOptionSelection();
            //CancelTypeSelection();
            ResetConfigurationWindow();
            //HideSensorOutput();

            //Switch back to the original camera state
            if (preConfigState != null)
            {
                dynamicCamera.SwitchToState(preConfigState);
                preConfigState = null;
            }

            HideInvisibleSensors();
        }
        public void ResetAngle()
        {
            currentSensor.transform.localRotation = Quaternion.Euler(Vector3.zero);
            UpdateSensorAnglePanel();
        }
        public void ResetRange()
        {
            currentSensor.SetSensorRange(currentSensor.sensorType.Equals("Ultrasonic") ? 10f : currentSensor.sensorType.Equals("Beam Break") ? 0.4f : 0);
            UpdateSensorRangePanel();
        }
    }
}
