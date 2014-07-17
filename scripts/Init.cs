using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class Init : MonoBehaviour
{		


		void Start ()
		{
				string homePath = (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable("HOME") : System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
				//Now you can use a default directory to load all of the files
				Directory.CreateDirectory(homePath + "/Documents/Skeleton/Skeleton/");
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
				transform.Rotate(new Vector3(-90.0f,0.0f,0.0f));
				//transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);
				

		}

		void FixedUpdate ()
		{

		WheelCollider[] tmps = transform.GetComponentsInChildren<WheelCollider> ();
		foreach (WheelCollider tmp in tmps){
						tmp.motorTorque = Input.GetAxis ("Vertical") * 5 - Input.GetAxis ("Horizontal") * 5;
		}

//				for (int i = 1; i < 2; i++) {
//						WheelCollider tmp = transform.GetChild (i).GetChild (1).GetComponent<WheelCollider> ();
//						tmp.motorTorque = Input.GetAxis ("Vertical") * 5 - Input.GetAxis ("Horizontal") * 5;
//				}
//				for (int i = 2; i < 4; i++) {
//						WheelCollider tmp = transform.GetChild (i).GetChild (1).GetComponent<WheelCollider> ();
//			tmp.motorTorque = Input.GetAxis ("Vertical") * 5 + Input.GetAxis ("Horizontal") * 5;
//				}

		}
		

}

