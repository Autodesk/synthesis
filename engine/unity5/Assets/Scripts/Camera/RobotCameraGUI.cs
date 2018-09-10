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

        bool changingAngle = false;
        bool changingAngleX = false;
        bool changingAngleY = false;
        bool changingAngleZ = false;
        float angleIncrement = 1f;
        int angleSign;

        //FOV panel
        GameObject cameraFOVPanel;
        GameObject showFOVButton;
        GameObject FOVEntry;

        bool changingFOV = false;
        float fovIncrement = 1f;
        int fovSign;


        GameObject robotCameraViewWindow;
        RenderTexture robotCameraView;

        //The indicator object is originally under robot camera list in unity scene
        public GameObject CameraIndicator;

        //Camera configuration
        GameObject configureRobotCameraButton;
        GameObject configureCameraPanel;

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
                    UpdateIndicatorTransform();
                }
            }

            //Allows users to save their configuration using enter
            if (isEditingAngle && UnityEngine.Input.GetKeyDown(KeyCode.Return)) ToggleEditAngle();
            if (isEditingFOV && UnityEngine.Input.GetKeyDown(KeyCode.Return)) ToggleEditFOV();

            //If an increment button is held, increment fov or angle
            if (changingFOV)
            {
                robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().fieldOfView =
                    robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().fieldOfView + fovIncrement * fovSign;
                UpdateCameraFOVPanel();
            }
            else if (robotCameraManager.IsChangingFOV) SyncCameraFOV();
            if (changingAngle)
            {
                if (changingAngleX) robotCameraManager.RotateTransform(angleIncrement * angleSign, 0, 0);
                else if (changingAngleY) robotCameraManager.RotateTransform(0, angleIncrement * angleSign, 0);
                else if (changingAngleZ) robotCameraManager.RotateTransform(0, 0, angleIncrement * angleSign);
                UpdateCameraAnglePanel();
            }
            else if (robotCameraManager.IsShowingAngle) SyncCameraAngle();
        }

        #region robot camera GUI functions

        /// <summary>
        /// Find all robot camera related GUI elements in the canvas
        /// </summary>
        private void FindGUIElements()
        {
            canvas = GameObject.Find("Canvas");
            sensorManagerGUI = GetComponent<SensorManagerGUI>();

            //For robot camera view window
            robotCameraView = Resources.Load("Images/Old Assets/RobotCameraView") as RenderTexture;
            robotCameraViewWindow = Auxiliary.FindObject(canvas, "RobotCameraPanelBorder");

            //For robot camera manager
            robotCameraListObject = GameObject.Find("RobotCameraList");
            robotCameraManager = robotCameraListObject.GetComponent<RobotCameraManager>();

            //For camera indicator
            if (CameraIndicator == null)
            {
                CameraIndicator = Auxiliary.FindObject(robotCameraListObject, "CameraIndicator");
            }

            //For camera position and attachment configuration
            configureCameraPanel = Auxiliary.FindObject(canvas, "CameraConfigurationPanel");
            configureRobotCameraButton = Auxiliary.FindObject(canvas, "CameraConfigurationButton");

            //For camera angle configuration
            cameraAnglePanel = Auxiliary.FindObject(canvas, "CameraAnglePanel");
            xAngleEntry = Auxiliary.FindObject(cameraAnglePanel, "xAngleEntry1");
            yAngleEntry = Auxiliary.FindObject(cameraAnglePanel, "yAngleEntry1");
            zAngleEntry = Auxiliary.FindObject(cameraAnglePanel, "zAngleEntry1");

            //For field of view configuration
            cameraFOVPanel = Auxiliary.FindObject(configureCameraPanel, "CameraFOVPanel");
            FOVEntry = Auxiliary.FindObject(cameraFOVPanel, "FOVEntry");
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
                }
            }
            //Free the target texture of the current camera when the window is closed (for normal toggle camera function)
            else if (robotCameraManager.CurrentCamera != null) robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().targetTexture = null;
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
            CameraIndicator.SetActive(indicatorActive);
            Auxiliary.FindObject(configureCameraPanel, "VisibilityButton").GetComponentInChildren<Text>().text = indicatorActive ? "Hide" : "Show";
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
            }
            else
            {
                configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure";
                ResetConfigurationWindow();
                dynamicCamera.SwitchToState(preConfigCamState);
            }
        }

        /// <summary>
        /// Toggle the state of changing the camera position with move arrows
        /// </summary>
        public void ToggleChangePosition()
        {
            StateMachine.SceneGlobal.PushState(new SensorSpawnState(robotCameraManager.CurrentCamera), true);
        }

        /// <summary>
        /// Toggle the state of selecting a new node and confirming it
        /// </summary>
        public void ToggleChangeNode()
        {
            StateMachine.SceneGlobal.PushState(new DefineSensorAttachmentState(robotCameraManager), true);
        }

        /// <summary>
        /// Update the local angle of the current camera to the camera angle panel
        /// </summary>
        private void UpdateCameraAnglePanel()
        {
            if (robotCameraManager.CurrentCamera != null)
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
            xAngleEntry.GetComponent<InputField>().text = xAngleEntry.GetComponent<InputField>().text.TrimStart('0');
            yAngleEntry.GetComponent<InputField>().text = yAngleEntry.GetComponent<InputField>().text.TrimStart('0');
            zAngleEntry.GetComponent<InputField>().text = zAngleEntry.GetComponent<InputField>().text.TrimStart('0');
            if (!float.TryParse(xAngleEntry.GetComponent<InputField>().text, out xTemp)) xAngleEntry.GetComponent<InputField>().text = "0";
            if (!float.TryParse(yAngleEntry.GetComponent<InputField>().text, out yTemp)) yAngleEntry.GetComponent<InputField>().text = "0";
            if (!float.TryParse(zAngleEntry.GetComponent<InputField>().text, out zTemp)) zAngleEntry.GetComponent<InputField>().text = "0";
            robotCameraManager.CurrentCamera.transform.localRotation = Quaternion.Euler(new Vector3(xTemp, yTemp, zTemp));
        }

        /// <summary>
        /// Control the button that toggles camera angle panel
        /// </summary>
        public void ToggleCameraAnglePanel()
        {
            robotCameraManager.IsShowingAngle = !robotCameraManager.IsShowingAngle;
            cameraAnglePanel.SetActive(robotCameraManager.IsShowingAngle);
            isEditingAngle = robotCameraManager.IsShowingAngle;

            //if (robotCameraManager.IsShowingAngle)
            //{
            //    showAngleButton.GetComponentInChildren<Text>().text = "Hide Camera Angle";
            //}
            //else
            //{
            //    showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Camera Angle";
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
            }
            else
            {
                SyncCameraAngle();
                isEditingAngle = false;
            }
        }

        /// <summary>
        /// Start changing camera x angle
        /// </summary>
        /// <param name="sign"></param>
        public void ChangeCameraAngleX(int sign)
        {
            angleSign = sign;
            changingAngleX = true;
            changingAngle = true;
        }

        /// <summary>
        /// Start changing camera y angle
        /// </summary>
        /// <param name="sign"></param>
        public void ChangeCameraAngleY(int sign)
        {
            angleSign = sign;
            changingAngleY = true;
            changingAngle = true;
        }

        /// <summary>
        /// Start changing camera z angle
        /// </summary>
        /// <param name="sign"></param>
        public void ChangeCameraAngleZ(int sign)
        {
            angleSign = sign;
            changingAngleZ = true;
            changingAngle = true;
        }

        /// <summary>
        /// Stop changing camera angle (called when +/- button is released)
        /// </summary>
        public void StopChangingCameraAngle()
        {
            changingAngleX = false;
            changingAngleY = false;
            changingAngleZ = false;
            changingAngle = false;
        }

        /// <summary>
        /// Update the local FOV of the current camera to the camera FOV panel
        /// </summary>
        public void UpdateCameraFOVPanel()
        {
            if (robotCameraManager.CurrentCamera != null)
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
            FOVEntry.GetComponent<InputField>().text = FOVEntry.GetComponent<InputField>().text.TrimStart('0');
            if (!(float.TryParse(FOVEntry.GetComponent<InputField>().text, out temp) || temp < 0)) { temp = 0; FOVEntry.GetComponent<InputField>().text = "0"; }
            robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().fieldOfView = temp;
        }

        /// <summary>
        /// Start changing camera fov
        /// </summary>
        /// <param name="sign"></param>
        public void ChangeFOV(int sign)
        {
            fovSign = sign;
            changingFOV = true;
        }

        /// <summary>
        /// Stop changing camera fov (called when +/- button is released)
        /// </summary>
        public void StopChangingFOV()
        {
            changingFOV = false;
        }

        /// <summary>
        /// Control the button that toggles camera FOV panel
        /// </summary>
        public void ToggleCameraFOVPanel()
        {
            FOVEntry.GetComponent<InputField>().text = robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().fieldOfView.ToString();
            robotCameraManager.IsChangingFOV = !robotCameraManager.IsChangingFOV;
            cameraFOVPanel.SetActive(robotCameraManager.IsChangingFOV);
            isEditingFOV = robotCameraManager.IsChangingFOV;
        }

        /// <summary>
        /// Toggle FOV edit mode and update FOV from the input
        /// </summary>
        public void ToggleEditFOV()
        {
            isEditingFOV = !isEditingFOV;
            if (isEditingFOV)
            {
            }
            else
            {
                SyncCameraFOV();
                isEditingFOV = false;
            }
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

            //Cancel configuration changes
            robotCameraManager.ResetConfigurationState();

            //Reset all gui stuff

            //showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Camera Angle";
            //showFOVButton.GetComponentInChildren<Text>().text = "Show/Edit Camera Range";

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

        public void ResetAngle()
        {
            robotCameraManager.CurrentCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
            UpdateCameraAnglePanel();
        }

        public void ResetFOV()
        {
            robotCameraManager.CurrentCamera.GetComponent<UnityEngine.Camera>().fieldOfView = 60;
            UpdateCameraFOVPanel();
        }
        #endregion
    }
}
