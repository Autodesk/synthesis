using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.FSM;
using UnityEngine.Analytics;

/// <summary>
/// This class handles every sensor-related GUI elements in Unity
/// </summary>
class SensorManagerGUI : MonoBehaviour
{
    Toolkit toolkit;
    GameObject canvas;
    SensorBase currentSensor;
    SensorManager sensorManager;
    DynamicCamera.CameraState preConfigState;
    DynamicCamera dynamicCamera;
    RobotCameraGUI robotCameraGUI;
    MainState main;

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

    GameObject selectedNode;

    GameObject sensorAnglePanel;
    GameObject xAngleEntry;
    GameObject yAngleEntry;
    GameObject zAngleEntry;
    GameObject showAngleButton;
    GameObject editAngleButton;

    GameObject sensorRangePanel;
    GameObject editRangeButton;
    GameObject showRangeButton;
    GameObject RangeEntry;
    Text rangeUnit;

    GameObject showSensorButton;
    GameObject sensorConfigurationModeButton;
    GameObject changeSensorNodeButton;
    GameObject configureSensorPanel;
    GameObject cancelNodeSelectionButton;
    GameObject deleteSensorButton;
    GameObject hideSensorButton;

    GameObject lockPositionButton;
    GameObject lockAngleButton;
    GameObject lockRangeButton;

    GameObject sensorOutputPanel;

    Text sensorNodeText;

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

    /// <summary>
    /// Link the sensor GUI to main state
    /// </summary>
    private void Awake()
    {
        StateMachine.Instance.Link<MainState>(this);
    }
    private void Start()
    {
        FindElements();
    }

    private void Update()
    {
        if(main == null)
        {
            main = StateMachine.Instance.FindState<MainState>();
        }
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
        if (isEditingAngle && Input.GetKeyDown(KeyCode.Return)) ToggleEditAngle();
        if (isEditingRange && Input.GetKeyDown(KeyCode.Return)) ToggleEditRange();

    }

