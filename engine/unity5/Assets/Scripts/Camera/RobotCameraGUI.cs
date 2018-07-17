using UnityEngine;
using UnityEngine.UI;
using Synthesis.FSM;
using Synthesis.Sensors;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Configuration;

namespace Synthesis.Camera
{
    /// <summary>
    /// This class handles all GUI elements related to robot camera in Unity
    /// </summary>
    class RobotCameraGUI : LinkedMonoBehaviour<MainState>
    {
        //Stuff needed to make gui work
        GameObject canvas;
        DynamicCamera dynamicCamera;
        DynamicCamera.CameraState preConfigCamState;
        GameObject robotCameraListObject;
        RobotCameraManager robotCameraManager;
        SensorManagerGUI sensorManagerGUI;

        //Angle panel
        GameObject cameraAnglePanel;
        GameObject xAngleEntry;
        GameObject yAngleEntry;
        GameObject zAngleEntry;
        GameObject showAngleButton;
        GameObject editAngleButton;

        //FOV panel
        GameObject cameraFOVPanel;
        GameObject editFOVButton;
        GameObject showFOVButton;
        GameObject FOVEntry;

        GameObject robotCameraViewWindow;
        RenderTexture robotCameraView;

        //The indicator object is originally under robot camera list in unity scene
        public GameObject CameraIndicator;
        GameObject showCameraButton;

        //Camera configuration
        GameObject configureRobotCameraButton;
        GameObject configureCameraPanel;
        //Toggle between horizontal plane and height
        GameObject changeCameraNodeButton;
        Text cameraNodeText;
        GameObject cancelNodeSelectionButton;

        //Dark buttons that lock the corresponding button so users can't do different configuration options at the same time
        GameObject lockPositionButton;
        GameObject lockAngleButton;
        GameObject lockFOVButton;

        private bool usingRobotView = false;
        private bool indicatorActive = false;
        private bool isEditingAngle;
        private bool isEditingFOV;

        private void Start()
        {
            FindGUIElements();
            dynamicCamera = State.DynamicCameraObject.GetComponent<DynamicCamera>();
        }

        private void Update()
        {
            //Make sure main state and dynamic camera get initialized
            //Update gui about robot camera once main and dynamic camera is ready
            if (dynamicCamera != null)
            {
                UpdateCameraWindow();
                if (indicatorActive)
                {
                    UpdateCameraAnglePanel();
                    UpdateCameraFOVPanel();
                    UpdateNodeAttachment();
                    UpdateIndicatorTransform();
                }
            }

            //Allows users to save their configuration using enter
            if (isEditingAngle && UnityEngine.Input.GetKeyDown(KeyCode.Return)) ToggleEditAngle();
            if (isEditingFOV && UnityEngine.Input.GetKeyDown(KeyCode.Return)) ToggleEditFOV();
        }

        #region robot camera GUI functions

