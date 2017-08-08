using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using BulletSharp;

class SensorManagerGUI : MonoBehaviour
{
    private SimUI simUI;
    private GameObject canvas;
    private SensorBase currentSensor;
    private SensorManager sensorManager;

    private GameObject configureSensorButton;
    private GameObject sensorOptionPanel;
    private GameObject sensorTypePanel;
    private GameObject cancelOptionButton;
    private GameObject addSensorButton;
    private GameObject selectExistingButton;

    private GameObject addUltrasonicButton;
    private GameObject addBeamBreakerButton;
    private GameObject cancelTypeButton;

    private GameObject selectedNode;

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

    GameObject lockPositionButton;
    GameObject lockAngleButton;
    GameObject lockRangeButton;

    Text sensorNodeText;

    private bool isChoosingOption;
    private bool isAddingSensor;
    private bool isAddingUltrasonic;
    private bool isAddingBeamBreaker;
    private bool isSelectingSensor;
    private bool isEditingAngle;
    private bool isEditingRange;
    private bool nodeConfirmed;
    private bool sensorConfirmed;

    private void Start()
    {
        FindElements();
    }

    private void Update()
    { 
        if (currentSensor != null && currentSensor.IsChangingPosition)
        {
            currentSensor.UpdateTransform();
            sensorNodeText.text = "Current Node: " + currentSensor.transform.parent.gameObject.name;
        }
    }

    private void FindElements()
    {
        canvas = GameObject.Find("Canvas");
        simUI = gameObject.GetComponent<SimUI>();
        sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();

        sensorOptionPanel = AuxFunctions.FindObject(canvas, "SensorOptionPanel");
        sensorTypePanel = AuxFunctions.FindObject(canvas, "SensorTypePanel");
        configureSensorButton = AuxFunctions.FindObject(canvas, "ConfigureSensorButton");

        //For sensor option panel
        addSensorButton = AuxFunctions.FindObject(sensorOptionPanel, "AddNewSensor");
        selectExistingButton = AuxFunctions.FindObject(sensorOptionPanel, "ConfigureExistingSensor");
        cancelOptionButton = AuxFunctions.FindObject(sensorOptionPanel, "CancelButton");

        //For choosing sensor type
        addUltrasonicButton = AuxFunctions.FindObject(sensorTypePanel, "AddUltrasonic");
        addBeamBreakerButton = AuxFunctions.FindObject(sensorTypePanel, "AddBeamBreaker");
        cancelTypeButton = AuxFunctions.FindObject(sensorTypePanel, "CancelButton");

        //For Sensor position and attachment configuration
        configureSensorPanel = AuxFunctions.FindObject(canvas, "SensorConfigurationPanel");
        changeSensorNodeButton = AuxFunctions.FindObject(configureSensorPanel, "ChangeNodeButton");
        sensorConfigurationModeButton = AuxFunctions.FindObject(configureSensorPanel, "ConfigurationMode");
        sensorNodeText = AuxFunctions.FindObject(configureSensorPanel, "NodeText").GetComponent<Text>();
        cancelNodeSelectionButton = AuxFunctions.FindObject(configureSensorPanel, "CancelNodeSelectionButton");

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

    }

    #region Sensor Option Panel

    public void ToggleSensorOption()
    {
        isChoosingOption = !isChoosingOption;
        sensorOptionPanel.SetActive(isChoosingOption);
        if (isChoosingOption)
        {
            configureSensorButton.GetComponentInChildren<Text>().text = "End Configuration";
        }
        else
        {
            configureSensorButton.GetComponentInChildren<Text>().text = "Add/Configure Sensor";
            EndProcesses();
        }
    }

    public void ToggleAddSensor()
    {
        isAddingSensor = !isAddingSensor;
        if (isAddingSensor)
        {
            addSensorButton.GetComponentInChildren<Text>().text = "Confirm";
            selectExistingButton.SetActive(false);
            cancelOptionButton.SetActive(sensorManager.SelectingNode = true);
            UserMessageManager.Dispatch("Please select a robot node for sensor attachment", 3);
        }
        else
        {
            addSensorButton.GetComponentInChildren<Text>().text = "Add New Sensor";
            selectExistingButton.SetActive(true);
            sensorManager.SelectingNode = false;
            SyncNodeSelection();
            Debug.Log(selectedNode.name);
            if (selectedNode != null)
            {
                sensorTypePanel.SetActive(true);
                sensorOptionPanel.SetActive(false);
            }
            else
            {
                UserMessageManager.Dispatch("No node selected!", 3f);
            }
        }
    }

    public void ToggleSelectExisting()
    {
        isSelectingSensor = !isSelectingSensor;
        if (isSelectingSensor)
        {
            selectExistingButton.GetComponentInChildren<Text>().text = "Confirm";
            addSensorButton.SetActive(false);
            cancelOptionButton.SetActive(sensorManager.SelectingSensor = true);
            UserMessageManager.Dispatch("Please select a sensor for configuration", 3f);

        }
        else
        {
            selectExistingButton.GetComponentInChildren<Text>().text = "Select Existing Sensor";
            addSensorButton.SetActive(true);
            sensorManager.SelectingSensor = false;
            SyncSensorSelection();
            if(currentSensor != null)
            {
                sensorOptionPanel.SetActive(false);
                StartConfiguration();
            }
            else
            {
                UserMessageManager.Dispatch("No sensor selected!", 3f);
            }
        }
    }

