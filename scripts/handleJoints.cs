using UnityEngine;
using System.Collections;
using System.IO;

public class HandleJoints : MonoBehaviour
{
	public enum SkeletalJointType
	{
		ROTATIONAL = 1, LINEAR = 2
	}
	
	public enum JointDriverType
	{
		MOTOR = 1,
		SERVO = 2,
		WORM_SCREW = 3,
		BUMPER_PNEUMATIC = 4,
		RELAY_PNEUMATIC = 5
	}

	public static void loadBXDJ(string path, Transform child0, Transform child1)
	{
		BinaryReader reader = new BinaryReader (File.Open (path, FileMode.Open));

		int nodeCount = reader.ReadInt32 ();
		int parentNode = reader.ReadInt32 ();
		string fileName = reader.ReadString ();

		reader.ReadInt32 ();

		int jointType = reader.ReadByte ();

		Transform rigid = child0;
		Transform owner = child1;
		
		double angLimLow, angLimHigh;

		Rigidbody rigidB = rigid.gameObject.AddComponent<Rigidbody> ();

		owner.gameObject.AddComponent<Rigidbody> ();

		ConfigurableJoint ownerB = owner.gameObject.AddComponent<ConfigurableJoint> ();
		ownerB.connectedBody = rigidB;
		ownerB.gameObject.rigidbody.useGravity = false;
		rigidB.useGravity = false;

		if (jointType == (int)SkeletalJointType.ROTATIONAL) 
		{
			Vector3 parentC = new Vector3 ((float)reader.ReadDouble (), (float)reader.ReadDouble (), (float)reader.ReadDouble ());
			Vector3 parentN = new Vector3 ((float)reader.ReadDouble (), (float)reader.ReadDouble (), (float)reader.ReadDouble ());
			Vector3 childC = new Vector3 ((float)reader.ReadDouble (), (float)reader.ReadDouble (), (float)reader.ReadDouble ());
			Vector3 childN = new Vector3 ((float)reader.ReadDouble (), (float)reader.ReadDouble (), (float)reader.ReadDouble ());

			int hasLimit = reader.ReadByte ();

			//int angLimit = reader.ReadByte ();
			if (hasLimit != -1) 
			{
				angLimLow = reader.ReadDouble ();
				angLimHigh = reader.ReadDouble ();
			} 

			else 
			{
				reader.ReadBytes (16);
			}

			//skips linear segment
			reader.ReadBytes(108); 

			//ignores footer information relevent to the driver
			rigidB.constraints = RigidbodyConstraints.FreezeAll;
			
			ownerB.anchor = parentC;
			ownerB.connectedAnchor = childC;
			ownerB.axis = parentN;
			ownerB.secondaryAxis = new Vector3 (0, 0, 1);
			
			ownerB.angularXMotion = ConfigurableJointMotion.Limited;
			ownerB.angularYMotion = ConfigurableJointMotion.Locked;
			ownerB.angularZMotion = ConfigurableJointMotion.Locked;
			ownerB.xMotion = ConfigurableJointMotion.Locked;
			ownerB.yMotion = ConfigurableJointMotion.Locked;
			ownerB.zMotion = ConfigurableJointMotion.Locked;

		} 

		else 
		{
			Debug.Log (jointType);
			throw new System.Exception("Linear Joint type");
		}
	}
}

