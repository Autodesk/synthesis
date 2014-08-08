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
	public int[] motors = {1,2,3,4};
	RigidNode_Base skeleton;
	unityPacket udp = new unityPacket();
	List<Vector3> unityWheelData = new List<Vector3>();

	public enum WheelPositions
	{
		FL = 1,
		FR = 2,
		BL = 3,
		BR = 4
	}

	[STAThread]
	void OnGUI ()
	{
		if (GUI.Button (new Rect (10, 10, 90, 30), "Load Model")) 
		{
			String filePath;
			FolderBrowserDialog fbd = new FolderBrowserDialog ();
			
			
			if (DialogResult.OK == fbd.ShowDialog ()) 
			{
				filePath = fbd.SelectedPath;

				List<RigidNode_Base> names = new List<RigidNode_Base>();
				RigidNode_Base.NODE_FACTORY = new UnityRigidNodeFactory();
				skeleton = BXDJSkeleton.ReadSkeleton(filePath + "/skeleton.bxdj");
				skeleton.ListAllNodes(names);
				foreach (RigidNode_Base node in names)
				{
					UnityRigidNode uNode = (UnityRigidNode)node;
					
					uNode.CreateTransform(transform);		
					uNode.CreateMesh(filePath +"/" + uNode.modelFileName);
					uNode.FlipNorms();
					uNode.CreateJoint();
					
					if (uNode.IsWheel)
					{
						unityWheelData.Add(uNode.GetWheelCenter());
					}
				}
				//auxFunctions.OrientRobot(unityWheelData, transform);
				//auxFunctions.placeRobotJustAboveGround(transform);

				GameObject.Find("Camera").AddComponent<Camera>();


			}
		}
	}

	void Start()
	{
		Physics.gravity = new Vector3(0,-9.8f,0);
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
		
	}
}
