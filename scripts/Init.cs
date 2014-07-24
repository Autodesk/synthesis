using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Init : MonoBehaviour
{		
		// We will need these
		public Dictionary<int, List<UnityRigidNode>> PWMAssignments;
		public float speed = 5;
		public int[] motors = {1,2,3,4};
		public enum WheelPositions
		{
				FL = 1,
				FR = 2,
				BL = 3,
				BR = 4
		}



		void Start ()
		{
				string homePath = (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable ("HOME") : System.Environment.ExpandEnvironmentVariables ("%HOMEDRIVE%%HOMEPATH%");
				//Now you can use a default directory to load all of the files
				Directory.CreateDirectory (homePath + "/Documents/Skeleton/Skeleton/");
				string path = homePath + "/Documents/Skeleton/Skeleton/";
				
	
				List<RigidNode_Base> names = new List<RigidNode_Base> ();
				RigidNode_Base.NODE_FACTORY = new UnityRigidNodeFactory ();
				RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton (homePath + "/Documents/Skeleton/Skeleton/skeleton.bxdj");

				skeleton.ListAllNodes (names);
				foreach (RigidNode_Base node in names) {
						UnityRigidNode uNode = (UnityRigidNode)node;
						uNode.CreateTransform (transform);
									
						uNode.CreateMesh (path + uNode.GetModelFileName ());
						uNode.CreateJoint ();
						

						
					
				}
				transform.Rotate (new Vector3 (-90.0f, 0.0f, 0.0f));
				
				skeleton.ListAllNodes (names);
		
				PWMAssignments = MotorInit.assignListOfMotors (skeleton);

				//foreach (KeyValuePair<int, List<UnityRigidNode>> node in PWMAssignments) {
				//		DriveTrain.initiateMecanum (node.Value);
				//}
		
				// These can be whatever you want
				Controls.setControls ("T", "G", "F", "H");

				
				
		
		}
		
		void FixedUpdate ()
		{
				if (Input.anyKey) {
					//DriveTrain.driveSwerve(PWMAssignments, 5, 10);
					//DriveTrain.discoDrive(PWMAssignments, 1,2,3,4);
					//DriveTrain.swerveDrive(PWMAssignments, 5, 10);
					//DriveTrain.MecanumDrive(PWMAssignments,40,1,2,3,4);
					DriveTrain.TankDrive(PWMAssignments, 40, 1,2,3,4);
				} else {
						DriveTrain.stopAllWheelColliders (PWMAssignments);
				}
		}
}