        /// <summary>
        /// Find all robot camera related GUI elements in the canvas
        /// </summary>
        public void FindGUIElements()
        {
            canvas = GameObject.Find("Canvas");
            sensorManagerGUI = GetComponent<SensorManagerGUI>();

            //For robot camera view window
            robotCameraView = Resources.Load("Images/RobotCameraView") as RenderTexture;
            robotCameraViewWindow = Auxiliary.FindObject(canvas, "RobotCameraPanelBorder");

            //For robot camera manager
            robotCameraListObject = GameObject.Find("RobotCameraList");
            robotCameraManager = robotCameraListObject.GetComponent<RobotCameraManager>();

            //For camera indicator
            if (CameraIndicator == null)
            {
                CameraIndicator = Auxiliary.FindObject(robotCameraListObject, "CameraIndicator");
            }
            showCameraButton = Auxiliary.FindObject(canvas, "ShowCameraButton");

            //For camera position and attachment configuration
            configureCameraPanel = Auxiliary.FindObject(canvas, "CameraConfigurationPanel");
            configureRobotCameraButton = Auxiliary.FindObject(canvas, "CameraConfigurationButton");
            changeCameraNodeButton = Auxiliary.FindObject(configureCameraPanel, "ChangeNodeButton");
            cameraNodeText = Auxiliary.FindObject(configureCameraPanel, "NodeText").GetComponent<Text>();
            cancelNodeSelectionButton = Auxiliary.FindObject(configureCameraPanel, "CancelNodeSelectionButton");

            //For camera angle configuration
            cameraAnglePanel = Auxiliary.FindObject(canvas, "CameraAnglePanel");
            xAngleEntry = Auxiliary.FindObject(cameraAnglePanel, "xAngleEntry");
            yAngleEntry = Auxiliary.FindObject(cameraAnglePanel, "yAngleEntry");
            zAngleEntry = Auxiliary.FindObject(cameraAnglePanel, "zAngleEntry");
            showAngleButton = Auxiliary.FindObject(configureCameraPanel, "ShowCameraAngleButton");
            editAngleButton = Auxiliary.FindObject(cameraAnglePanel, "EditButton");

            //For field of view configuration
            cameraFOVPanel = Auxiliary.FindObject(canvas, "CameraFOVPanel");
            FOVEntry = Auxiliary.FindObject(cameraFOVPanel, "FOVEntry");
            showFOVButton = Auxiliary.FindObject(configureCameraPanel, "ShowCameraFOVButton");
            editFOVButton = Auxiliary.FindObject(cameraFOVPanel, "EditButton");

            lockPositionButton = Auxiliary.FindObject(configureCameraPanel, "LockPositionButton");
            lockAngleButton = Auxiliary.FindObject(configureCameraPanel, "LockAngleButton");
            lockFOVButton = Auxiliary.FindObject(configureCameraPanel, "LockFOVButton");
        }
        /// <summary>
        /// Updates the robot camera view window
        /// </summary>
        private void UpdateCameraWindow()
        {
            //Can use robot view when dynamicCamera is active
            if (usingRobotView && State.DynamicCameraObject.activeSelf)
            {
                //Make sure there is camera on robot
                if (robotCameraManager.CurrentCamera != null)
                {
                    robotCameraManager.CurrentCamera.SetActive(true);
                    robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().targetTexture = robotCameraView;
                    //Toggle the robot camera using Z (can be changed later)
                    if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
                    {
                        //Reset the targetTexture of current camera or they will conflict
                        robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().targetTexture = null;
                        robotCameraManager.ToggleCamera();
                        robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().targetTexture = robotCameraView;
                    }
                }
            }
            //Free the target texture of the current camera when the window is closed (for normal toggle camera function)
            else
            {
                if (robotCameraManager.CurrentCamera != null)
                    robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().targetTexture = null;
            }
        }

        /// <summary>
        /// Toggles the state of usingRobotView when the camera button in toolbar is clicked
        /// </summary>
        public void ToggleCameraWindow()
        {
            //Deal with UI conflicts (configuration stuff) between robot camera & sensors
            sensorManagerGUI.EndProcesses();
            usingRobotView = !usingRobotView;
            robotCameraViewWindow.SetActive(usingRobotView);
            if (usingRobotView)
            {
                robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().targetTexture = robotCameraView;
                ToggleCameraIndicator();
            }
            else
            {
                //Free the target texture and disable the camera since robot camera has more depth than main camera
                robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().targetTexture = null;
                robotCameraManager.CurrentCamera.SetActive(false);
                //Close the panel when indicator is not active and stop all configuration
                configureCameraPanel.SetActive(false);
                configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure";
                EndProcesses();
            }
        }

        /// <summary>
        /// Toggles the state of showing or hiding the robot indicator
        /// </summary>
        public void ToggleCameraIndicator()
        {
            indicatorActive = !indicatorActive;
            configureRobotCameraButton.SetActive(indicatorActive);

            if (indicatorActive)
            {
                //Only allow the camera configuration when the indicator is active
                showCameraButton.GetComponentInChildren<Text>().text = "Hide Camera";
            }
            else
            {
                showCameraButton.GetComponentInChildren<Text>().text = "Show Camera";
                //Close the panel when indicator is not active and stop all configuration
                ResetConfigurationWindow();
                configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure";
                configureRobotCameraButton.SetActive(false);

                robotCameraManager.ArrowsActive = false;
            }
            CameraIndicator.SetActive(indicatorActive);
        }

