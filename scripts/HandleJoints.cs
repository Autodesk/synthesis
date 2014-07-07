using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic; 
public class HandleJoints : MonoBehaviour
{
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
	
	public static void loadBXDJ(string filePath = null, Transform jointO, Transform jointC)
	{

		List<RigidNode_Base> nodes = new List<RigidNode_Base>();
		RigidNode_Base skeleton = SkeletonIO.readSkeleton ("C:/Users/t_waggn/Documents/bxdj/skeleton.bxdj");
		skeleton.listAllNodes(nodes); 

		foreach (RigidNode_Base node in nodes) {

			SkeletalJoint_Base nodeX = node.getSkeletalJoint();
			if (nodeX != null){
				if ((int)nodeX.getJointType() == (int)SkeletalJointType.ROTATIONAL){
					RotationalJoint_Base nodeR = (RotationalJoint_Base)nodeX;
					var rigid = jointC;
					var owner = jointO;

					Rigidbody rigidB = rigid.gameObject.AddComponent<Rigidbody>();
					var ownerB = owner.gameObject.AddComponent<ConfigurableJoint>();
					ownerB.connectedBody = rigidB;
					rigidB.useGravity = false;


					Vector3 parentC = new Vector3((float)nodeR.parentBase.x,(float)nodeR.parentBase.y,(float)nodeR.parentBase.z);
					Vector3 parentN = new Vector3((float)nodeR.parentNormal.x, (float)nodeR.parentNormal.y,(float)nodeR.parentNormal.z);
					Vector3 childC = new Vector3((float)nodeR.childBase.x,(float)nodeR.childBase.y,(float)nodeR.childBase.z);
					Vector3 childN = new Vector3((float)nodeR.childNormal.x,(float)nodeR.childNormal.y,(float)nodeR.childNormal.z);

					
					ownerB.anchor = parentC;
					ownerB.connectedAnchor = childC;
					ownerB.axis = parentN;
					//ownerB.secondaryAxis = new Vector3 (0, 0, 1);
					
					ownerB.angularXMotion = ConfigurableJointMotion.Limited;
					ownerB.angularYMotion = ConfigurableJointMotion.Locked;
					ownerB.angularZMotion = ConfigurableJointMotion.Locked;
					ownerB.xMotion = ConfigurableJointMotion.Locked;
					ownerB.yMotion = ConfigurableJointMotion.Locked;
					ownerB.zMotion = ConfigurableJointMotion.Locked;
				}
				
			}

			Debug.Log("Alive!");
				}
		//Rigidbody rigidB = rigid.gameObject.AddComponent<Rigidbody> ();
		//owner.gameObject.AddComponent<Rigidbody> ();
		//var ownerB = owner.gameObject.AddComponent<ConfigurableJoint> ();
		//ownerB.connectedBody = rigidB;
		//ownerB.gameObject.rigidbody.useGravity = false;
		//rigidB.useGravity = false;
		
		

		
		//ownerB.anchor = parentC;
		//ownerB.connectedAnchor = childC;
		//ownerB.axis = parentN;
		//ownerB.secondaryAxis = new Vector3 (0, 0, 1);
		
		//ownerB.angularXMotion = ConfigurableJointMotion.Limited;
		//ownerB.angularYMotion = ConfigurableJointMotion.Locked;
		//ownerB.angularZMotion = ConfigurableJointMotion.Locked;
		//ownerB.xMotion = ConfigurableJointMotion.Locked;
		//ownerB.yMotion = ConfigurableJointMotion.Locked;
		//ownerB.zMotion = ConfigurableJointMotion.Locked;
		//Debug.Log (jointType);
	}
	void Start(){
		loadBXDJ ();

	}

}

