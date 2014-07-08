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
	public static bool isWheel = true;

	public static void loadBXDJ(string filePath, Transform jointO, Transform jointC, Transform parent)
	{
		//this loads the skeleton for the object model
		List<RigidNode_Base> nodes = new List<RigidNode_Base>();
		RigidNode_Base skeleton = SkeletonIO.ReadSkeleton (filePath);
		skeleton.ListAllNodes(nodes); 

		int i = 0;
		foreach (RigidNode_Base node in nodes) {


			//returns skeletonjointtype to then be used as required joint type
			SkeletalJoint_Base nodeX = node.GetSkeletalJoint();
			if (nodeX != null){

				var rigid = jointC;
				var owner = jointO;

				//this is the conditional for Identified wheels
				if ((int)nodeX.GetJointType() == (int)SkeletalJointType.ROTATIONAL){

					RotationalJoint_Base nodeR = (RotationalJoint_Base)nodeX;

					//takes the x, y, and z axis information from a custom vector class to unity's vector class
					Vector3 parentC = new Vector3((float)nodeR.parentBase.x,(float)nodeR.parentBase.y,(float)nodeR.parentBase.z);
					Vector3 parentN = new Vector3((float)nodeR.parentNormal.x, (float)nodeR.parentNormal.y,(float)nodeR.parentNormal.z);
					Vector3 childC = new Vector3((float)nodeR.childBase.x,(float)nodeR.childBase.y,(float)nodeR.childBase.z);
					Vector3 childN = new Vector3((float)nodeR.childNormal.x,(float)nodeR.childNormal.y,(float)nodeR.childNormal.z);

					if((int)nodeX.GetJointType() == (int)SkeletalJointType.ROTATIONAL && isWheel == true){
						Wheelcolliders(parent, i);
						i++;
						//Debug.Log ("Alive");
					}else{
						Rigidbody rigidB = rigid.gameObject.AddComponent<Rigidbody>();
						var ownerB = owner.gameObject.AddComponent<ConfigurableJoint>();
						ownerB.connectedBody = rigidB;

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
				
					}
				}
			}
		}
	}
	public static void Wheelcolliders(Transform parent, int i){
					GameObject collider = new GameObject ();
					collider.transform.parent = parent;
					collider.transform.position = parent.GetChild (i).GetComponent<MeshCollider> ().bounds.center;
					collider.AddComponent<WheelCollider> ();
					collider.GetComponent<WheelCollider> ().radius = 5.2f;
					collider.GetComponent<WheelCollider> ().transform.Rotate (90, 0, 0);
				
				parent.Rotate (new Vector3 (-90, 0, 0));
				parent.gameObject.AddComponent<Rigidbody> ();
				parent.rigidbody.mass = 120;


		}
}

