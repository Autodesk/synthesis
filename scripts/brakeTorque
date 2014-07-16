using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

//Prevents robot from moving when motors are stopped
//Thanks brakeTorque...
//The code is just stripped down to just using the brake function


public class Init : MonoBehaviour
{		
		void Start ()
		{
//		Physics.gravity = new Vector3(0, -980.0f, 0); 

				string homePath = (System.Environment.OSVersion.Platform == System.PlatformID.Unix || System.Environment.OSVersion.Platform == System.PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable("HOME") : System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
				//Now you can use a default directory to load all of the files
				Directory.CreateDirectory(homePath + "/Documents/NewSkeleton/");
				string path = homePath + "/Documents/NewSkeleton/";
				
				List<string> filepaths = new List<string> ();
				List<RigidNode_Base> names = new List<RigidNode_Base> ();
				
				RigidNode_Base.NODE_FACTORY = new UnityRigidNodeFactory ();
				RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton (homePath + "/Documents/NewSkeleton/skeleton.bxdj");
				skeleton.ListAllNodes (names);
				foreach (RigidNode_Base node in names) {
					UnityRigidNode uNode = (UnityRigidNode)node;
					uNode.CreateTransform (transform);
					uNode.CreateMesh (path + uNode.GetModelFileName ());
					uNode.CreateJoint ();
	
		}
				transform.Rotate(new Vector3(-90.0f,0.0f,0.0f));
				transform.position = new Vector3( 340,5,340); //relocates for convience
		}

		void FixedUpdate ()
		{
		WheelCollider[] tmps = transform.GetComponentsInChildren<WheelCollider> ();

		foreach (WheelCollider tmp in tmps){
			tmp.brakeTorque = 0;
			tmp.motorTorque = Input.GetAxis ("Vertical") * 5 - (Input.GetAxis ("Horizontal") * 5);
//			tmp.steerAngle = -((Input.GetAxis("RightStick")) * speed * Time.deltaTime);

//if the motors aren't moving, then the wheel colliders certainly aren't as well...
			if (tmp.motorTorque == 0){
				tmp.brakeTorque = 10000000; 

			}
    //Need access to input manager to complete this:
    //But all you need is something to control breaking
			if(Input.GetAxis("360_VerticalDPAD") < 0)
				{
					Debug.Log ("Down D-Pad button");
					tmp.brakeTorque = 1000000; ///BRAKE
				}

	
			if (Input.GetAxis("360_Triggers")> 0.001) 
			{
			//omni turn? Could use work
			//Hold triggers while driving normally
			tmp.steerAngle = -((Input.GetAxis("360_Triggers")) * speed * Time.deltaTime);
			}
			
}
}
