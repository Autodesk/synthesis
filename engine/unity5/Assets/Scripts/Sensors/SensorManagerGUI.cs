﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class SensorManagerGUI : MonoBehaviour
{
    SimUI simUI;
    GameObject canvas;
    SensorBase currentSensor;
    SensorManager sensorManager;
    DynamicCamera.CameraState preConfigState;
    DynamicCamera dynamicCamera;

    GameObject configureSensorButton;
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

    GameObject showSensorButton;
    GameObject sensorConfigurationModeButton;
    GameObject changeSensorNodeButton;
    GameObject configureSensorPanel;
    GameObject cancelNodeSelectionButton;
    GameObject deleteSensorButton;

    GameObject lockPositionButton;
    GameObject lockAngleButton;
    GameObject lockRangeButton;

    GameObject hideOutputPanelsButton;

    Text sensorNodeText;

    private bool isChoosingOption;

    private bool isAddingSensor;
    private bool isAddingUltrasonic;
    private bool isAddingBeamBreaker;
    private bool isAddingGyro;

    private bool isSelectingSensor;
    private bool isEditingAngle;
    private bool isEditingRange;
    private bool nodeConfirmed;
    private bool sensorConfirmed;
    private bool isHidingOutput;

    //A list of all output panels instantiated
    private List<GameObject> sensorOutputPanels = new List<GameObject>();

    private void Start()
    {
        FindElements();
    }

    private void Update()
    {
        if (dynamicCamera == null)
        {
            dynamicCamera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
        }
        //When the current sensor is ready to be configured, call its UpdateTransformFunction
        if (currentSensor != null && currentSensor.IsChangingPosition)
        {
            currentSensor.UpdateTransform();
            sensorNodeText.text = "Current Node: " + currentSensor.transform.parent.gameObject.name;
            UpdateSensorAnglePanel();
            UpdateSensorRangePanel();
        }
    }

    /// <summary>
    /// Find ALL the GUI stuff needed for the sensor GUI to work
    /// </summary>
    private void FindElements()
    {
        canvas = GameObject.Find("Canvas");
        simUI = gameObject.GetComponent<SimUI>();
        sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
        dynamicCamera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();

        sensorOptionPanel = AuxFunctions.FindObject(canvas, "SensorOptionPanel");
        sensorTypePanel = AuxFunctions.FindObject(canvas, "SensorTypePanel");
        configureSensorButton = AuxFunctions.FindObject(canvas, "ConfigureSensorButton");

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

        lockPositionButton = AuxFunctions.FindObject(configureSensorPanel, "LockPositionButton");
        lockAngleButton = AuxFunctions.FindObject(configureSensorPanel, "LockAngleButton");
        lockRangeButton = AuxFunctions.FindObject(configureSensorPanel, "LockRangeButton");

        hideOutputPanelsButton = AuxFunctions.FindObject(canvas, "HideOutputButton");
    }

    #region Sensor Option Panel
    /// <summary>
    /// Method for "Add/Configure Sensor" button. End configuration ends all window related to sensor addition/configuration
    /// </summary>
    public void ToggleSensorOption()
    {
        isChoosingOption = !isChoosingOption;
        sensorOptionPanel.SetActive(isChoosingOption);
        if (isChoosingOption)
        {
            preConfigState = dynamicCamera.cameraState;
            dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera));
            configureSensorButton.GetComponentInChildren<Text>().text = "End Configuration";
        }
        else
        {
            configureSensorButton.GetComponentInChildren<Text>().text = "Add/Configure Sensor";
            EndProcesses();
        }
    }

    /// <summary>
    /// Activate the state of choosing anchor node for new sensors to attach
    /// </summary>
    public void ToggleAddSensor()
    {
        isAddingSensor = !isAddingSensor;
        if (isAddingSensor)
        {
            addSensorButton.GetComponentInChildren<Text>().text = "Confirm";
            selectExistingButton.SetActive(false);
            sensorOptionToolTip.SetActive(true);
            sensorOptionToolTip.GetComponentInChildren<Text>().text = "Select the robot node to which the new sensor will attach and Confirm";
            cancelOptionButton.SetActive(sensorManager.SelectingNode = true);
            UserMessageManager.Dispatch("Please select a robot node for sensor attachment", 3);
        }
        else
        {
            addSensorButton.GetComponentInChildren<Text>().text = "Add New Sensor";
            selectExistingButton.SetActive(true);
            //Turn off selectingNode state
            sensorManager.SelectingNode = false;
            //Update the node selected to selectedNode
            SyncNodeSelection();

            cancelOptionButton.SetActive(false);
            sensorOptionToolTip.SetActive(false);
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
        if (isSelectingSensor)
        {
            selectExistingButton.GetComponentInChildren<Text>().text = "Confirm";
            addSensorButton.SetActive(false);
            sensorOptionToolTip.SetActive(true);
            sensorOptionToolTip.GetComponentInChildren<Text>().text = "Select an existing sensor for configuration and Confirm";
            cancelOptionButton.SetActive(sensorManager.SelectingSensor = true);
            UserMessageManager.Dispatch("Please select a sensor for configuration", 3f);

        }
        else
        {
            cancelOptionButton.SetActive(false);
            sensorOptionToolTip.SetActive(false);

            selectExistingButton.GetComponentInChildren<Text>().text = "Select Existing Sensor";
            addSensorButton.SetActive(true);
            //Turn off selecting sensor state
            sensorManager.SelectingSensor = false;
            //Update selectedSensor
            SyncSensorSelection();
            //If a valid sensor is selected, start configuring it
            if (currentSensor != null)
            {
                sensorOptionPanel.SetActive(false);
                StartConfiguration();
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
        selectExistingButton.GetComponentInChildren<Text>().text = "Select Existing Sensor";
        addSensorButton.SetActive(true);
        addSensorButton.GetComponentInChildren<Text>().text = "Add New Sensor";
        selectExistingButton.SetActive(true);
        cancelOptionButton.SetActive(false);
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
        if (isAddingUltrasonic)
        {
            addUltrasonicButton.GetComponentInChildren<Text>().text = "Confirm";

            //Add a sensor
            AddUltrasonic();
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
        if (isAddingBeamBreaker)
        {
            addBeamBreakerButton.GetComponentInChildren<Text>().text = "Confirm";

            AddBeamBreaker();
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
        if (isAddingGyro)
        {
            addGyroButton.GetComponentInChildren<Text>().text = "Confirm";

            AddGyro();
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
    public void CancelSensorTypeSelection()
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
        Destroy(currentSensor.gameObject);
        sensorManager.RemoveSensor(currentSensor.gameObject);
        cancelTypeButton.SetActive(false);
    }

    /// <summary>
    /// Methods that call corresponding add sensor functions in sensorManager, attach a new sensor on selectedNode
    /// </summary>
    public void AddUltrasonic()
    {
        if (selectedNode != null)
        {
            currentSensor = sensorManager.AddUltrasonic(selectedNode, new Vector3(0, 0.2f, 0), Vector3.zero);
        }
    }
    public void AddBeamBreaker()
    {
        if (selectedNode != null)
        {
            currentSensor = sensorManager.AddBeamBreaker(selectedNode, Vector3.zero, Vector3.zero);
        }
    }
    public void AddGyro()
    {
        if (selectedNode != null)
        {
            currentSensor = sensorManager.AddGyro(selectedNode, Vector3.zero, Vector3.zero);
        }
    }

    #endregion

    #region Configuration GUI

    /// <summary>
    /// Start the sensor configuration
    /// </summary>
    public void StartConfiguration()
    {
        currentSensor.IsChangingPosition = true;
        configureSensorPanel.SetActive(true);
        dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera, currentSensor.gameObject));
        DisplayOutput();
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
        if (!sensorManager.SelectingNode && sensorManager.SelectedNode == null)
        {
            sensorManager.DefineNode(); //Start selecting a new node
            changeSensorNodeButton.GetComponentInChildren<Text>().text = "Confirm";
            cancelNodeSelectionButton.SetActive(true);
            deleteSensorButton.SetActive(false);
        }
        else if (sensorManager.SelectingNode && sensorManager.SelectedNode != null)
        {
            //Change the node where Sensor is attached to, clear selected node, and update name of current node
            currentSensor.gameObject.transform.parent = sensorManager.SelectedNode.transform;
            sensorNodeText.text = "Current Node: " + currentSensor.transform.parent.gameObject.name;
            CancelNodeSelection();
            deleteSensorButton.SetActive(true);
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
        sensorManager.ClearSelectedNode();
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
            //Debug.Log("Sync angle!");
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
            currentSensor.SetSensorRange(temp);
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
        Destroy(currentSensor.gameObject);
        sensorManager.RemoveSensor(currentSensor.gameObject);
        ShiftOutputPanels();
        currentSensor = null;
        configureSensorButton.GetComponentInChildren<Text>().text = "Add/Configure Sensor";
        EndProcesses();
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
            GameObject panel = Instantiate(sensorManager.OutputPanel, new Vector3(1185, 700 - (index+1) * 60, 0), new Quaternion(0, 0, 0, 0), canvas.transform);
            panel.transform.parent = canvas.transform;
            panel.name = currentSensor.name + "_Panel";
            sensorOutputPanels.Add(panel);
            DisplayAllOutput();
        }
    }

    /// <summary>
    /// Shift the position of the output panel according to the sensor index in the active sensor list
    /// </summary>
    public void ShiftOutputPanels()
    {
        //Find the deleted sensor's corresponding panel and destroy it
        GameObject uselessPanel = GameObject.Find(currentSensor.name + "_Panel");
        sensorOutputPanels.Remove(uselessPanel);
        Destroy(uselessPanel);

        //Shift position of remaining panels
        foreach (GameObject panel in sensorOutputPanels)
        {
            string sensorName = panel.name.Substring(0, panel.name.IndexOf("_Panel"));
            GameObject sensor = GameObject.Find(sensorName);
            panel.transform.position = new Vector3(1185, 700 - (sensorManager.GetSensorIndex(sensor)+1) * 60, 0);
        }
    }

    /// <summary>
    /// Toggle between showing sensor output and hiding them
    /// </summary>
    public void ToggleSensorOutput()
    {
        isHidingOutput = !isHidingOutput;

        foreach (GameObject panel in sensorOutputPanels)
        {
            panel.SetActive(!isHidingOutput);
        }

        if (isHidingOutput)
        {
            hideOutputPanelsButton.GetComponentInChildren<Text>().text = "Show Sensor Output";
        }
        else
        {
            hideOutputPanelsButton.GetComponentInChildren<Text>().text = "Hide Sensor Output";
        }
    }

    /// <summary>
    /// Display all sensor output and set the toggle button text to "Hide Sensor Output"
    /// </summary>
    public void DisplayAllOutput()
    {
        isHidingOutput = false;
        foreach (GameObject panel in sensorOutputPanels)
        {
            panel.SetActive(true);
        }
        hideOutputPanelsButton.SetActive(true);
        hideOutputPanelsButton.GetComponentInChildren<Text>().text = "Hide Sensor Output";
    }
    #endregion

    /// <summary>
    /// Close all window related to adding/configuring sensor
    /// </summary>
    public void EndProcesses()
    {
        isChoosingOption = isSelectingSensor = isAddingSensor = isAddingBeamBreaker = isAddingUltrasonic = nodeConfirmed = false;
        sensorOptionPanel.SetActive(false);
        configureSensorPanel.SetActive(false);
        sensorTypePanel.SetActive(false);
        selectedNode = null;
        CancelNodeSelection();
        CancelOptionSelection();
        configureSensorButton.GetComponentInChildren<Text>().text = "Add/Configure Sensor";
        if (currentSensor != null)
        {
            currentSensor.IsChangingPosition = false;
        }

        if (preConfigState != null)
        {
            dynamicCamera.SwitchToState(preConfigState);
            preConfigState = null;
        }
    }
}

