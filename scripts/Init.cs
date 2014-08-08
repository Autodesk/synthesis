using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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



	void Start()
	{
		Physics.gravity = new Vector3(0,-9.8f,0);
		string homePath = (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable("HOME") : System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
		//Now you can use a default directory to load all of the files
		Directory.CreateDirectory(homePath + "/Documents/Skeleton/Skeleton/");
		string path = homePath + "/Documents/Skeleton/Skeleton/";
				
	
		List<RigidNode_Base> names = new List<RigidNode_Base>();
		RigidNode_Base.NODE_FACTORY = delegate()
		{
			return new UnityRigidNode();
		};
		skeleton = BXDJSkeleton.ReadSkeleton(homePath + "/Documents/Skeleton/Skeleton/skeleton.bxdj");
		skeleton.ListAllNodes(names);
		foreach (RigidNode_Base node in names)
		{
			UnityRigidNode uNode = (UnityRigidNode)node;

			uNode.CreateTransform(transform);		
			uNode.CreateMesh(path + uNode.modelFileName);
			uNode.FlipNorms();
			uNode.CreateJoint();
			
			if (uNode.IsWheel)
			{
				unityWheelData.Add(uNode.GetWheelCenter());
			}
		}
		auxFunctions.OrientRobot(unityWheelData, transform);
		auxFunctions.placeRobotJustAboveGround(transform);
	
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
		unityPacket.OutputStatePacket packet = udp.getLastPacket();
		
		DriveJoints.UpdateAllWheels(skeleton ,packet.dio[0].pwmValues);
		
	}
}