    public void SyncNodeSelection()
    {
        selectedNode = sensorManager.SelectedNode;
        sensorManager.ClearSelectedNode();
    }

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
    /// Exit the state of selecting node attachment
    /// </summary>
    public void CancelOptionSelection()
    {
        if (isAddingSensor)
        {
            isAddingSensor = false;
            sensorManager.ClearSelectedNode();
            addSensorButton.GetComponentInChildren<Text>().text = "Add New Sensor";
            selectExistingButton.SetActive(true);
        }else if (isSelectingSensor)
        {
            isSelectingSensor = false;
            sensorManager.ClearSelectedSensor();
            selectExistingButton.GetComponentInChildren<Text>().text = "Select Existing Sensor";
            addSensorButton.SetActive(true);
        }
        cancelOptionButton.SetActive(false);
    }

    #endregion

    #region Choose Sensor Type Panel
    public void ToggleAddUltrasonic()
    {
        isAddingUltrasonic = !isAddingUltrasonic;

        if (isAddingUltrasonic)
        {
            addUltrasonicButton.GetComponentInChildren<Text>().text = "Confirm";
            addBeamBreakerButton.SetActive(false);
            cancelTypeButton.SetActive(true);
            AddUltrasonic();
            Debug.Log(currentSensor.name);
        }
        else
        {
            addUltrasonicButton.GetComponentInChildren<Text>().text = "Add Ultrasonic";
            Debug.Log(currentSensor.name);
            StartConfiguration();
            sensorTypePanel.SetActive(false);
        }
    }

    public void ToggleAddBeamBreaker()
    {
        isAddingBeamBreaker = !isAddingBeamBreaker;
        if (isAddingBeamBreaker)
        {
            addBeamBreakerButton.GetComponentInChildren<Text>().text = "Confirm";
            addUltrasonicButton.SetActive(false);
            cancelTypeButton.SetActive(true);
            AddBeamBreaker();
        }
        else
        {
            addBeamBreakerButton.GetComponentInChildren<Text>().text = "Add Beam Breaker";
            StartConfiguration();
            sensorTypePanel.SetActive(false);
        }
    }

    public void CancelSensorTypeSelection()
    {
        if (isAddingBeamBreaker)
        {
            isAddingBeamBreaker = false;
            addBeamBreakerButton.GetComponentInChildren<Text>().text = "Add Beam Breaker";
            addUltrasonicButton.SetActive(true);
        }else if (isAddingUltrasonic)
        {
            isAddingUltrasonic = false;
            addUltrasonicButton.GetComponentInChildren<Text>().text = "Add Ultrasonic";
            addBeamBreakerButton.SetActive(true);
        }
        Destroy(currentSensor.gameObject);
        cancelTypeButton.SetActive(false);
    }

    public void AddUltrasonic()
    {
        if (selectedNode != null)
        {
            currentSensor = sensorManager.AddUltrasonicSensor(selectedNode, new Vector3(0, 0.2f, 0), Vector3.zero);
        }
    }
    public void AddBeamBreaker()
    {
        if (selectedNode != null)
        {
            currentSensor = sensorManager.AddBeamBreaker(selectedNode, Vector3.zero, Vector3.zero);
        }
    }

    #endregion

    #region Configuration GUI
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
        }
        else if (sensorManager.SelectingNode && sensorManager.SelectedNode != null)
        {
            //Change the node where Sensor is attached to, clear selected node, and update name of current node
            currentSensor.gameObject.transform.parent = sensorManager.SelectedNode.transform;
            sensorNodeText.text = "Current Node: " + currentSensor.transform.parent.gameObject.name;
            changeSensorNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
            cancelNodeSelectionButton.SetActive(false);
            sensorManager.ClearSelectedNode();
            sensorManager.SelectingNode = false;
        }
    }

    public void CancelNodeSelection()
    {
        changeSensorNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
        cancelNodeSelectionButton.SetActive(false);
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
    /// Update the local FOV of the current Sensor to the Sensor angle panel
    /// </summary>
    public void UpdateSensorRangePanel()
    {
        if (!isEditingRange)
        {
            RangeEntry.GetComponent<InputField>().text = currentSensor.GetSensorRange().ToString();
        }
    }

    /// <summary>
    /// Take the FOV input and set the rotation of the current Sensor
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
    /// Control the button that toggles Sensor FOV panel
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
    /// Toggle FOV edit mode and update FOV from the input
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

    public void UpdateNodeAttachment()
    {
        sensorNodeText.text = "Current Node: " + currentSensor.transform.parent.gameObject.name;
    }

    #endregion


    public void EndProcesses()
    {
        isChoosingOption = isSelectingSensor = isAddingBeamBreaker = isAddingUltrasonic = nodeConfirmed = false;
        sensorOptionPanel.SetActive(false);
        configureSensorPanel.SetActive(false);
        sensorTypePanel.SetActive(false);
        if (currentSensor != null)
        {
            Destroy(currentSensor.gameObject);
            sensorManager.SelectingNode = false;
            currentSensor.IsChangingPosition = false;
            currentSensor = null;
        }
        selectedNode = null;
    }

    public void StartConfiguration()
    {
        currentSensor.IsChangingPosition = true;
        configureSensorPanel.SetActive(true);
    }
}

