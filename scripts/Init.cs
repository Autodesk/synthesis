using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Init : MonoBehaviour
{		
		
		public static void generateCubes (int amt, Transform parent, string name)
		{
				GameObject tmp;
				for (int i = 0; i < amt; i++) {
						tmp = GameObject.CreatePrimitive (PrimitiveType.Cube);
						tmp.transform.parent = parent;
						tmp.transform.position = new Vector3 (0, 0, 0);
						Destroy (tmp.collider);
						tmp.name = name + i;
				}
		
		}
		//converts custom vector format to unity vector3's
		public static Vector3 ConvertV3(BXDVector3 vector)	
		{
			Vector3 vectorUnity = new Vector3 ((float)vector.x, (float)vector.y, (float)vector.z);
			return vectorUnity;
		}

		void Start ()
		{
				//Now you can use a default directory to load all of the files
				string path = "C:/Users/t_waggn/Documents/Skeleton/";
				
				List<string> filepaths = new List<string>();
				List<RigidNode_Base> names = new List<RigidNode_Base> ();
				HandleJoints.genSkeleton ("C:/Users/t_waggn/Documents/Skeleton/skeleton.bxdj", names);
				foreach (RigidNode_Base node in names) { 
						filepaths.Add(path + node.modelName);
						
				}


				//generateCubes (filepaths.Count);
				
				for (int i = 0; i < filepaths.Count; i++) {
						HandleMeshes.loadBXDA (filepaths [i], transform.GetChild (i));
				}
				HandleMeshes.attachMeshColliders (transform);
				HandleJoints.loadBXDJ (transform);
				//HandleMeshes.attachRigidBodies (transform);

				


		}

		void FixedUpdate ()
		{
		/*
				for (int i = 7; i < 10; i++) {
						WheelCollider tmp = transform.GetChild (i).GetComponent<WheelCollider> ();
						tmp.motorTorque = Input.GetAxis ("Vertical") * 5 - Input.GetAxis ("Horizontal") * 5;
				}
				for (int i = 10; i < 12; i++) {
						WheelCollider tmp = transform.GetChild (i).GetComponent<WheelCollider> ();
						tmp.motorTorque = Input.GetAxis ("Vertical") * 5 + Input.GetAxis ("Horizontal") * 5;
				}
*/
}
		

}

