using UnityEngine;
using System.Collections;

public class Init : MonoBehaviour
{
	
		public void generateCubes (int amt)
		{
				GameObject tmp;
				for (int i = 0; i < amt; i++) {
						tmp = GameObject.CreatePrimitive (PrimitiveType.Cube);
						tmp.transform.parent = this.transform;
						tmp.transform.position = new Vector3 (0, 0, 0);
						Destroy (tmp.collider);
						tmp.name = "part" + i;
				}
		}

		void Start ()
		{
				string[] filepaths = {"C:/Users/t_waggn/Documents/Skeleton/node_0.bxda",
							  "C:/Users/t_waggn/Documents/Skeleton/node_1.bxda",
			 				  "C:/Users/t_waggn/Documents/Skeleton/node_2.bxda",
							  "C:/Users/t_waggn/Documents/Skeleton/node_3.bxda", 
							  "C:/Users/t_waggn/Documents/Skeleton/node_4.bxda", 
			 				  "C:/Users/t_waggn/Documents/Skeleton/node_5.bxda",
							  "C:/Users/t_waggn/Documents/Skeleton/node_6.bxda"};

				generateCubes (filepaths.Length);

				for (int i = 0; i < filepaths.Length; i++) {
						HandleMeshes.loadBXDA (filepaths [i], transform.GetChild (i));
				}
				HandleMeshes.attachMeshColliders (transform);
				HandleJoints.loadBXDJ ("C:/Users/t_waggn/Documents/Skeleton/skeleton.bxdj", transform);
				//HandleMeshes.attachRigidBodies (transform);



		}
	/*
		void FixedUpdate ()
		{

				for (int i = 7; i < 10; i++) {
						WheelCollider tmp = transform.GetChild (i).GetComponent<WheelCollider> ();
						tmp.motorTorque = Input.GetAxis ("Vertical") * 5 - Input.GetAxis ("Horizontal") * 5;
				}
				for (int i = 10; i < 13; i++) {
						WheelCollider tmp = transform.GetChild (i).GetComponent<WheelCollider> ();
						tmp.motorTorque = Input.GetAxis ("Vertical") * 5 + Input.GetAxis ("Horizontal") * 5;
				}
		}
*/
}
