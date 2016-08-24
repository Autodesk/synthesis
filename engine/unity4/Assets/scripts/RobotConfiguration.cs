using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class RobotConfiguration : MonoBehaviour
{
    // We will need these
    public const float PHYSICS_MASS_MULTIPLIER = 0.001f;

    public const float FORMAT_3DS_SCALE = 0.2558918f;

    private GUIController gui;

    private PhysicMaterial chuteMaterial;

    private RigidNode_Base skeleton;
    private GameObject activeRobot;
    private GameObject activeField;
    private GameObject cameraObject;
    private DynamicCamera dynamicCamera;
    private GameObject light;

    private unityPacket udp;
    private string filePath;
    //main node of robot from which speed and other stats are derived
    private GameObject mainNode;
    //for orienting the robot
    private Quaternion rotation;

    private string fieldName;

    /// <summary>
    /// Frames before the robot gets reloaded, or -1 if no reload is queued.
    /// </summary>
    /// <remarks>
    /// This allows reloading the robot to be delayed until a "Loading" dialog can be drawn.
    /// </remarks>
    private volatile int reloadRobotInFrames;

    //
    private bool driverPraticeOn = false;
    private DriverPracticeMode driverPracticeMode;

    public List<string> dpmNodes;
    public List<string> dpmVectors;
    public List<string> gamepieces;
    public List<GameObject> nodeList;
    public List<Vector3> vectorSave { get; set; }
    public List<string> nodeSave { get; set; }

    public bool editingNode = false;
    public bool editingVector = false;
    public int editingIndex = 0;

    private GameObject editingNodePanel;
    private GameObject editingVectorPanel;
    private ScrollableConfiguration nodeScrollable;

    private List<Color> origColors;
    private GameObject highlightedNode;
    private Color highlightColor;

    public RobotConfiguration()
    {
        udp = new unityPacket();
        Debug.Log(filePath);

        reloadRobotInFrames = 1;
        rotation = Quaternion.identity;

        origColors = new List<Color>();
        highlightColor = new Color(1, 1, 0, 0.1f);
    }

    [STAThread]
    void OnGUI()
    {
        if (editingNode) editingNodePanel.SetActive(true);
        else editingNodePanel.SetActive(false);
        if (editingVector) editingVectorPanel.SetActive(true);
        else editingVectorPanel.SetActive(false);

        UserMessageManager.Render();

    }

    /// <summary>
    /// Repositions the robot so it is aligned at the center of the field, and resets all the
    /// joints, velocities, etc..
    /// </summary>
    private void resetRobot()
    {
        if (activeRobot != null && skeleton != null)
        {
            unityPacket.OutputStatePacket packet = null;
            var unityWheelData = new List<GameObject>();
            var unityWheels = new List<UnityRigidNode>();
            // Invert the position of the root object
            /**/
            activeRobot.transform.localPosition = new Vector3(1f, 1f, -0.5f);
            activeRobot.transform.rotation = Quaternion.identity;
            activeRobot.transform.localRotation = Quaternion.identity;
            mainNode.transform.rotation = Quaternion.identity;
            /**/
            var nodes = skeleton.ListAllNodes();
            foreach (RigidNode_Base node in nodes)
            {
                UnityRigidNode uNode = (UnityRigidNode)node;
                uNode.unityObject.transform.localPosition = new Vector3(0f, 0f, -5f);
                uNode.unityObject.transform.localRotation = Quaternion.identity;
                if (uNode.unityObject.rigidbody != null)
                {
                    uNode.unityObject.rigidbody.velocity = Vector3.zero;
                    uNode.unityObject.rigidbody.angularVelocity = Vector3.zero;
                }
                if (uNode.HasDriverMeta<WheelDriverMeta>() && uNode.wheelCollider != null && uNode.GetDriverMeta<WheelDriverMeta>().isDriveWheel)
                {
                    unityWheelData.Add(uNode.wheelCollider);
                    unityWheels.Add(uNode);
                }
            }
            bool isMecanum = false;

            if (unityWheelData.Count > 0)
            {
                auxFunctions.OrientRobot(unityWheelData, activeRobot.transform);
                foreach (RigidNode_Base node in nodes)
                {
                    UnityRigidNode uNode = (UnityRigidNode)node;
                    if (uNode.HasDriverMeta<WheelDriverMeta>() && uNode.wheelCollider != null)
                    {
                        unityWheelData.Add(uNode.wheelCollider);

                        if (uNode.GetDriverMeta<WheelDriverMeta>().GetTypeString().Equals("Mecanum"))
                        {
                            isMecanum = true;
                            uNode.unityObject.GetComponent<BetterWheelCollider>().wheelType = (int)WheelType.MECANUM;
                        }

                        if (uNode.GetDriverMeta<WheelDriverMeta>().GetTypeString().Equals("Omni Wheel"))
                            uNode.unityObject.GetComponent<BetterWheelCollider>().wheelType = (int)WheelType.OMNI;
                    }
                }
                auxFunctions.rightRobot(unityWheelData, activeRobot.transform);

                if (isMecanum)
                {
                    float sumX = 0;
                    float sumZ = 0;

                    foreach (UnityRigidNode uNode in unityWheels)
                    {
                        sumX += uNode.wheelCollider.transform.localPosition.x;
                        sumZ += uNode.wheelCollider.transform.localPosition.z;
                    }

                    float avgX = sumX / unityWheels.Count;
                    float avgZ = sumZ / unityWheels.Count;

                    foreach (UnityRigidNode uNode in unityWheels)
                    {
                        if (uNode.unityObject.GetComponent<BetterWheelCollider>().wheelType == (int)WheelType.MECANUM)
                        {
                            if ((avgX > uNode.wheelCollider.transform.localPosition.x && avgZ < uNode.wheelCollider.transform.localPosition.z) ||
                               (avgX < uNode.wheelCollider.transform.localPosition.x && avgZ > uNode.wheelCollider.transform.localPosition.z))
                                uNode.unityObject.GetComponent<BetterWheelCollider>().wheelAngle = -45;

                            else
                                uNode.unityObject.GetComponent<BetterWheelCollider>().wheelAngle = 45;
                        }
                    }
                }
            }
            mainNode.transform.rotation = rotation;
            mainNode.rigidbody.inertiaTensorRotation = Quaternion.identity;

            //makes sure robot spawns in the correct place
            mainNode.transform.position = new Vector3(-2f, 1f, -3f);

            foreach(Rigidbody rb in GetComponentsInChildren<Rigidbody>())
            {
                
                nodeList.Add(rb.gameObject);
            }
            mainNode.rigidbody.isKinematic = true;
            StartCoroutine(FinishReset());
        }
    }

    /// <summary>
    /// Loads a robot from file into the simulator.
    /// </summary>
    private void TryLoadRobot()
    {
        //resets rotation for new robot
        rotation = Quaternion.identity;
        if (activeRobot != null)
        {
            skeleton = null;
            UnityEngine.Object.Destroy(activeRobot);
        }
        if (filePath != null && skeleton == null)
        {
            //Debug.Log (filePath);
            List<Collider> meshColliders = new List<Collider>();
            activeRobot = new GameObject("Robot");
            activeRobot.transform.parent = transform;

            List<RigidNode_Base> names = new List<RigidNode_Base>();
            RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
            {
                return new UnityRigidNode(guid);
            };

            skeleton = BXDJSkeleton.ReadSkeleton(filePath + "skeleton.bxdj");
            //Debug.Log(filePath + "skeleton.bxdj");
            skeleton.ListAllNodes(names);
            foreach (RigidNode_Base node in names)
            {
                UnityRigidNode uNode = (UnityRigidNode)node;

                uNode.CreateTransform(activeRobot.transform);

                if (!uNode.CreateMesh(filePath + uNode.ModelFileName))
                {
                    UserMessageManager.Dispatch(node.ModelFileName + " has been modified and cannot be loaded.", 6f);
                    skeleton = null;
                    UnityEngine.Object.Destroy(activeRobot);
                    return;
                }

                uNode.CreateJoint();

                Debug.Log("Joint");

                meshColliders.AddRange(uNode.unityObject.GetComponentsInChildren<Collider>());
            }

            {   // Add some mass to the base object
                UnityRigidNode uNode = (UnityRigidNode)skeleton;
                uNode.unityObject.transform.rigidbody.mass += 20f * PHYSICS_MASS_MULTIPLIER; // Battery'
            }

            //finds main node of robot to use its rigidbody
            mainNode = GameObject.Find("node_0.bxda");

            string robotname = new DirectoryInfo(filePath).Name; //Retrieving the name of the robot folder from the filepath.
            driverPracticeMode = activeRobot.AddComponent<DriverPracticeMode>();
            driverPracticeMode.isConfiguring = true;
            driverPracticeMode.Initialize();
            driverPraticeOn = true;

            //Debug.Log ("HELLO AMIREKA: " + mainNode);
            auxFunctions.IgnoreCollisionDetection(meshColliders);
            resetRobot();
        }
        else
        {
            Debug.Log("unityWheelData is null...");
        }
    }

    void Start()
    {
        editingVectorPanel = auxFunctions.FindObject(GameObject.Find("Canvas"), "VectorPopup");
        editingNodePanel = auxFunctions.FindObject(GameObject.Find("Canvas"), "NodePopup");
        editingVectorPanel.SetActive(false);
        editingNodePanel.SetActive(false);
        nodeScrollable = auxFunctions.FindObject(GameObject.Find("Canvas"),"NodeList").GetComponent<ScrollableConfiguration>();

        Physics.gravity = new Vector3(0, -9.8f, 0);
        Physics.solverIterationCount = 30;
        Physics.minPenetrationForPenalty = 0.001f;

        cameraObject = new GameObject("Camera");
        cameraObject.AddComponent<Camera>();
        cameraObject.GetComponent<Camera>().backgroundColor = new Color(.3f, .3f, .3f);
        dynamicCamera = cameraObject.AddComponent<DynamicCamera>();

        light = new GameObject("Light");
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Spot;
        lightComponent.intensity = 1.5f;
        lightComponent.range = 100f;
        lightComponent.spotAngle = 135;
        light.transform.position = new Vector3(0f, 30f, 0f);
        light.transform.Rotate(90f, 0f, 0f);

        reloadRobotInFrames = 1;
        string line = "";
        int counter = 0;
        StreamReader reader = new StreamReader(PlayerPrefs.GetString("dpmSelectedField")+"\\driverpracticemode.txt");

        while ((line = reader.ReadLine()) != null)
        {
            if (line.Equals("#Name")) counter++;
            else if (counter == 1)
            {
                if (line.Equals("#Nodes")) counter++;
                else
                {
                    fieldName = line;
                    GameObject.Find("ConfiguringText").GetComponent<Text>().text = "Configuring Robot For: " + fieldName;
                }
            }
            else if (counter == 2)
            {
                if (line.Equals("#Vectors")) counter++;
                else dpmNodes.Add(line);
            }
            else if (counter == 3)
            {
                if (line.Equals("#Gamepieces")) counter++;
                else dpmVectors.Add(line);
            }
            else if (counter == 4)
            {
                if (line.Equals("#Holdinglimit")) counter++;
                else gamepieces.Add(line);
            }
        }
        reader.Close();

        nodeSave = new List<string>();
        vectorSave = new List<Vector3>();
        nodeList = new List<GameObject>();

        if (File.Exists(PlayerPrefs.GetString("dpmSelectedRobot") + "\\dpmConfiguration.txt"))
        {
            reader= new StreamReader(PlayerPrefs.GetString("dpmSelectedRobot") + "\\dpmConfiguration.txt");
            line = "";
            counter = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals("#Nodes")) counter++;
                else if (counter == 1)
                {
                    if (line.Equals("#Vectors")) counter++;
                    else
                    {
                        nodeSave.Add(line);
                    }
                }
                else if (counter == 2)
                {
                    if (line.Equals("#Gamepieces")) counter++;
                    else vectorSave.Add(DeserializeVector3Array(line));
                }
            }
            reader.Close();
        }
        else
        {
            for (int i = 0; i < dpmVectors.Count; i++)
            {
                vectorSave.Add(Vector3.zero);
            }
            for (int i = 0; i < dpmNodes.Count; i++)
            {
                nodeSave.Add("node_0.bxda");
            }
        }
    }

    void OnEnable()
    {
        udp.Start();
    }

    void OnDisable()
    {
        udp.Stop();
    }

    void Update()
    {
        if (mainNode == null)
        {
            mainNode = GameObject.Find("node_0.bxda");
            resetRobot();
        }

        //Debug.Log(filePath);
        if (reloadRobotInFrames >= 0 && reloadRobotInFrames-- == 0)
        {
            reloadRobotInFrames = -1;
            filePath = PlayerPrefs.GetString("dpmSelectedRobot");
            TryLoadRobot();
        }

        // Reset Robot
        if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.ResetRobot]))
            resetRobot();
    }

    void FixedUpdate()
    {
        if (skeleton != null)
        {
            List<RigidNode_Base> nodes = skeleton.ListAllNodes();

            unityPacket.OutputStatePacket packet = udp.GetLastPacket();


            DriveJoints.UpdateAllMotors(skeleton, packet.dio);
            //TODO put this code in drivejoints, figure out nullreference problem with cDriver
            foreach (RigidNode_Base node in nodes)
            {
                UnityRigidNode uNode = (UnityRigidNode)node;
                if (uNode.GetSkeletalJoint() != null)
                {
                    if (uNode.GetSkeletalJoint().GetJointType() == SkeletalJointType.LINEAR && uNode.GetSkeletalJoint().cDriver != null && uNode.GetSkeletalJoint().cDriver.GetDriveType() == JointDriverType.ELEVATOR)
                    {
                        ElevatorScript es = uNode.unityObject.GetComponent<ElevatorScript>();

                        float[] pwm = packet.dio[0].pwmValues;
                        if (Input.anyKey)
                        {
                            pwm[3] += (Input.GetKey(KeyCode.Alpha1) ? 1f : 0f);
                            pwm[3] += (Input.GetKey(KeyCode.Alpha2) ? -1f : 0f);
                        }
                        if (es != null)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                if (uNode.GetSkeletalJoint().cDriver.portA == i)
                                    es.currentTorque = pwm[i] * 2;
                            }
                        }
                    }
                }
            }
            DriveJoints.UpdateSolenoids(skeleton, packet.solenoid);

            #region HANDLE_SENSORS
            InputStatePacket sensorPacket = new InputStatePacket();
            foreach (RigidNode_Base node in nodes)
            {
                UnityRigidNode uNode = (UnityRigidNode)node;

                if (node.GetSkeletalJoint() == null)
                    continue;

                foreach (RobotSensor sensor in node.GetSkeletalJoint().attachedSensors)
                {
                    int aiValue; //int between 0 and 1024, typically
                    InputStatePacket.DigitalState dioValue;
                    switch (sensor.type)
                    {
                        case RobotSensorType.POTENTIOMETER:
                            if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint() is RotationalJoint_Base)
                            {
                                float angle = DriveJoints.GetAngleBetweenChildAndParent(uNode) + ((RotationalJoint_Base)uNode.GetSkeletalJoint()).currentAngularPosition;
                                sensorPacket.ai[sensor.module - 1].analogValues[sensor.port - 1] = (int)sensor.equation.Evaluate(angle);
                            }
                            break;

                        case RobotSensorType.ENCODER:
                            throw new NotImplementedException();
                            break;

                        case RobotSensorType.LIMIT:
                            throw new NotImplementedException();
                            break;

                        case RobotSensorType.GYRO:
                            throw new NotImplementedException();
                            break;

                        case RobotSensorType.MAGNETOMETER:
                            throw new NotImplementedException();
                            break;

                        case RobotSensorType.ACCELEROMETER:
                            throw new NotImplementedException();
                            break;

                        case RobotSensorType.LIMIT_HALL:
                            throw new NotImplementedException();
                            break;
                    }

                }
            }
            udp.WritePacket(sensorPacket);
            #endregion
        }
    }
    public void OpenVectorPopup()
    {
        editingVectorPanel.SetActive(true);
        editingVector = true;
        GameObject.Find("xInput").GetComponent<InputField>().text = vectorSave[editingIndex].x.ToString();
        GameObject.Find("yInput").GetComponent<InputField>().text = vectorSave[editingIndex].y.ToString();
        GameObject.Find("zInput").GetComponent<InputField>().text = vectorSave[editingIndex].z.ToString();
    }

    public void SaveVectorPopup()
    {
        float x, y, z;
        if (float.TryParse(GameObject.Find("xInput").GetComponent<InputField>().text, out x) && float.TryParse(GameObject.Find("yInput").GetComponent<InputField>().text, out y) && float.TryParse(GameObject.Find("zInput").GetComponent<InputField>().text, out z))
        {
            vectorSave[editingIndex] = new Vector3(x, y, z);
            editingVector = false;
        }
        else UserMessageManager.Dispatch("Please only enter in numbers!", 5);
        driverPracticeMode.UpdateConfiguration();
    }

    public void OpenNodePopup()
    {
        
        editingNodePanel.SetActive(true);
        editingNode = true;
        nodeScrollable.selectedEntry = nodeSave[editingIndex];
        Debug.Log(nodeSave[editingIndex]);

    }

    public void SaveNodePopup()
    {
        nodeSave[editingIndex] = nodeScrollable.selectedEntry;
        RevertHighlight();
        editingNodePanel.SetActive(false);
        editingNode = false;
        driverPracticeMode.UpdateConfiguration();
    }
    public void HighlightNode()
    {
        RevertHighlight();

        highlightedNode = GameObject.Find(nodeScrollable.selectedEntry);
        foreach (Renderer renderers in highlightedNode.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderers.materials)
            {
                origColors.Add(m.color);
                m.color = highlightColor;
            }
        }
        
    }
    public void RevertHighlight()
    {
        if (highlightedNode != null)
        {
            int counter = 0;
            foreach (Renderer renderers in highlightedNode.GetComponentsInChildren<Renderer>())
            {

                foreach (Material m in renderers.materials)
                {
                    m.color = origColors[counter];
                    counter++;
                }
            }
            origColors.Clear();
        }
        highlightedNode = null;
    }
    public void SpawnGamepiece()
    {
        GameObject.Instantiate(GameObject.Find("gamepiece"),Vector3.zero,Quaternion.identity);
    }

    public void Save()
    {
        if (File.Exists(filePath + "\\dpmConfiguration.txt"))
        {
            File.Delete(filePath + "\\dpmConfiguration.txt");
        }
        using (StreamWriter writer = new StreamWriter(filePath + "\\dpmConfiguration.txt", false))
        {
            writer.WriteLine("#Field");
            writer.WriteLine(fieldName);
            writer.WriteLine("#Nodes");
            foreach (string node in nodeSave)
            {
                writer.WriteLine(node);
            }
            writer.WriteLine("#Vectors");
            foreach (Vector3 vector in vectorSave)
               {
                StringBuilder sb = new StringBuilder();
                writer.WriteLine(sb.Append(vector.x).Append("|").Append(vector.y).Append("|").Append(vector.z));
            }
            writer.WriteLine("#Gamepieces");
            writer.WriteLine(gamepieces[0]);
            writer.Close();
        }
        Exit();
    }

    public void Exit()
    {
        Application.LoadLevel("MainMenu");
    }

    /// <summary>
    /// Waits .1 seconds before making robot move again.
    /// </summary>
    IEnumerator FinishReset()
    {
        yield return new WaitForSeconds(0.1f);
        mainNode.rigidbody.isKinematic = false;
    }

    public static Vector3 DeserializeVector3Array(string aData)
    {
        Vector3 result = new Vector3(0, 0, 0);
        string[] values = aData.Split('|');
        if (values.Length != 3)
            throw new System.FormatException("component count mismatch. Expected 3 components but got " + values.Length);
        result = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        return result;
    }
}