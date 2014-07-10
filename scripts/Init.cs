using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Init : MonoBehaviour
{		
		
		public static void generateCubes (int amt, Transform parent, string name)
		{
				GameObject tmp;
				for (int i = 0; i < amt; i++) {
						tmp = new GameObject ();
						tmp.transform.parent = parent;
						tmp.transform.position = new Vector3 (0, 0, 0);
						Destroy (tmp.collider);
						tmp.name = name + i;
				}
		
		}
		//converts custom vector format to unity vector3's
		public static Vector3 ConvertV3 (BXDVector3 vector)
		{
				Vector3 vectorUnity = new Vector3 ((float)vector.x, (float)vector.y, (float)vector.z);
				return vectorUnity;
		}

		void Start ()
		{
				//Now you can use a default directory to load all of the files
				string path = "C:/Users/t_waggn/Documents/Skeleton/Skeleton/";
				
				List<string> filepaths = new List<string> ();
				List<RigidNode_Base> names = new List<RigidNode_Base> ();
				
				RigidNode_Base.NODE_FACTORY = new UnityRigidNodeFactory ();
				RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton ("C:/Users/t_waggn/Documents/Skeleton/Skeleton/skeleton.bxdj");
				skeleton.ListAllNodes (names);
				foreach (RigidNode_Base node in names) {
						UnityRigidNode uNode = (UnityRigidNode)node;
						uNode.CreateTransform (transform);
						uNode.CreateMesh (path + uNode.GetModelFileName ());
						uNode.CreateJoint ();
					
				}




				/*	
		foreach (RigidNode_Base node in names) { 
						//Debug.Log (node.modelName);
						filepaths.Add (path + node.modelName);
						
				}


				generateCubes (filepaths.Count, transform, "part");
				for (int i = 0; i < filepaths.Count; i++) {
						HandleMeshes.loadBXDA (filepaths [i], transform.GetChild (i));
		}
				transform.Rotate (new Vector3(0,0,180));
				HandleMeshes.attachMeshColliders (transform);
				HandleJoints.loadBXDJ (transform);
				//HandleMeshes.attachRigidBodies (transform);
				//transform.Rotate (90,90,0, Space.World);
				//transform.rotation = Quaternion.Euler(0,90,0);
				*/
		}

		void FixedUpdate ()
		{
				
				for (int i = 1; i < 2; i++) {
						WheelCollider tmp = transform.GetChild (i).GetChild(1).GetComponent<WheelCollider> ();
						tmp.motorTorque = Input.GetAxis ("Vertical") * 5 - Input.GetAxis ("Horizontal") * 5;
				}
				for (int i = 2; i < 4; i++) {
						WheelCollider tmp = transform.GetChild (i).GetChild(1).GetComponent<WheelCollider> ();
						tmp.motorTorque = Input.GetAxis ("Vertical") * 5 + Input.GetAxis ("Horizontal") * 5;
				}

		}
		

}