    /// <summary>
    /// Find ALL the GUI stuff needed for the sensor GUI to work
    /// </summary>
    private void FindElements()
    {
        canvas = GameObject.Find("Canvas");
        sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
        dynamicCamera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
        toolkit = GameObject.Find("StateMachine").GetComponent<Toolkit>();

        sensorOptionPanel = AuxFunctions.FindObject(canvas, "SensorOptionPanel");
        sensorTypePanel = AuxFunctions.FindObject(canvas, "SensorTypePanel");

        //For sensor option panel
        addSensorButton = AuxFunctions.FindObject(sensorOptionPanel, "AddNewSensor");
        selectExistingButton = AuxFunctions.FindObject(sensorOptionPanel, "ConfigureExistingSensor");
        cancelOptionButton = AuxFunctions.FindObject(sensorOptionPanel, "CancelButton");
        sensorOptionToolTip = AuxFunctions.FindObject(sensorOptionPanel, "ToolTipPanel");

        //For choosing sensor type
        addUltrasonicButton = AuxFunctions.FindObject(sensorTypePanel, "AddUltrasonic");
        addBeamBreakerButton = AuxFunctions.FindObject(sensorTypePanel, "AddBeamBreaker");
        addGyroButton = AuxFunctions.FindObject(sensorTypePanel, "AddGyro");
        cancelTypeButton = AuxFunctions.FindObject(sensorTypePanel, "CancelButton");

        //For Sensor position and attachment configuration
        configureSensorPanel = AuxFunctions.FindObject(canvas, "SensorConfigurationPanel");
        changeSensorNodeButton = AuxFunctions.FindObject(configureSensorPanel, "ChangeNodeButton");
        sensorConfigurationModeButton = AuxFunctions.FindObject(configureSensorPanel, "ConfigurationMode");
        sensorNodeText = AuxFunctions.FindObject(configureSensorPanel, "NodeText").GetComponent<Text>();
        cancelNodeSelectionButton = AuxFunctions.FindObject(configureSensorPanel, "CancelNodeSelectionButton");
        deleteSensorButton = AuxFunctions.FindObject(configureSensorPanel, "DeleteSensorButton");
        hideSensorButton = AuxFunctions.FindObject(configureSensorPanel, "HideSensorButton");

        //For Sensor angle configuration
        sensorAnglePanel = AuxFunctions.FindObject(canvas, "SensorAnglePanel");
        xAngleEntry = AuxFunctions.FindObject(sensorAnglePanel, "xAngleEntry");
        yAngleEntry = AuxFunctions.FindObject(sensorAnglePanel, "yAngleEntry");
        zAngleEntry = AuxFunctions.FindObject(sensorAnglePanel, "zAngleEntry");
        showAngleButton = AuxFunctions.FindObject(configureSensorPanel, "ShowSensorAngleButton");
        editAngleButton = AuxFunctions.FindObject(sensorAnglePanel, "EditButton");

        //For range configuration
        sensorRangePanel = AuxFunctions.FindObject(canvas, "SensorRangePanel");
        RangeEntry = AuxFunctions.FindObject(sensorRangePanel, "RangeEntry");
        showRangeButton = AuxFunctions.FindObject(configureSensorPanel, "ShowSensorRangeButton");
        editRangeButton = AuxFunctions.FindObject(sensorRangePanel, "EditButton");
        rangeUnit = AuxFunctions.FindObject(sensorRangePanel, "RangeUnit").GetComponent<Text>();

        lockPositionButton = AuxFunctions.FindObject(configureSensorPanel, "LockPositionButton");
        lockAngleButton = AuxFunctions.FindObject(configureSensorPanel, "LockAngleButton");
        lockRangeButton = AuxFunctions.FindObject(configureSensorPanel, "LockRangeButton");

        showSensorButton = AuxFunctions.FindObject(canvas, "ShowOutputButton");
        sensorOutputPanel = AuxFunctions.FindObject(canvas, "SensorOutputBorder");
        robotCameraGUI = GameObject.Find("StateMachine").GetComponent<RobotCameraGUI>();
        
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
        sensorManager.SelectingNode = isAddingSensor;
        selectExistingButton.SetActive(!isAddingSensor);
        cancelOptionButton.SetActive(isAddingSensor);
        cancelOptionButton.transform.position = selectExistingButton.transform.position;
        sensorOptionToolTip.SetActive(isAddingSensor);

        if (isAddingSensor)
        {
            addSensorButton.GetComponentInChildren<Text>().text = "Confirm";
            sensorOptionToolTip.GetComponentInChildren<Text>().text = "Select the robot node to which the new sensor will attach and Confirm";
            UserMessageManager.Dispatch("Please select a robot node for sensor attachment", 3);
        }
        else
        {
            addSensorButton.GetComponentInChildren<Text>().text = "Add New Sensor";
            //Turn off selectingNode state
            //Update the node selected to selectedNode
            SyncNodeSelection();
            //Activate sensor type panel if a valid node is selected
            if (selectedNode != null)
            {
                sensorTypePanel.SetActive(true);
                sensorOptionPanel.SetActive(false);
                CancelOptionSelection();
            }
            //Stay at sensor option panel
            else
            {
                UserMessageManager.Dispatch("No node selected!", 3f);
            }
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
    /// Set the GUI selectedNode to the sensorManager's selectedNode and then set the sensorManager's selectedNode to null
    /// </summary>
    public void SyncNodeSelection()
    {
        selectedNode = sensorManager.SelectedNode;
        sensorManager.ClearSelectedNode();
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
            sensorManager.ClearSelectedNode();
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
        sensorManager.SelectingNode = false;
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
            if (SimUI.changeAnalytics)
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

            if (SimUI.changeAnalytics)
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
            AddGyro();

            if (SimUI.changeAnalytics)
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
            sensorManager.RemoveSensor(currentSensor.gameObject);
            //Shift the panel up for the current sensor destroyed
            ShiftOutputPanels();
            Destroy(currentSensor.gameObject);
            currentSensor = null;
        }
        cancelTypeButton.SetActive(false);
    }

    /// <summary>
    /// Add an ultrasonic sensor to the selected node and display its output
    /// </summary>
    public void AddUltrasonic()
    {
        if (selectedNode != null)
        {
            currentSensor = sensorManager.AddUltrasonic(selectedNode, new Vector3(0, 0.2f, 0), Vector3.zero);
            DisplayOutput();
        }
    }

    /// <summary>
    /// Add a beam breaker sensor to the selected node and display its output
    /// </summary>
    public void AddBeamBreaker()
    {
        if (selectedNode != null)
        {
            currentSensor = sensorManager.AddBeamBreaker(selectedNode, Vector3.zero, Vector3.zero);
            DisplayOutput();
        }
    }

    /// <summary>
    /// Add a gyro to the selected node and display its output
    /// </summary>
    public void AddGyro()
    {
        if (selectedNode != null)
        {
            currentSensor = sensorManager.AddGyro(selectedNode, Vector3.zero, Vector3.zero);
            DisplayOutput();
        }
    }

    #endregion

    #region Configuration GUI

    /// <summary>
    /// Start the sensor configuration
    /// </summary>
    public void StartConfiguration()
    {
        HideInvisibleSensors();
        currentSensor.ChangeVisibility(true);
        SyncHideSensorButton();
        currentSensor.IsChangingPosition = true;
        configureSensorPanel.SetActive(true);
        sensorNodeText.text = "Current Node: " + currentSensor.transform.parent.gameObject.name;
        dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera, currentSensor.gameObject));
    }

    /// <summary>
    /// Toggle between changing position along horizontal plane or changing height
    /// </summary>
    public void ToggleConfigurationMode()
    {
        currentSensor.IsChangingHeight = !currentSensor.IsChangingHeight;
        if (currentSensor.IsChangingHeight)
        {
            sensorConfigurationModeButton.GetComponentInChildren<Text>().text = "Configure Horizontal Plane";
        }
        else
        {
            sensorConfigurationModeButton.GetComponentInChildren<Text>().text = "Configure Height";
        }
    }

    /// <summary>
    /// Going into the state of selecting a new node and confirming it
    /// </summary>
    public void ToggleChangeNode()
    {
        deleteSensorButton.SetActive(sensorManager.SelectingNode);

        if (!sensorManager.SelectingNode && sensorManager.SelectedNode == null)
        {
            sensorManager.DefineNode(); //Start selecting a new node
            changeSensorNodeButton.GetComponentInChildren<Text>().text = "Confirm";
            cancelNodeSelectionButton.SetActive(true);
        }
        else if (sensorManager.SelectingNode && sensorManager.SelectedNode != null)
        {
            //Change the node where Sensor is attached to, clear selected node, and update name of current node
            currentSensor.gameObject.transform.parent = sensorManager.SelectedNode.transform;
            sensorNodeText.text = "Current Node: " + currentSensor.transform.parent.gameObject.name;
            CancelNodeSelection();
        }
    }

    /// <summary>
    /// Cancel node selection button method
    /// </summary>
    public void CancelNodeSelection()
    {
        changeSensorNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
        cancelNodeSelectionButton.SetActive(false);
        deleteSensorButton.SetActive(true);
        hideSensorButton.SetActive(true);
        sensorManager.ClearSelectedNode();
        sensorManager.ResetNodeColors();
        sensorManager.SelectingNode = false;
    }
    /// <summary>
    /// Update the local angle of the current Sensor to the Sensor angle panel
    /// </summary>
    public void UpdateSensorAnglePanel()
    {
        if (!isEditingAngle)
        {
            xAngleEntry.GetComponent<InputField>().text = currentSensor.transform.localEulerAngles.x.ToString();
            yAngleEntry.GetComponent<InputField>().text = currentSensor.transform.localEulerAngles.y.ToString();
            zAngleEntry.GetComponent<InputField>().text = currentSensor.transform.localEulerAngles.z.ToString();
        }
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

        lockPositionButton.SetActive(currentSensor.IsChangingAngle);
        lockRangeButton.SetActive(currentSensor.IsChangingAngle);

        if (currentSensor.IsChangingAngle)
        {
            showAngleButton.GetComponentInChildren<Text>().text = "Hide Sensor Angle";
        }
        else
        {
            showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Sensor Angle";
        }
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

    /// <summary>
    /// Update the local range of the current Sensor to the Sensor range panel
    /// </summary>
    public void UpdateSensorRangePanel()
    {
        if (main.IsMetric) rangeUnit.text = "Range (meters)";
        else rangeUnit.text = "Range (feet)";
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

    /// <summary>
    /// Control the button that toggles Sensor range panel
    /// </summary>
    public void ToggleSensorRangePanel()
    {
        currentSensor.IsChangingRange = !currentSensor.IsChangingRange;
        sensorRangePanel.SetActive(currentSensor.IsChangingRange);

        lockPositionButton.SetActive(currentSensor.IsChangingRange);
        lockAngleButton.SetActive(currentSensor.IsChangingRange);

        if (currentSensor.IsChangingRange)
        {
            showRangeButton.GetComponentInChildren<Text>().text = "Hide Sensor Range";
        }
        else
        {
            showRangeButton.GetComponentInChildren<Text>().text = "Show/Edit Sensor Range";
        }
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
    /// Reset configuration window to its default state
    /// </summary>
    public void ResetConfigurationWindow()
    {
        lockPositionButton.SetActive(false);
        lockAngleButton.SetActive(false);
        lockRangeButton.SetActive(false);

        showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Sensor Angle";
        showRangeButton.GetComponentInChildren<Text>().text = "Show/Edit Sensor Range";
        sensorConfigurationModeButton.GetComponentInChildren<Text>().text = "Configure Height";

        sensorAnglePanel.SetActive(false);
        sensorRangePanel.SetActive(false);
        configureSensorPanel.SetActive(false);

        CancelNodeSelection();
    }

    /// <summary>
    /// Update the text for current node
    /// </summary>
    public void UpdateNodeAttachment()
    {
        sensorNodeText.text = "Current Node: " + currentSensor.transform.parent.gameObject.name;
    }

    /// <summary>
    /// Delete the current sensor from the robot
    /// </summary>
    public void DeleteSensor()
    {
        //Don't change the order of following lines or it won't work
        Destroy(currentSensor.gameObject);
        sensorManager.RemoveSensor(currentSensor.gameObject);
        ShiftOutputPanels();
        currentSensor = null;

        EndProcesses();
    }

    /// <summary>
    /// Toggle between showing current sensor and hiding it
    /// </summary>
    public void ToggleHideSensor()
    {
        currentSensor.IsVisible = !currentSensor.IsVisible;
        currentSensor.SyncVisibility();
        if (currentSensor.IsVisible)
        {
            hideSensorButton.GetComponentInChildren<Text>().text = "Hide Sensor";
        }
        else
        {
            hideSensorButton.GetComponentInChildren<Text>().text = "Show Sensor";
        }
    }

    /// <summary>
    /// Sync the text of hide button so that it actually reflect the state of the sensor
    /// </summary>
    public void SyncHideSensorButton()
    {
        if (currentSensor.IsVisible)
        {
            hideSensorButton.GetComponentInChildren<Text>().text = "Hide Sensor";
        }
        else
        {
            hideSensorButton.GetComponentInChildren<Text>().text = "Show Sensor";
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
    /// Toggle between showing sensor output and hiding them
    /// </summary>
    public void ShowSensorOutput()
    {
        sensorOutputPanel.SetActive(true);
        isHidingOutput = false ;
        showSensorButton.SetActive(false);
        sensorOutputPanel.transform.position = showSensorButton.transform.position;
    }

    /// <summary>
    /// Hide sensor output
    /// </summary>
    public void HideSensorOutput()
    {
        sensorOutputPanel.SetActive(false);
        isHidingOutput = true;
        showSensorButton.SetActive(true);
        showSensorButton.transform.position = sensorOutputPanel.transform.position;
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
            currentSensor.ResetConfigurationState();
            currentSensor = null;
        }
        sensorOptionPanel.SetActive(false);
        sensorTypePanel.SetActive(false);
        CancelOptionSelection();
        CancelTypeSelection();
        ResetConfigurationWindow();
        //HideSensorOutput();
        selectedNode = null;

        //Switch back to the original camera state
        if (preConfigState != null)
        {
            dynamicCamera.SwitchToState(preConfigState);
            preConfigState = null;
        }

        HideInvisibleSensors();
    }
}

