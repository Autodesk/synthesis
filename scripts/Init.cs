using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEditor;
public class Init : MonoBehaviour
{

    // We will need these
	public const float PHYSICS_MASS_MULTIPLIER = 0.001f;
    public List<List<UnityRigidNode>> PWMAssignments;
    public float speed = 5;
    public int[] motors = { 1, 2, 3, 4 };
    RigidNode_Base skeleton;
    unityPacket udp = new unityPacket();
    List<GameObject> unityWheelData = new List<GameObject>();
    List<Collider> meshColliders = new List<Collider>();
    // int robots = 0;
    string filePath = BXDSettings.Instance.LastSkeletonDirectory + "\\";
    public enum WheelPositions
    {
        FL = 1,
        FR = 2,
        BL = 3,
        BR = 4
    }
    [STAThread]
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 90, 30), "Load Model"))
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {

                filePath = fbd.SelectedPath;
                TryLoad();
            }
        }

    }


    void TryLoad()
    {
        if (filePath != null && skeleton == null)
        {

            UnityRigidNode nodeThing = new UnityRigidNode();
            nodeThing.modelFileName = "field.bxda";
            nodeThing.CreateTransform(transform);
            nodeThing.CreateMesh(UnityEngine.Application.dataPath + "\\field.bxda", true);
            nodeThing.unityObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            GameObject robot = new GameObject("Robot");
            robot.transform.parent = transform;

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

                uNode.CreateTransform(robot.transform);
                uNode.CreateMesh(filePath + uNode.modelFileName, true);

                uNode.CreateJoint();
                if (uNode.modelFileName == "node_0.bxda")
                {
                    uNode.unityObject.transform.rigidbody.mass += 20f * PHYSICS_MASS_MULTIPLIER; // Battery'
					Vector3 vec = uNode.unityObject.rigidbody.centerOfMass;
					vec.y *= 0.9f;
					uNode.unityObject.rigidbody.centerOfMass = vec;
                }
                if (uNode.IsWheel)
                {
                    unityWheelData.Add(uNode.wCollider);

                }
                //meshColliders.Add(uNode.meshCollider);                
                meshColliders.AddRange(uNode.unityObject.GetComponentsInChildren<Collider>());
            }
            if (unityWheelData.Count > 0)
            {
                auxFunctions.OrientRobot(unityWheelData, robot.transform);

            }
            auxFunctions.IgnoreCollisionDetection(meshColliders);

        }
        else
        {
            Debug.Log("unityWheelData is null...");
        }

    }



    void Start()
    {
        Physics.gravity = new Vector3(0, -9.8f, 0);
        Physics.solverIterationCount = 15;
        Physics.minPenetrationForPenalty = 0.001f;

        TryLoad();
    }

    void OnEnable()
    {
        udp.Start();
    }

    void OnDisable()
    {
        udp.Stop();
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
