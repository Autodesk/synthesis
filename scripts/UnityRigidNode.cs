using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnityRigidNode : RigidNode_Base
{
	protected GameObject unityObject, subObject, subCollider, wCollider;
	protected ConfigurableJoint joint;
	protected WheelDriverMeta wheel;
	private BXDAMesh mesh;
	private SoftJointLimit low, high, linear;
	private float center, current;
	public static Vector3 wheelData;
	
	


	//public delegate void Action(); //reminder of how action and function work
		
	//The root transform for the whole object model is determined in this constructor passively
	public void CreateTransform(Transform root)
	{
		unityObject = new GameObject();
		unityObject.transform.parent = root;
		unityObject.transform.position = new Vector3(0, 0, 0);
		unityObject.name = base.modelFileName; 
	}

	//creates a uniform configurable joint which can be altered through conditionals.
	private ConfigurableJoint ConfigJointInternal(Vector3 pos, Vector3 axis, Action<ConfigurableJoint> jointType)
	{
		
		GameObject rigid = ((UnityRigidNode)GetParent()).unityObject;
		if (!rigid.gameObject.GetComponent<Rigidbody>())
		{
			rigid.gameObject.AddComponent<Rigidbody>();
		}
		Rigidbody rigidB = rigid.gameObject.GetComponent<Rigidbody>();
		joint = unityObject.gameObject.AddComponent<ConfigurableJoint>();
				
		joint.connectedBody = rigidB;

		//configures the joint
		joint.anchor = pos;
		joint.connectedAnchor = pos;
				
		joint.axis = axis;
		
		//joint.secondaryAxis = new Vector3 (0, 0, 1);
		
		joint.angularXMotion = ConfigurableJointMotion.Locked;
		joint.angularYMotion = ConfigurableJointMotion.Locked;
		joint.angularZMotion = ConfigurableJointMotion.Locked;
		joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;
		jointType(joint);
		return joint;
	}
	//creates a wheel collider and centers it on the current transform
	private void CreateWheel(RotationalJoint_Base center)
	{
		
		Quaternion q = new Quaternion();
		q.SetFromToRotation(new Vector3(1, 0, 0), joint.axis);

		wCollider = new GameObject(unityObject.name + " Collider");
				
		wCollider.transform.parent = GetParent() != null ? ((UnityRigidNode)GetParent()).unityObject.transform : unityObject.transform;
		wCollider.transform.position = auxFunctions.ConvertV3(wheel.center);
		wCollider.AddComponent<WheelCollider>();
		wCollider.GetComponent<WheelCollider>().radius = wheel.radius + (wheel.radius * 0.15f);
		wCollider.GetComponent<WheelCollider>().transform.Rotate(90, 0, 0);
		wCollider.transform.localRotation *= q;
		
		//I want the grandfather to have a rigidbody
				
	}

	//converts inventor's limit information to the modular system unity uses (180/-180)
	private void AngularLimit(float[] limit)
	{
		
		if ((limit [2] - limit [1]) >= Mathf.Abs(360.0f))
		{
			joint.angularXMotion = ConfigurableJointMotion.Free;
			return;
		} else
		{
			limit [1] = (limit [2] - limit [1]) / 2.0f;
			limit [2] = limit [1] < 0.0f ? Mathf.Abs(limit [1]) : -(limit [1]);  
			low.limit = limit [1];
			high.limit = limit [2];
		}
				 		

		joint.lowAngularXLimit = low;
		joint.highAngularXLimit = high;	

		JointDrive drMode = new JointDrive();
		drMode.mode = JointDriveMode.Velocity;
		drMode.maximumForce = 100.0f;
		joint.angularXDrive = drMode;	

	}
	
	private void LinearLimit(Dictionary<string, float> limit)
	{
		center = (limit ["end"] - limit ["start"]) / 2.0f;
		//current = limit ["current"];
		//Debug.Log("center: " + center + " current: " + current);
				
		linear.limit = Mathf.Abs(center);
		joint.linearLimit = linear;

		JointDrive drMode = new JointDrive();
		drMode.mode = JointDriveMode.Velocity;
		drMode.maximumForce = 100.0f;
		joint.xDrive = drMode;	

		
	}
	
	
	//creates the configurable joint then preforms the appropriate alterations based on the joint type
	public void CreateJoint()
	{
		if (joint != null || GetSkeletalJoint() == null)
		{
			return;			
		}
				
		SkeletalJoint_Base nodeX = GetSkeletalJoint();
		//this is the conditional for Identified wheels
		if (nodeX.GetJointType() == SkeletalJointType.ROTATIONAL)
		{
					
			RotationalJoint_Base nodeR = (RotationalJoint_Base)nodeX;
					
			//takes the x, y, and z axis information from a custom vector class to unity's vector class
			joint = ConfigJointInternal(auxFunctions.ConvertV3(nodeR.basePoint), auxFunctions.ConvertV3(nodeR.axis), delegate(ConfigurableJoint jointSub)
			{
				jointSub.angularXMotion = !nodeR.hasAngularLimit ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited;
				if (joint.angularXMotion == ConfigurableJointMotion.Limited)
				{
					float[] aLimit = {
											nodeR.currentAngularPosition * (180.0f / Mathf.PI),
											nodeR.angularLimitLow * (180.0f / Mathf.PI),
											nodeR.angularLimitHigh * (180.0f / Mathf.PI)
										};
					AngularLimit(aLimit);
				}
			});
			//don't worry, I'm a doctor
						
			//if the mesh contains information which identifies it as a wheel then create a wheel collider.
			wheel = nodeX.cDriver != null ? nodeX.cDriver.GetInfo<WheelDriverMeta>() : null;
			if (wheel != null && wheel.type != WheelType.NOT_A_WHEEL)
			{
				CreateWheel(nodeR);	
				wheelData = auxFunctions.ConvertV3(wheel.center);
				
			}
					
		} else if (nodeX.GetJointType() == SkeletalJointType.CYLINDRICAL)
		{
			CylindricalJoint_Base nodeC = (CylindricalJoint_Base)nodeX;
						
			joint = ConfigJointInternal(auxFunctions.ConvertV3(nodeC.basePoint), auxFunctions.ConvertV3(nodeC.axis), delegate(ConfigurableJoint jointSub)
			{
				joint.xMotion = ConfigurableJointMotion.Limited;
				joint.angularXMotion = !nodeC.hasAngularLimit ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited;
				Dictionary<string, float> lLimit = new Dictionary<string, float>() {
															{"end",nodeC.linearLimitEnd},
															{"start",nodeC.linearLimitStart},
															{"current",nodeC.currentLinearPosition}
								};
				LinearLimit(lLimit);
				if (joint.angularXMotion == ConfigurableJointMotion.Limited)
				{
					float[] aLimit = {
												nodeC.currentAngularPosition * (180.0f / Mathf.PI),
												nodeC.angularLimitLow * (180.0f / Mathf.PI),
												nodeC.angularLimitHigh * (180.0f / Mathf.PI)
								};
					AngularLimit(aLimit);
				}
			});
			
			
		} else if (nodeX.GetJointType() == SkeletalJointType.LINEAR)
		{
			LinearJoint_Base nodeL = (LinearJoint_Base)nodeX;
			
			joint = ConfigJointInternal(auxFunctions.ConvertV3(nodeL.basePoint), auxFunctions.ConvertV3(nodeL.axis), delegate(ConfigurableJoint jointSub)
			{
				joint.xMotion = ConfigurableJointMotion.Limited;

				Dictionary<string, float> lLimit = new Dictionary<string, float>() {
												{"end",nodeL.linearLimitHigh},
												{"start",nodeL.linearLimitLow},
												{"current",nodeL.currentLinearPosition}
						};
				LinearLimit(lLimit);
			});
						
		}
	}		
		
	//loads the bxda format meshes
	public void CreateMesh(string filePath)
	{
		mesh = new BXDAMesh();
		mesh.ReadBXDA(filePath);
		
		auxFunctions.ReadMeshSet(mesh.meshes, delegate(int id, Mesh meshu)
		{
			//new gameobject is made for the submesh

			subObject = new GameObject(unityObject.name + " Subpart" + id);
			//it is passively assigned as a child to the root transform 
			subObject.transform.parent = unityObject.transform;
			subObject.transform.position = new Vector3(0, 0, 0);

			subObject.AddComponent <MeshFilter>();
			subObject.GetComponent<MeshFilter>().mesh = meshu;
			subObject.AddComponent <MeshRenderer>();
			subObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/VertexColors"));

			if (!unityObject.GetComponent<Rigidbody>())
			{
				unityObject.AddComponent<Rigidbody>();
			}
		});	
				
		auxFunctions.ReadMeshSet(mesh.colliders, delegate(int id, Mesh meshu)
		{
			//Debug.Log (unityObject.name + " " + id + " tris: " + meshu.triangles.Length / 3 + " Vertices: " + meshu.vertexCount);
			subCollider = new GameObject(unityObject.name + " Subcollider" + id);
			subCollider.transform.parent = unityObject.transform;
			subCollider.transform.position = new Vector3(0, 0, 0);
			subCollider.AddComponent<MeshCollider>().sharedMesh = meshu;
			subCollider.GetComponent<MeshCollider>().convex = true;

		});
				
		Rigidbody rigidB = unityObject.GetComponent<Rigidbody>();
		rigidB.mass = mesh.physics.mass;
		rigidB.centerOfMass = auxFunctions.ConvertV3(mesh.physics.centerOfMass);
				
		
	}
	//These are all of the public functions which have varying uses. Mostly "get" functions, but convertV3 is especially useful.
		

	//portA used mostly for drive controls. Allows for proper motor simulation
	public int GetPortA()
	{
		if (base.GetSkeletalJoint() == null || base.GetSkeletalJoint().cDriver == null)
		{
			return -1;			
		}
		return GetSkeletalJoint().cDriver.portA;
	}

	public int GetPortB()
	{
		if (base.GetSkeletalJoint() == null || base.GetSkeletalJoint().cDriver == null)
		{
			return -1;			
		}
		return GetSkeletalJoint().cDriver.portB;
	}

	public WheelCollider GetWheelCollider()
	{
		return wCollider != null ? wCollider.GetComponent<WheelCollider>() : null;
	}

	public ConfigurableJoint GetConfigJoint()
	{
		return joint != null ? joint : null;
	}
	public Vector3 GetWheelCenter()
	{
		return wheelData;
	}
	
	
	// Returns the center of mass of the skeleton. It calculates a weighted average of all the rigiBodies in the gameObject. (Its an average of their positions, weighted by the masses of each rigidBody)
	public static Vector3 TotalCenterOfMass(GameObject gameObj)
	{
		Vector3 centerOfMass = Vector3.zero;
		float sumOfAllWeights = 0f;

		Rigidbody[] rigidBodyArray = gameObj.GetComponentsInChildren<Rigidbody>();
		
		foreach (Rigidbody rigidBase in rigidBodyArray)
		{
			centerOfMass += rigidBase.worldCenterOfMass * rigidBase.mass;
			sumOfAllWeights += rigidBase.mass;
		}
		centerOfMass /= sumOfAllWeights;
		//Debug.Log(centerOfMass.ToString());
		return centerOfMass;
	}

	public void FlipNorms()
	{
		Vector3 com = TotalCenterOfMass(unityObject.transform.parent.gameObject);
		
		if (GetParent() != null && GetSkeletalJoint() != null && GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
		{
			RotationalJoint_Base rJoint = (RotationalJoint_Base)GetSkeletalJoint();
			Vector3 diff = auxFunctions.ConvertV3(rJoint.basePoint) - com;
			double dot = Vector3.Dot(diff, auxFunctions.ConvertV3(rJoint.axis));
			if (dot < 0)
			{
				rJoint.axis = rJoint.axis.Multiply(-1);
			}
		}
	}


}
//This allows the rigidnode_base to be loaded with unityrigidnode_base information
public class UnityRigidNodeFactory : RigidNodeFactory
{
	public RigidNode_Base Create()
	{
		return new UnityRigidNode();
	}
}

