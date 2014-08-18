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
    List<Vector3> unityWheelData = new List<Vector3>();
    // int robots = 0;
    string filePath = "C:/Users/" + Environment.UserName + "/Documents/Skeleton/";

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
            nodeThing.CreateMesh("C:/Users/t_waggn/Documents/Skeleton/field.bxda");
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

                uNode.FlipNorms();
                uNode.CreateJoint();

                if (uNode.IsWheel)
                {

                    unityWheelData.Add(auxFunctions.ConvertV3(uNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().center));
		//unityWheelData.add(uNode.GetWheelCollider());
                }
            }
            if (unityWheelData.Count > 0)
            {
                auxFunctions.OrientRobot(unityWheelData, robot.transform);

            }
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