        /// <summary>
        /// Activate the configure camera panel and start position configuration (which is the main configuration state for robot camera)
        /// </summary>
        public void ToggleCameraConfiguration()
        {
            robotCameraManager.ChangingCameraPosition = !robotCameraManager.ChangingCameraPosition;
            configureCameraPanel.SetActive(robotCameraManager.ChangingCameraPosition);
            if (robotCameraManager.ChangingCameraPosition)
            {
                preConfigCamState = dynamicCamera.ActiveState;
                dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera, robotCameraManager.CurrentCamera));
                //Update the node where current camera is attached to
                cameraNodeText.text = "Current Node: " + robotCameraManager.CurrentCamera.transform.parent.gameObject.name;
                configureRobotCameraButton.GetComponentInChildren<Text>().text = "End";

                robotCameraManager.ArrowsActive = true;
            }
            else
            {
                configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure";
                ResetConfigurationWindow();
                dynamicCamera.SwitchToState(preConfigCamState);

                robotCameraManager.ArrowsActive = false;
            }
        }

        /// <summary>
        /// Going into the state of selecting a new node and confirming it
        /// </summary>
        public void ToggleChangeNode()
        {
            if (!robotCameraManager.SelectingNode && robotCameraManager.SelectedNode == null)
            {
                robotCameraManager.DefineNode(); //Start selecting a new node, set SelectingNode to true
                changeCameraNodeButton.GetComponentInChildren<Text>().text = "Confirm";
                cancelNodeSelectionButton.SetActive(true);
            }
            else if (robotCameraManager.SelectingNode && robotCameraManager.SelectedNode != null)
            {
                //Change the node where camera is attached to, clear selected node, and update name of current node
                robotCameraManager.ChangeNodeAttachment();
                cameraNodeText.text = "Current Node: " + robotCameraManager.CurrentCamera.transform.parent.gameObject.name;
                changeCameraNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
                cancelNodeSelectionButton.SetActive(false);

            }
        }

        /// <summary>
        /// Exit the state of selecting node attachment
        /// </summary>
        public void CancelNodeSelection()
        {
            robotCameraManager.ResetNodeColors();
            robotCameraManager.SelectedNode = null;
            robotCameraManager.SelectingNode = false;
            cameraNodeText.text = "Current Node: " + robotCameraManager.CurrentCamera.transform.parent.gameObject.name;
            changeCameraNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
            cancelNodeSelectionButton.SetActive(false);
        }

        /// <summary>
        /// Update the local angle of the current camera to the camera angle panel
        /// </summary>
        public void UpdateCameraAnglePanel()
        {
            if (!isEditingAngle && robotCameraManager.CurrentCamera != null)
            {
                xAngleEntry.GetComponent<InputField>().text = robotCameraManager.CurrentCamera.transform.localEulerAngles.x.ToString();
                yAngleEntry.GetComponent<InputField>().text = robotCameraManager.CurrentCamera.transform.localEulerAngles.y.ToString();
                zAngleEntry.GetComponent<InputField>().text = robotCameraManager.CurrentCamera.transform.localEulerAngles.z.ToString();
            }
        }

        /// <summary>
        /// Take the angle input and set the rotation of the current camera
        /// </summary>
        public void SyncCameraAngle()
        {
            float xTemp = 0;
            float yTemp = 0;
            float zTemp = 0;
            if (float.TryParse(xAngleEntry.GetComponent<InputField>().text, out xTemp) &&
                float.TryParse(yAngleEntry.GetComponent<InputField>().text, out yTemp) &&
                float.TryParse(zAngleEntry.GetComponent<InputField>().text, out zTemp))
            {
                robotCameraManager.CurrentCamera.transform.localRotation = Quaternion.Euler(new Vector3(xTemp, yTemp, zTemp));
            }

        }

        /// <summary>
        /// Control the button that toggles camera angle panel
        /// </summary>
        public void ToggleCameraAnglePanel()
        {
            robotCameraManager.IsShowingAngle = !robotCameraManager.IsShowingAngle;
            cameraAnglePanel.SetActive(robotCameraManager.IsShowingAngle);

            lockPositionButton.SetActive(robotCameraManager.IsShowingAngle);
            lockFOVButton.SetActive(robotCameraManager.IsShowingAngle);

            if (robotCameraManager.IsShowingAngle)
            {
                showAngleButton.GetComponentInChildren<Text>().text = "Hide Camera Angle";
            }
            else
            {
                showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Camera Angle";
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
                SyncCameraAngle();
                isEditingAngle = false;
            }
        }

        /// <summary>
        /// Update the local FOV of the current camera to the camera FOV panel
        /// </summary>
        public void UpdateCameraFOVPanel()
        {
            if (!isEditingFOV && robotCameraManager.CurrentCamera != null)
            {
                FOVEntry.GetComponent<InputField>().text = robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().fieldOfView.ToString();
            }
        }

        /// <summary>
        /// Take the FOV input and set the FOV of the current camera
        /// </summary>
        public void SyncCameraFOV()
        {
            float temp = 0;
            if ((float.TryParse(FOVEntry.GetComponent<InputField>().text, out temp) && temp >= 0))
            {
                robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().fieldOfView = temp;
            }
            else
            {
                robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().fieldOfView = temp;
            }
        }

        /// <summary>
        /// Control the button that toggles camera FOV panel
        /// </summary>
        public void ToggleCameraFOVPanel()
        {
            robotCameraManager.IsChangingFOV = !robotCameraManager.IsChangingFOV;
            cameraFOVPanel.SetActive(robotCameraManager.IsChangingFOV);

            lockPositionButton.SetActive(robotCameraManager.IsChangingFOV);
            lockAngleButton.SetActive(robotCameraManager.IsChangingFOV);

            if (robotCameraManager.IsChangingFOV)
            {
                showFOVButton.GetComponentInChildren<Text>().text = "Hide Camera FOV";
            }
            else
            {
                showFOVButton.GetComponentInChildren<Text>().text = "Show/Edit Camera FOV";
            }
        }

        /// <summary>
        /// Toggle FOV edit mode and update FOV from the input
        /// </summary>
        public void ToggleEditFOV()
        {
            isEditingFOV = !isEditingFOV;
            if (isEditingFOV)
            {
                editFOVButton.GetComponentInChildren<Text>().text = "Done";
            }
            else
            {
                editFOVButton.GetComponentInChildren<Text>().text = "Edit";
                SyncCameraFOV();
                isEditingFOV = false;
            }
        }

        /// <summary>
        /// Update the text indicating attachment node for current camera
        /// </summary>
        public void UpdateNodeAttachment()
        {
            if (robotCameraManager.CurrentCamera != null)
                cameraNodeText.text = "Current Node: " + robotCameraManager.CurrentCamera.transform.parent.gameObject.name;
        }

        /// <summary>
        /// Update transform of robot camera indicator to follow the current camera
        /// </summary>
        public void UpdateIndicatorTransform()
        {
            CameraIndicator.transform.position = robotCameraManager.CurrentCamera.transform.position;
            CameraIndicator.transform.rotation = robotCameraManager.CurrentCamera.transform.rotation;
            CameraIndicator.transform.parent = robotCameraManager.CurrentCamera.transform;
        }

        /// <summary>
        /// Reset configuration window and configuration settings to its default state (nothing is changing), change the camera back
        /// </summary>
        public void ResetConfigurationWindow()
        {
            //Change the dynamic camera back to its original state
            if (robotCameraManager.ChangingCameraPosition) dynamicCamera.SwitchToState(preConfigCamState);

            //Cancel configuration changes and node selection process
            CancelNodeSelection();
            robotCameraManager.ResetConfigurationState();

            //Reset all gui stuff
            lockPositionButton.SetActive(false);
            lockAngleButton.SetActive(false);
            lockFOVButton.SetActive(false);

            showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Camera Angle";
            showFOVButton.GetComponentInChildren<Text>().text = "Show/Edit Camera Range";

            cameraAnglePanel.SetActive(false);
            cameraFOVPanel.SetActive(false);
            configureCameraPanel.SetActive(false);
        }

        /// <summary>
        /// Ends all processes related to robot camera
        /// </summary>
        public void EndProcesses()
        {
            ResetConfigurationWindow();
            if (indicatorActive)
            {
                ToggleCameraIndicator();
            }
            if (usingRobotView)
            {
                ToggleCameraWindow();
            }
            robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().targetTexture = null;
            robotCameraManager.CurrentCamera.SetActive(false);
        }
        #endregion
    }
}
