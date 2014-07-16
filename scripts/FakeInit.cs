using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class FakeInit : MonoBehaviour
{		

	public Dictionary<int, UnityRigidNode> PWMAssignments = new Dictionary<int, UnityRigidNode> ();
	public float speed = 10;
	
	void Start ()
	{
		string homePath = (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable ("HOME") : System.Environment.ExpandEnvironmentVariables ("%HOMEDRIVE%%HOMEPATH%");
		//Now you can use a default directory to load all of the files
		Directory.CreateDirectory (homePath + "/Documents/Skeleton/Skeleton");
		string path = homePath + "/Documents/Skeleton/Skeleton/";
		
		
		List<RigidNode_Base> names = new List<RigidNode_Base> ();
		RigidNode_Base.NODE_FACTORY = new UnityRigidNodeFactory ();
		RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton (homePath + "/Documents/Skeleton/Skeleton/skeleton.bxdj");
		
		skeleton.ListAllNodes (names);
		foreach (RigidNode_Base node in names) {
			UnityRigidNode uNode = (UnityRigidNode)node;
			uNode.CreateTransform (transform);
			transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);			
			uNode.CreateMesh (path + uNode.GetModelFileName ());
			uNode.CreateJoint ();
			
			
			
			
		}
		transform.Rotate (new Vector3 (-90.0f, 0.0f, 0.0f));
		PWMAssignments = Controls.assignMotors (skeleton);
		
		Controls.setControls ("W", "S", "A", "D");
		
	}
	
	void FixedUpdate ()
	{

		// Old Reliable
		/*if (Input.anyKey) {
			if (Input.GetKey (Controls.forward)) {
				Controls.setMotor (PWMAssignments [1], speed);
				Controls.setMotor (PWMAssignments [2], speed);
				Controls.setMotor (PWMAssignments [3], speed);
				Controls.setMotor (PWMAssignments [4], speed);
			}
			if (Input.GetKey (Controls.backward)) {
				Controls.setMotor (PWMAssignments [1], -speed);
				Controls.setMotor (PWMAssignments [2], -speed);
				Controls.setMotor (PWMAssignments [3], -speed);
				Controls.setMotor (PWMAssignments [4], -speed);
			}
			if (Input.GetKey (Controls.left)) {
				Controls.setMotor (PWMAssignments [4], -speed);
				Controls.setMotor (PWMAssignments [1], -speed);
				Controls.setMotor (PWMAssignments [2], speed);
				Controls.setMotor (PWMAssignments [3], speed);
			}
			if (Input.GetKey (Controls.right)) {
				Controls.setMotor (PWMAssignments [4], speed);
				Controls.setMotor (PWMAssignments [1], speed);
				Controls.setMotor (PWMAssignments [2], -speed);
				Controls.setMotor (PWMAssignments [3], -speed);
			}
		} else {
			Controls.setMotor (PWMAssignments [1], 0);
			Controls.setMotor (PWMAssignments [2], 0);
			Controls.setMotor (PWMAssignments [3], 0);
			Controls.setMotor (PWMAssignments [4], 0);
		}*/

		// Using Equations
		Controls.setMotor (PWMAssignments[4], -10*(Input.GetAxis("Vertical")+Input.GetAxis("Horizontal")));
		Controls.setMotor (PWMAssignments[3], 10*(Input.GetAxis("Vertical")-Input.GetAxis("Horizontal")));
		Controls.setMotor (PWMAssignments[1], 10*(Input.GetAxis("Vertical")+Input.GetAxis("Horizontal")));
		Controls.setMotor (PWMAssignments[2], 10*(Input.GetAxis("Vertical")-Input.GetAxis("Horizontal")));			
		
	}


}

