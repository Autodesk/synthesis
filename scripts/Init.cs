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
    public Dictionary<List<int>, UnityRigidNode> SolenoidAssignments;
    public float speed = 5;
    public int[] motors = { 1, 2, 3, 4 };
    RigidNode_Base skeleton;
    unityPacket udp = new unityPacket();
    List<WheelCollider> unityWheelData = new List<WheelCollider>();

    int robots = 0;
	string filePath = "C:/Users/t_defap/Documents/2014 Competition Robot_Skeleton/2014 Competition Robot_Skeleton";

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
		if (GUI.Button (new Rect (10, 10, 90, 30), "Load Model")) 
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog ();
			
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
           // UnityRigidNode nodeThing = new UnityRigidNode();
           // nodeThing.modelFileName = "field.bxda";
           // nodeThing.CreateTransform(transform);
			//nodeThing.CreateMesh("C:/Users/t_defap/Desktop/Skeleton/field.bxda");
            //nodeThing.unityObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;


            GameObject robot = new GameObject("Robot");
            robot.transform.parent = transform;

            List<RigidNode_Base> names = new List<RigidNode_Base>();
            RigidNode_Base.NODE_FACTORY = delegate()
            {
                return new UnityRigidNode();
            };
            skeleton = BXDJSkeleton.ReadSkeleton(filePath + "/skeleton.bxdj");
            skeleton.ListAllNodes(names);
            foreach (RigidNode_Base node in names)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;

                uNode.CreateTransform(robot.transform);
                uNode.CreateMesh(filePath + "/" + uNode.modelFileName);
                uNode.FlipNorms();
                uNode.CreateJoint();

                if (uNode.IsWheel)
                {
                    unityWheelData.Add(uNode.GetWheelCollider());

                }
            }
			if(unityWheelData.Count > 0)
			{
				Debug.Log (unityWheelData[1]);
				//robot.transform.Rotate(30,30,30);
            	auxFunctions.OrientRobot(unityWheelData, robot.transform);
			}else
			{
				Debug.Log ("unityWheelData is null: Does the robot have wheel colliders?");
			}

       }
    }

    void Start()
    {
        Physics.gravity = new Vector3(0, -9.8f, 0);
        TryLoad();
		
        //byte test = (byte)2;		
        //DriveJoints.updateSolenoids(skeleton, test);
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
            DriveJoints.UpdateAllMotors(skeleton, packet.dio[0].pwmValues);
        }
    }
}
