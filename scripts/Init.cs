using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Init : MonoBehaviour
{
		unityPacket udp = new unityPacket();

		void Start ()
		{
				

				Physics.gravity = new Vector3 (0, -9.8f, 0);
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
				//transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);
				transform.Rotate (new Vector3 (-90.0f, 0.0f, 0.0f));
				

		}

		void OnEnable ()
		{
				udp.Start ();	
		}

		void OnDisable ()
		{
				udp.Stop ();
		}

		void FixedUpdate ()
		{
				
		

		}
}

