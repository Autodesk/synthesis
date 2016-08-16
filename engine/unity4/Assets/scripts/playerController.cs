using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class playerController : MonoBehaviour {

    public const float PHYSICS_MASS_MULTIPLIER = 0.001f;
    public const float FORMAT_3DS_SCALE = 0.2558918f;
    private RigidNode_Base skeleton;
    private GameObject activeRobot;
    private multiplayer_Init other;
    public float acceleration;
    public float angvelo;
    public float speed;
    public float weight;
    public float time;
    public bool time_stop;
    public float oldSpeed;
    private GameObject mainNode;
    private Quaternion rotation;
    public int playerNumber;

    // Use this for initialization
    void Start () {
        GameObject go = GameObject.Find("Initi");
        other = (multiplayer_Init)go.GetComponent(typeof(multiplayer_Init));
        playerNumber = other.numPlayers;
        TryLoadRobot();   
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("r"))
        {
            resetRobot();
        }
	}

    /// <summary>
    /// Repositions the robot so it is aligned at the center of the field, and resets all the
    /// joints, velocities, etc..
    /// </summary>
    private void resetRobot()
    {
        if (activeRobot != null)
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

        }

        /** foreach (GameObject o in totes)
        {
            GameObject.Destroy(o);
        }

        totes.Clear(); **/
    }

    /// <summary>
	/// Loads a robot from file into the simulator.
	/// </summary>
    public void TryLoadRobot()
    {
        //resets rotation for new robot
        rotation = Quaternion.identity;
        if (activeRobot != null)
        {
            //skeleton = null;
            //UnityEngine.Object.Destroy(activeRobot);
        }
        if (other.filePath != null) //&& skeleton == null)
        {
            //Debug.Log (filePath);
            List<Collider> meshColliders = new List<Collider>();
            activeRobot = new GameObject("Robot" + playerNumber);
            activeRobot.transform.parent = transform;

            List<RigidNode_Base> names = new List<RigidNode_Base>();
            RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
            {
                return new UnityRigidNode(guid);
            };

            skeleton = BXDJSkeleton.ReadSkeleton(other.filePath + "skeleton.bxdj");
            //Debug.Log(filePath + "skeleton.bxdj");
            skeleton.ListAllNodes(names);
            foreach (RigidNode_Base node in names)
            {
                UnityRigidNode uNode = (UnityRigidNode)node;

                uNode.CreateTransform(activeRobot.transform);

                if (!uNode.CreateMesh(other.filePath + uNode.ModelFileName))
                {
                    UserMessageManager.Dispatch(node.ModelFileName + " has been modified and cannot be loaded.", 6f);
                    skeleton = null;
                    UnityEngine.Object.Destroy(activeRobot);
                    return;
                }

                uNode.CreateJoint();

                //Debug.Log("Joint");

                meshColliders.AddRange(uNode.unityObject.GetComponentsInChildren<Collider>());
            }

            {   // Add some mass to the base object
                UnityRigidNode uNode = (UnityRigidNode)skeleton;
                uNode.unityObject.transform.rigidbody.mass += 20f * PHYSICS_MASS_MULTIPLIER; // Battery'
            }

            //finds main node of robot to use its rigidbody
            mainNode = transform.GetChild(0).gameObject;

            string robotname = new DirectoryInfo(other.filePath).Name; //Retrieving the name of the robot folder from the filepath.
            /** if (DriverPracticeMode.CheckRobot(robotname))
            {
                driverPracticeMode = activeRobot.AddComponent<DriverPracticeMode>();
                driverPracticeMode.Initialize(robotname);
                driverPraticeOn = true;
            } **/

            //Debug.Log ("HELLO AMIREKA: " + mainNode);
            auxFunctions.IgnoreCollisionDetection(meshColliders);
            resetRobot();
        }
        else
        {
            //Debug.Log("unityWheelData is null...");
        }
        //HideGuiSidebar();
    }

    void FixedUpdate()
    {
        if (skeleton != null)
        {
            List<RigidNode_Base> nodes = skeleton.ListAllNodes();

            unityPacket.OutputStatePacket packet = other.udp.GetLastPacket();

            /** //stops robot while menu is open
            if (gui.guiVisible)
            {
                mainNode.rigidbody.isKinematic = true;
            }
            else
            {
                mainNode.rigidbody.isKinematic = false;
            } **/

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
            other.udp.WritePacket(sensorPacket);
            #endregion

            //calculates stats of robot
            /** if (mainNode != null)
            {
                speed = (float)Math.Abs(mainNode.rigidbody.velocity.magnitude);
                weight = (float)Math.Round(mainNode.rigidbody.mass * 2.20462 * 860, 1);
                angvelo = (float)Math.Abs(mainNode.rigidbody.angularVelocity.magnitude);
                acceleration = (float)(mainNode.rigidbody.velocity.magnitude - oldSpeed) / Time.deltaTime;
                oldSpeed = speed;
                if (!time_stop)
                    time += Time.deltaTime;
            } **/
        }
    }
}
