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
	public enum WheelPositions
	{
		FL = 1,
		FR = 2,
		BL = 3,
		BR = 4
	}



	void Start()
	{
		//Physics.gravity = new Vector3(0,-980f,0);
		string homePath = (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable("HOME") : System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
		//Now you can use a default directory to load all of the files
		Directory.CreateDirectory(homePath + "/Documents/Skeleton/Skeleton/");
		string path = homePath + "/Documents/Skeleton/Skeleton/";
				
	
		List<RigidNode_Base> names = new List<RigidNode_Base>();
		RigidNode_Base.NODE_FACTORY = new UnityRigidNodeFactory();
		skeleton = BXDJSkeleton.ReadSkeleton(homePath + "/Documents/Skeleton/Skeleton/skeleton.bxdj");

		skeleton.ListAllNodes(names);
		foreach (RigidNode_Base node in names)
		{
			UnityRigidNode uNode = (UnityRigidNode)node;
			uNode.CreateTransform(transform);
									
			uNode.CreateMesh(path + uNode.GetModelFileName());
//			uNode.CreateJoint();
		}
		Vector3 com = UnityRigidNode.TotalCenterOfMass(transform.gameObject);
		foreach (RigidNode_Base node in names)
		{
			UnityRigidNode uNode = (UnityRigidNode)node;
			if (uNode.GetParent() != null && uNode.GetSkeletalJoint() != null && uNode.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL) {
				RotationalJoint_Base rj = (RotationalJoint_Base) uNode.GetSkeletalJoint();
				Vector3 diff = auxFunctions.ConvertV3(rj.basePoint) - com;
				double dot = Vector3.Dot(diff,auxFunctions.ConvertV3(rj.axis));
				if (dot < 0) {
					rj.axis = rj.axis.Multiply(-1);
				}
			}
			uNode.CreateJoint();
		}
		//transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
				
		skeleton.ListAllNodes(names);
		byte test = (byte)2;		
		DriveJoints.updateSolenoids(skeleton, test);
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
		DriveJoints.updateSolenoids(skeleton, 1);
		//DriveJoints.UpdateAllWheels(skeleton, new float[]{.05f,.05f,.05f,.05f,.05f,.05f,.05f,.05f});
		/*if (Input.GetKey (KeyCode.W)) 
			{
				DriveJoints.UpdatePiston(SolenoidAssignments, 1, 2, 5);
			} else if (Input.GetKey (KeyCode.S)) 
			{
			DriveJoints.UpdatePiston(SolenoidAssignments, 1, 2, -5);
			}*/
	}
}