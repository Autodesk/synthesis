using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

public class Init : MonoBehaviour
{

    // We will need these
    public List<List<UnityRigidNode>> PWMAssignments;
    public float speed = 5;
    public int[] motors = { 1, 2, 3, 4 };
    RigidNode_Base skeleton;
    unityPacket udp = new unityPacket();
    List<GameObject> unityWheelData = new List<GameObject>();
    List<MeshCollider> meshColliders = new List<MeshCollider>();
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
            nodeThing.CreateMesh("C:/Users/" + Environment.UserName + "/Documents/Skeleton/field.bxda");
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
                UnityRigidNode uNode = (UnityRigidNode)node;

                uNode.CreateTransform(robot.transform);
                uNode.CreateMesh(filePath + uNode.modelFileName);

                uNode.CreateJoint();
                if (uNode.modelFileName == "node_0.bxda")
                {
                    uNode.unityObject.transform.rigidbody.mass = 10;
                }
                if (uNode.IsWheel)
                {
                    unityWheelData.Add(uNode.wCollider);
                    
                }
                meshColliders.Add(uNode.meshCollider);
                
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

        }
    }
}
