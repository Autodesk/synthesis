using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
public class Init : MonoBehaviour
{		
	// We will need these
	public List<List<UnityRigidNode>> PWMAssignments, wheels;
	public Dictionary<List<int>, UnityRigidNode> SolenoidAssignments;
	public float speed = 5;
	public int[] motors = {1,2,3,4};
	RigidNode_Base skeleton;
	//unityPacket udp = new unityPacket();
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
		Directory.CreateDirectory(homePath + "/Documents/ScaledSkeleton/Skeleton/");
		string path = homePath + "/Documents/ScaledSkeleton/Skeleton/";
				
	
		List<RigidNode_Base> names = new List<RigidNode_Base>();
		RigidNode_Base.NODE_FACTORY = new UnityRigidNodeFactory();
		skeleton = BXDJSkeleton.ReadSkeleton(homePath + "/Documents/Skeleton/Skeleton/skeleton.bxdj");
		skeleton.ListAllNodes(names);
		foreach (RigidNode_Base node in names)
		{
			UnityRigidNode uNode = (UnityRigidNode)node;
			uNode.CreateTransform(transform);		
			uNode.CreateMesh(path + uNode.GetModelFileName());
			uNode.FlipNorms();
			uNode.CreateJoint();
			if (uNode.wheelData != Vector3.zero)
            	unityWheelData.Add(uNode.wheelData);
            
			
		}
		
		Quaternion rotation = auxFunctions.FlipRobot(unityWheelData, transform);
		transform.localRotation *= rotation;
		//byte test = (byte)2;		
		//DriveJoints.updateSolenoids(skeleton, test);
	}

	void OnEnable()
	{
		
		//udp.Start();
	}

	void OnDisable()
	{
		//udp.Stop();	
	}
	
	void FixedUpdate()
	{
	
	}
}