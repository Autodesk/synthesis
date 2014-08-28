using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Init : MonoBehaviour
{
    // We will need these
    public const float PHYSICS_MASS_MULTIPLIER = 0.001f;

    private GUIController gui;

    private RigidNode_Base skeleton;
    private GameObject activeRobot;

    private unityPacket udp = new unityPacket();
    private string filePath = BXDSettings.Instance.LastSkeletonDirectory + "\\";
    private volatile int reloadInFrames = -1;

    public Init()
    {
    }

    [STAThread]
    void OnGUI()
    {
        if (gui == null)
        {
            gui = new GUIController();
            gui.AddAction("Orient Robot", () =>
            {
                OrientRobot();
            });
            gui.OpenedRobot += (string path) =>
            {
                if (File.Exists(path + "\\skeleton.bxdj"))
                {
                    this.filePath = path;
                    reloadInFrames = 2;
                }
            };
            if (!File.Exists(filePath + "\\skeleton.bxdj"))
            {
                gui.ShowBrowser();
            }
        }
        gui.Render();

        if (reloadInFrames >= 0)
        {
            GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
            GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Loading... Please Wait", gui.BlackBoxStyle);
        }
    }

    private void OrientRobot()
    {
        if (activeRobot != null && skeleton != null)
        {
            var unityWheelData = new List<GameObject>();
            // Invert the position of the root object
            Vector3 basePos = ((UnityRigidNode) skeleton).unityObject.transform.position;
            activeRobot.transform.position -= basePos;
            var nodes = skeleton.ListAllNodes();
            foreach (RigidNode_Base node in nodes)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;

                if (uNode.IsWheel && uNode.wCollider != null)
                {
                    unityWheelData.Add(uNode.wCollider);
                }
            }
            if (unityWheelData.Count > 0)
            {
               auxFunctions.OrientRobot(unityWheelData, activeRobot.transform);
            }
        }
    }

    private void TryLoad()
    {
        if (activeRobot != null)
        {
            skeleton = null;
            UnityEngine.Object.Destroy(activeRobot);
        }
        if (filePath != null && skeleton == null)
        {
            List<Collider> meshColliders = new List<Collider>();
            activeRobot = new GameObject("Robot");
            activeRobot.transform.parent = transform;

            List<RigidNode_Base> names = new List<RigidNode_Base>();
            RigidNode_Base.NODE_FACTORY = delegate()
            {
                return new UnityRigidNode();
            };

            skeleton = BXDJSkeleton.ReadSkeleton(filePath + "skeleton.bxdj");
            skeleton.ListAllNodes(names);
            foreach (RigidNode_Base node in names)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;

                uNode.CreateTransform(activeRobot.transform);
                uNode.CreateMesh(filePath + uNode.modelFileName);
                uNode.CreateJoint();

                meshColliders.AddRange(uNode.unityObject.GetComponentsInChildren<Collider>());
            }

            {   // Add some mass to the base object
                UnityRigidNode uNode = (UnityRigidNode) skeleton;
                uNode.unityObject.transform.rigidbody.mass += 20f * PHYSICS_MASS_MULTIPLIER; // Battery'
                Vector3 vec = uNode.unityObject.rigidbody.centerOfMass;
                vec.y *= 0.9f;
                uNode.unityObject.rigidbody.centerOfMass = vec;
            }

            auxFunctions.IgnoreCollisionDetection(meshColliders);
        }
        else
        {
            Debug.Log("unityWheelData is null...");
        }
        gui.guiVisible = false;
    }

    void Start()
    {
        Physics.gravity = new Vector3(0, -9.8f, 0);
        Physics.solverIterationCount = 15;
        Physics.minPenetrationForPenalty = 0.001f;

        // Load Field

        UnityRigidNode nodeThing = new UnityRigidNode();
        nodeThing.modelFileName = "field.bxda";
        nodeThing.CreateTransform(transform);
        nodeThing.CreateMesh(UnityEngine.Application.dataPath + "\\Resources\\field.bxda");
        nodeThing.unityObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        reloadInFrames = 2;
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
        if (reloadInFrames >= 0 && reloadInFrames-- == 0)
        {
            reloadInFrames = -1;
            TryLoad();
        }
    }

    void FixedUpdate()
    {
        if (skeleton != null)
        {
            unityPacket.OutputStatePacket packet = udp.GetLastPacket();
            DriveJoints.UpdateAllMotors(skeleton, packet.dio);
            DriveJoints.UpdateSolenoids(skeleton, packet.solenoid);
            List<RigidNode_Base> nodes = skeleton.ListAllNodes();
            InputStatePacket sensorPacket = new InputStatePacket();
            foreach (RigidNode_Base node in nodes)
            {
                if (node.GetSkeletalJoint() == null)
                    continue;
                foreach (RobotSensor sensor in node.GetSkeletalJoint().attachedSensors)
                {
                    if (sensor.type == RobotSensorType.POTENTIOMETER && node.GetSkeletalJoint() is RotationalJoint_Base)
                    {
                        UnityRigidNode uNode = (UnityRigidNode) node;
                        float angle = DriveJoints.GetAngleBetweenChildAndParent(uNode) + ((RotationalJoint_Base) uNode.GetSkeletalJoint()).currentAngularPosition;
                        sensorPacket.ai[sensor.module - 1].analogValues[sensor.port - 1] = (int) sensor.equation.Evaluate(angle);
                    }
                }
            }
            udp.WritePacket(sensorPacket);
        }
    }
}
