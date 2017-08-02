using UnityEngine;
using System.Collections.Generic;
using BulletUnity;
using BulletSharp;
using UnityEngine.UI;
using Assets.Scripts.FSM;


public class RobotCamera : MonoBehaviour
{
    public List<GameObject> robotCameraList = new List<GameObject>();
    public GameObject CurrentCamera { get; set; }
    public GameObject CameraIndicator;
    public float FOV;

    private GameObject robotCameraListObject;
    public GameObject SelectedNode;
    public bool SelectingNode { get; set; }


    private static float positionSpeed = 0.5f;
    private static float rotationSpeed = 25;

    //GUI stuff
    MainState main;
    GameObject canvas;

    GameObject cameraAnglePanel;
    GameObject xAngleEntry;
    GameObject yAngleEntry;
    GameObject zAngleEntry;
    GameObject showAngleButton;
    GameObject editAngleButton;

    GameObject cameraFOVPanel;
    GameObject editFOVButton;
    GameObject showFOVButton;
    GameObject FOVEntry;

    GameObject robotCameraViewWindow;
    RenderTexture robotCameraView;
    RobotCamera robotCamera;

    GameObject robotCameraIndicator;
    GameObject showCameraButton;
    GameObject configureRobotCameraButton;
    GameObject cameraConfigurationModeButton;
    GameObject changeCameraNodeButton;
    GameObject configureCameraPanel;
    GameObject cancelNodeSelectionButton;

    GameObject lockPositionButton;
    GameObject lockAngleButton;
    GameObject lockFOVButton;

    Text cameraNodeText;

    private bool usingRobotView = false;
    private bool indicatorActive = false;
    private bool changingCameraPosition;
    private bool isChangingHeight;
    private bool isShowingAngle;
    private bool isEditingAngle;
    private bool isChangingFOV;
    private bool isEditingFOV;

    /// <summary>
    /// Switching between different cameras on robot given the specific camera
    /// </summary>
    /// <param name="robotCamera"></param> the specific camera on the robot camera list
    public void SwitchCamera(GameObject robotCamera)
    {
        CurrentCamera.SetActive(false);
        CurrentCamera = robotCamera;
        CurrentCamera.SetActive(true);
    }

    /// <summary>
    /// Switch to the next camera on the list
    /// </summary>
    /// <param name="currentCamera"></param>
    public void ToggleCamera()
    {
        SwitchCamera(robotCameraList[(robotCameraList.IndexOf(CurrentCamera) + 1) % robotCameraList.Count]);
        //CameraIndicator.SetActive(CurrentCamera.activeSelf);
    }

    /// <summary>
    /// Add a new camera to the robot
    /// </summary>
    /// <param name="anchor"></param> The robot node to which the camera attaches
    /// <param name="positionOffset"></param> 
    /// <param name="rotationOffset"></param>
    /// <returns></returns>
    public GameObject AddCamera(Transform anchor, Vector3 positionOffset, Vector3 rotationOffset)
    {
        GameObject newCamera = new GameObject("RobotCamera_" + robotCameraList.Count);
        newCamera.AddComponent<Camera>();
        newCamera.transform.parent = anchor;
        newCamera.transform.localPosition = positionOffset;
        newCamera.transform.localRotation = Quaternion.Euler(rotationOffset);

        newCamera.SetActive(false);
        if (robotCameraList.Count == 0)
            CurrentCamera = newCamera;

        robotCameraList.Add(newCamera);

        return newCamera;
    }

    /// <summary>
    /// Add a new camera to the robot using the default position and rotation
    /// </summary>
    /// <returns></returns>
    public GameObject AddCamera()
    {
        GameObject newCamera = new GameObject("RobotCamera_" + robotCameraList.Count);
        newCamera.AddComponent<Camera>();
        newCamera.transform.localPosition = new Vector3(0f, 0f, 0f);
        newCamera.transform.localRotation = Quaternion.identity;
        newCamera.SetActive(false);
        if (robotCameraList.Count == 0)
            CurrentCamera = newCamera;

        robotCameraList.Add(newCamera);
        CurrentCamera = newCamera;
        return newCamera;
    }

    public void RemoveCameras()
    {
        foreach (GameObject robotCamera in robotCameraList)
        {
            Destroy(robotCamera);
        }
        CurrentCamera = null;
        robotCameraList.Clear();
    }

    /// <summary>
    /// Return true if the current camera is the last on the list
    /// </summary>
    /// <returns></returns> true if the current camera is the last on the list
    public bool IsLastCamera()
    {
        return robotCameraList.IndexOf(CurrentCamera) == robotCameraList.Count - 1;
    }

