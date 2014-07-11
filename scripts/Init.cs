using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Init : MonoBehaviour
{		


		void Start ()
		{
				string homePath = (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable("HOME") : System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
				//Now you can use a default directory to load all of the files
				Directory.CreateDirectory(homePath + "/Documents/Skeleton/Skeleton);
				string path = homePath + "/Documents/Skeleton/Skeleton/";
				
				List<string> filepaths = new List<string> ();
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

