using UnityEngine;
using System.Collections;
using System.IO;

public class handleJoints : MonoBehaviour
{
	void generateCubes(int amt)
	{
		GameObject tmp;
		for (int i = 0; i < amt; i++) 
		{
			tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
			tmp.transform.parent = this.transform;
			tmp.transform.position = new Vector3(0,0,0);
			tmp.name = "part" + i;
		}
	}
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

				var rigid = child0;
				var owner = child1;
				
				double angLimLow, angLimHigh;

				Rigidbody rigidB = rigid.gameObject.AddComponent<Rigidbody> ();
				owner.gameObject.AddComponent<Rigidbody> ();
				var ownerB = owner.gameObject.AddComponent<ConfigurableJoint> ();
				ownerB.connectedBody = rigidB;
				ownerB.gameObject.rigidbody.useGravity = false;
				rigidB.useGravity = false;

			if (jointType == (int)SkeletalJointType.ROTATIONAL) {

					Vector3 parentC = new Vector3 ((float)reader.ReadDouble (), (float)reader.ReadDouble (), (float)reader.ReadDouble ());
					Vector3 parentN = new Vector3 ((float)reader.ReadDouble (), (float)reader.ReadDouble (), (float)reader.ReadDouble ());
					Vector3 childC = new Vector3 ((float)reader.ReadDouble (), (float)reader.ReadDouble (), (float)reader.ReadDouble ());
					Vector3 childN = new Vector3 ((float)reader.ReadDouble (), (float)reader.ReadDouble (), (float)reader.ReadDouble ());

					int hasLimit = reader.ReadByte ();
					//int angLimit = reader.ReadByte ();
					if (hasLimit != -1) {
							angLimLow = reader.ReadDouble ();
							angLimHigh = reader.ReadDouble ();
							
					} else {
							
							reader.ReadBytes (16);
					}
					reader.ReadBytes(108); //skips linear segment
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
					//Debug.Log(parentC);

			} else {
			Debug.Log (jointType);
			throw new System.Exception("Linear Joint type");
		}
		}

		
		// Use this for initialization
		void Start ()
		{
				generateCubes (2);
				HandleBXDA.loadBXDA ("C:\\Users\\t_waggn\\Documents\\bxda\\node_Part2_1.bxda", transform.GetChild (0));
				HandleBXDA.loadBXDA ("C:\\Users\\t_waggn\\Documents\\bxda\\node_Movement_1.bxda", transform.GetChild (1));
				//            

				loadBXDJ ("C:/Users/t_waggn/Documents/bxdj/skeleton.bxdj", transform.GetChild(0), transform.GetChild(1));
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}
}