    public List<GameObject> GetRobotCameraList()
    {
        return robotCameraList;
    }

    public void Start()
    {
        FindGUIElements();
    }

    public void Update()
    {
        if (main == null)
        {
            main = GameObject.Find("StateMachine").GetComponent<StateMachine>().CurrentState as MainState;
        }
        else
        {
            UpdateCameraWindow();
        }
        if (CameraIndicator.activeSelf)
        {
            CameraIndicator.transform.position = CurrentCamera.transform.position;
            CameraIndicator.transform.rotation = CurrentCamera.transform.rotation;

            CameraIndicator.transform.parent = CurrentCamera.transform;
        }

        //Enable selecting node state, and users can left click on a node to choose it
        if (SelectingNode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetNode();
                Debug.Log("Selecting node");

            }
        }

        ConfigurateCameraPosition();
        UpdateCameraAnglePanel();
        UpdateCameraFOVPanel();
    }


    /// <summary>
    /// Initialize robot node selection
    /// </summary>
    public void DefineNode()
    {
        UserMessageManager.Dispatch("Click on a robot node to set it as the attachment node", 5);
        SelectingNode = true;
        //SelectedNode = null;
    }

    /// <summary>
    /// Set the node where the camera will be attached to
    /// </summary>
    public void SetNode()
    {
        //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
        BulletSharp.Math.Vector3 end = ray.GetPoint(200).ToBullet();

        //Creates a callback result that will be updated if we do a ray test with it
        ClosestRayResultCallback rayResult = new ClosestRayResultCallback(ref start, ref end);

        //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(start, end, rayResult);

        Debug.Log("Selected:" + rayResult.CollisionObject);
        //If there is a collision object and it is a robot part, set that to be new attachment point
        if (rayResult.CollisionObject != null)
        {
            GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;
            if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
            {
                string name = selectedObject.name;

                SelectedNode = selectedObject;

                UserMessageManager.Dispatch(name + " has been selected as the node for camera attachment", 5);
            }
            else
            {
                UserMessageManager.Dispatch("Please select a robot node", 3);
            }
        }
    }

    /// <summary>
    /// Update the attachment point to be the selected node and toggle the state back
    /// </summary>
    public void ChangeNodeAttachment()
    {
        CurrentCamera.transform.parent = SelectedNode.transform;
        SelectingNode = false;
        SelectedNode = null;
    }

    /// <summary>
    /// Use WASD change the position, rotation of camera
    /// </summary>
    private void ConfigurateCameraPosition()
    {
        if (changingCameraPosition)
        {
            if (isChangingFOV)
            {
                CurrentCamera.GetComponent<Camera>().fieldOfView += Input.GetAxis("CameraVertical");
            }
            else if (isShowingAngle) //Control rotation (only when the angle panel is active)
            {
                CurrentCamera.transform.Rotate(new Vector3(-Input.GetAxis("CameraVertical") * rotationSpeed, Input.GetAxis("CameraHorizontal") * rotationSpeed, 0) * Time.deltaTime);
            }
            else if (!isChangingHeight) //Control horizontal plane transform
            {
                CurrentCamera.transform.Translate(new Vector3(Input.GetAxis("CameraHorizontal") * positionSpeed, 0, Input.GetAxis("CameraVertical") * positionSpeed) * Time.deltaTime);
            }
            else //Control height transform
            {
                CurrentCamera.transform.Translate(new Vector3(0, Input.GetAxis("CameraVertical") * positionSpeed, 0) * Time.deltaTime);
            }
        }
    }

    #region robot camera GUI functions

    /// <summary>
    /// Find all robot camera related GUI elements in the canvas
    /// </summary>
    public void FindGUIElements()
    {
        canvas = GameObject.Find("Canvas");

        //For robot camera view window
        robotCameraView = Resources.Load("Images/RobotCameraView") as RenderTexture;
        robotCameraViewWindow = AuxFunctions.FindObject(canvas, "RobotCameraPanel");
        robotCameraViewWindow = AuxFunctions.FindObject(canvas, "RobotCameraPanelBorder");

        //For camera indicator
        robotCameraListObject = GameObject.Find("RobotCameraList");
        if (CameraIndicator == null)
        {
            CameraIndicator = AuxFunctions.FindObject(robotCameraListObject, "CameraIndicator");
        }
        showCameraButton = AuxFunctions.FindObject(canvas, "ShowCameraButton");

        //For camera position and attachment configuration
        configureRobotCameraButton = AuxFunctions.FindObject(canvas, "CameraConfigurationButton");
        changeCameraNodeButton = AuxFunctions.FindObject(canvas, "ChangeNodeButton");
        configureCameraPanel = AuxFunctions.FindObject(canvas, "CameraConfigurationPanel");
        cameraConfigurationModeButton = AuxFunctions.FindObject(canvas, "ConfigurationMode");
        cameraNodeText = AuxFunctions.FindObject(canvas, "NodeText").GetComponent<Text>();
        cancelNodeSelectionButton = AuxFunctions.FindObject(canvas, "CancelNodeSelectionButton");

        //For camera angle configuration
        cameraAnglePanel = AuxFunctions.FindObject(canvas, "CameraAnglePanel");
        xAngleEntry = AuxFunctions.FindObject(cameraAnglePanel, "xAngleEntry");
        yAngleEntry = AuxFunctions.FindObject(cameraAnglePanel, "yAngleEntry");
        zAngleEntry = AuxFunctions.FindObject(cameraAnglePanel, "zAngleEntry");
        showAngleButton = AuxFunctions.FindObject(configureCameraPanel, "ShowCameraAngleButton");
        editAngleButton = AuxFunctions.FindObject(cameraAnglePanel, "EditButton");

        //For field of view configuration
        cameraFOVPanel = AuxFunctions.FindObject(canvas, "CameraFOVPanel");
        FOVEntry = AuxFunctions.FindObject(cameraFOVPanel, "FOVEntry");
        showFOVButton = AuxFunctions.FindObject(configureCameraPanel, "ShowCameraFOVButton");
        editFOVButton = AuxFunctions.FindObject(cameraFOVPanel, "EditButton");

        lockPositionButton = AuxFunctions.FindObject(configureCameraPanel, "LockPositionButton");
        lockAngleButton = AuxFunctions.FindObject(configureCameraPanel, "LockAngleButton");
        lockFOVButton = AuxFunctions.FindObject(configureCameraPanel, "LockFOVButton");
    }
    /// <summary>
    /// Updates the robot camera view window
    /// </summary>
    private void UpdateCameraWindow()
    {
        //Can use robot view when dynamicCamera is active
        if (usingRobotView && main.dynamicCameraObject.activeSelf)
        {
            //Debug.Log(CurrentCamera);

            //Make sure there is camera on robot
            if (CurrentCamera != null)
            {
                CurrentCamera.SetActive(true);
                CurrentCamera.GetComponent<Camera>().targetTexture = robotCameraView;
                //Toggle the robot camera using Q (should be changed later)
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    CurrentCamera.GetComponent<Camera>().targetTexture = null;
                    ToggleCamera();
                    CurrentCamera.GetComponent<Camera>().targetTexture = robotCameraView;

                }
                //Debug.Log("Robot camera view is " + robotCameraView.name);
                //Debug.Log(CurrentCamera);
            }
        }
        //Don't allow using robot view window when users are currently using one of the robot view
        else if (usingRobotView && !main.dynamicCameraObject.activeSelf)
        {
            UserMessageManager.Dispatch("You can only use robot view window when you are not in robot view mode!", 2f);
            usingRobotView = false;
            robotCameraViewWindow.SetActive(false);
        }
        //Free the target texture of the current camera when the window is closed (for normal toggle camera function)
        else
        {
            if (CurrentCamera != null)
                CurrentCamera.GetComponent<Camera>().targetTexture = null;
        }

    }

    /// <summary>
    /// Toggles the state of usingRobotView when the button "Toggle Robot Camera" is clicked
    /// </summary>
    public void ToggleCameraWindow()
    {
        usingRobotView = !usingRobotView;
        robotCameraViewWindow.SetActive(usingRobotView);
        if (usingRobotView)
        {
            CurrentCamera.GetComponent<Camera>().targetTexture = robotCameraView;
        }
        else
        {
            //Free the target texture and disable the camera since robot camera has more depth than main camera
            CurrentCamera.GetComponent<Camera>().targetTexture = null;
            CurrentCamera.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the state of showing or hiding the robot indicator
    /// </summary>
    public void ToggleCameraIndicator()
    {
        indicatorActive = !indicatorActive;
        if (indicatorActive)
        {
            //Only allow the camera configuration when the indicator is active
            showCameraButton.GetComponentInChildren<Text>().text = "Hide Camera";
            configureRobotCameraButton.SetActive(true);
        }
        else
        {
            showCameraButton.GetComponentInChildren<Text>().text = "Show Camera";
            configureRobotCameraButton.SetActive(false);
            //Close the panel when indicator is not active and stop all configuration
            configureCameraPanel.SetActive(false);
            isChangingHeight = SelectingNode = changingCameraPosition = false;
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure Robot Camera";
            SelectingNode = false;
            SelectedNode = null;
        }
        CameraIndicator.SetActive(indicatorActive);
    }

    /// <summary>
    /// Activate the configure camera panel and start position configuration
    /// </summary>
    public void ConfigureCameraPosition()
    {
        changingCameraPosition = !changingCameraPosition;
        configureCameraPanel.SetActive(changingCameraPosition);
        if (changingCameraPosition)
        {
            //Update the node where current camera is attached to
            cameraNodeText.text = "Current Node: " + CurrentCamera.transform.parent.gameObject.name;
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "End Configuration";
        }
        else
        {
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure Robot Camera";
        }
    }

    /// <summary>
    /// Toggle between changing position along horizontal plane or changing height
    /// </summary>
    public void ToggleConfigurationMode()
    {
        isChangingHeight = !isChangingHeight;
        if (isChangingHeight)
        {
            cameraConfigurationModeButton.GetComponentInChildren<Text>().text = "Configure Horizontal Plane";
        }
        else
        {
            cameraConfigurationModeButton.GetComponentInChildren<Text>().text = "Configure Height";
        }
        CurrentCamera.GetComponent<Camera>().fieldOfView = FOV;
    }

    /// <summary>
    /// Going into the state of selecting a new node and confirming it
    /// </summary>
    public void ToggleChangeNode()
    {
        if (!SelectingNode && SelectedNode == null)
        {
            DefineNode(); //Start selecting a new node
            changeCameraNodeButton.GetComponentInChildren<Text>().text = "Confirm";
            cancelNodeSelectionButton.SetActive(true);
        }
        else if (SelectingNode && SelectedNode != null)
        {
            //Change the node where camera is attached to, clear selected node, and update name of current node
            ChangeNodeAttachment();
            cameraNodeText.text = "Current Node: " + CurrentCamera.transform.parent.gameObject.name;
            changeCameraNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
            cancelNodeSelectionButton.SetActive(false);

        }
    }

    /// <summary>
    /// Exit the state of selecting node attachment
    /// </summary>
    public void CancelNodeSelection()
    {
        SelectedNode = null;
        SelectingNode = false;
        cameraNodeText.text = "Current Node: " + CurrentCamera.transform.parent.gameObject.name;
        changeCameraNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
        cancelNodeSelectionButton.SetActive(false);
    }

    /// <summary>
    /// Update the local angle of the current camera to the camera angle panel
    /// </summary>
    public void UpdateCameraAnglePanel()
    {
        if (!isEditingAngle)
        {
            xAngleEntry.GetComponent<InputField>().text = CurrentCamera.transform.localEulerAngles.x.ToString();
            yAngleEntry.GetComponent<InputField>().text = CurrentCamera.transform.localEulerAngles.y.ToString();
            zAngleEntry.GetComponentInChildren<Text>().text = CurrentCamera.transform.localEulerAngles.z.ToString();
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
            //Debug.Log("Sync angle!");
            CurrentCamera.transform.localRotation = Quaternion.Euler(new Vector3(xTemp, yTemp, zTemp));
        }

    }

    /// <summary>
    /// Control the button that toggles camera angle panel
    /// </summary>
    public void ToggleCameraAnglePanel()
    {
        isShowingAngle = !isShowingAngle;
        cameraAnglePanel.SetActive(isShowingAngle);

        lockPositionButton.SetActive(isShowingAngle);
        lockFOVButton.SetActive(isShowingAngle);

        if (isShowingAngle)
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
    /// Update the local FOV of the current camera to the camera angle panel
    /// </summary>
    public void UpdateCameraFOVPanel()
    {
        if (!isEditingFOV)
        {
            FOVEntry.GetComponent<InputField>().text = CurrentCamera.GetComponent<Camera>().fieldOfView.ToString();
        }
    }

    /// <summary>
    /// Take the FOV input and set the rotation of the current camera
    /// </summary>
    public void SyncCameraFOV()
    {
        float temp = 0;
        if ((float.TryParse(FOVEntry.GetComponent<InputField>().text, out temp) && temp >= 0))
        {
            CurrentCamera.GetComponent<Camera>().fieldOfView = temp;
        }
        else
        {
            CurrentCamera.GetComponent<Camera>().fieldOfView = temp;
        }

    }

    /// <summary>
    /// Control the button that toggles camera FOV panel
    /// </summary>
    public void ToggleCameraFOVPanel()
    {
        isChangingFOV = !isChangingFOV;
        cameraFOVPanel.SetActive(isChangingFOV);

        lockPositionButton.SetActive(isChangingFOV);
        lockAngleButton.SetActive(isChangingFOV);

        if (isChangingFOV)
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
    #endregion


}