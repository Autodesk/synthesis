using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class HandleJoints : MonoBehaviour
{
		//enums for quick ID'ing within conditionals
		public enum SkeletalJointType:byte
		{
				ROTATIONAL = 1, 
				LINEAR = 2
		}
	
		public enum JointDriverType:byte
		{
				MOTOR = 1,
				SERVO = 2,
				WORM_SCREW = 3,
				BUMPER_PNEUMATIC = 4,
				RELAY_PNEUMATIC = 5
		}

		//temp variable until wheels are included in the bxdj file format
		//public static bool isWheel = true;

		public static void loadBXDJ (Transform parent)
		{		
				//this loads the skeleton for the object model
				List<RigidNode_Base> nodes = new List<RigidNode_Base> ();
				genSkeleton ("C:/Users/t_waggn/Documents/Skeleton/Skeleton/skeleton.bxdj", nodes);
				

				int i = 1;
				foreach (RigidNode_Base node in nodes) {


						//returns skeletonjointtype to then be used as required joint type
						SkeletalJoint_Base nodeX = node.GetSkeletalJoint ();
						if (nodeX != null) {

								//this is the conditional for Identified wheels
								if ((int)nodeX.GetJointType () == (int)SkeletalJointType.ROTATIONAL) {

										RotationalJoint_Base nodeR = (RotationalJoint_Base)nodeX;

										//takes the x, y, and z axis information from a custom vector class to unity's vector class
										Vector3 parentC = Init.ConvertV3 (nodeR.parentBase);
										Vector3 parentN = Init.ConvertV3 (nodeR.parentNormal);
										Vector3 childC = Init.ConvertV3 (nodeR.childBase);
										Vector3 childN = Init.ConvertV3 (nodeR.childNormal);
										//Debug.Log ("Vector3" + parentC);
										//Debug.Log (node.children.Count);


										WheelDriverMeta testWheel = nodeX.cDriver != null ? nodeX.cDriver.GetInfo<WheelDriverMeta> () : null;
										Debug.Log ("testWheel: " + testWheel);
										
										var rigid = parent.GetChild (nodes.IndexOf (node.GetParent ()));
										var owner = parent.GetChild (nodes.IndexOf (node));
										if (!rigid.gameObject.GetComponent<Rigidbody>()) {
											rigid.gameObject.AddComponent<Rigidbody>();
										}
										Rigidbody rigidB = rigid.gameObject.GetComponent<Rigidbody> ();
										//rigidB.isKinematic = true;
										var ownerB = owner.gameObject.AddComponent<ConfigurableJoint> ();
										ownerB.connectedBody = rigidB;
										ownerB.enableCollision = false;

										//configures the joint
										ownerB.anchor = parentC;
										ownerB.connectedAnchor = childC;
										ownerB.axis = parentN;
										ownerB.secondaryAxis = new Vector3 (0, 0, 1);
					
										ownerB.angularXMotion = ConfigurableJointMotion.Free;
										ownerB.angularYMotion = ConfigurableJointMotion.Locked;
										ownerB.angularZMotion = ConfigurableJointMotion.Locked;
										ownerB.xMotion = ConfigurableJointMotion.Locked;
										ownerB.yMotion = ConfigurableJointMotion.Locked;
										ownerB.zMotion = ConfigurableJointMotion.Locked;
				
										
										if (testWheel != null && testWheel.position != WheelPosition.NO_WHEEL) {
												float radius = testWheel.radius;	
												Wheelcolliders (parent, radius, i);
												i++;
												
												
										}
												
								}
						}
				}
		}

		public static void Wheelcolliders (Transform parent, float radius, int i)
		{
				
				GameObject collider = new GameObject ();
				collider.name = "collider" + i;
				collider.transform.parent = parent.GetChild (i);
				collider.transform.position = parent.GetChild (i).GetChild (0).GetComponent<MeshCollider> ().bounds.center;
				collider.AddComponent<WheelCollider> ();
				collider.GetComponent<WheelCollider> ().radius = radius + 0.2f;
				collider.GetComponent<WheelCollider> ().transform.Rotate (0, 0, 0);
				
				//parent.Rotate (new Vector3 (0, 90, 0));

				//I want the grandfather to have a rigidbody
				parent.gameObject.AddComponent<Rigidbody> ();
				parent.rigidbody.mass = 120;


		}

		public static void genSkeleton (string filePath, List<RigidNode_Base> nodes)
		{
				RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton (filePath);
				skeleton.ListAllNodes (nodes);
		}

}

