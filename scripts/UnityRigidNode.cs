using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnityRigidNode : RigidNode_Base
{
	public GameObject unityObject, subObject, subCollider, wCollider;
	protected ConfigurableJoint joint;
	protected WheelDriverMeta wheel;
	private PhysicalProperties bxdPhysics;
	private SoftJointLimit low, high, linear;
	private float center, current;
    public MeshCollider meshCollider = new MeshCollider();

	
	public bool IsWheel
	{
		get
		{
			return (wheel != null && wheel.type != WheelType.NOT_A_WHEEL);
		}
	}

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
				
		axis.Normalize();
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
		
		wCollider = new GameObject(unityObject.name + " Collider");
		
		wCollider.transform.parent = GetParent() != null ? ((UnityRigidNode)GetParent()).unityObject.transform : unityObject.transform;
		wCollider.transform.position = auxFunctions.ConvertV3(wheel.center);
		wCollider.AddComponent<WheelCollider>();
		wCollider.GetComponent<WheelCollider>().radius = (wheel.radius * 1.10f) * 0.01f;
		wCollider.transform.localRotation *= Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(joint.axis.x, joint.axis.y, joint.axis.z));

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
			

	}
	//finds the difference between the current position, which is always one of the two end points, then finds the difference between the two. 
    //this is then divided by 2 to find the limit for unity
	private void LinearLimit(Dictionary<string, float> limit)
	{
		center = (limit ["end"] - limit ["start"]) / 2.0f;
		//also sets limit properties to eliminate any shaking and twitching from the joint when it hit sthe limit
		linear.limit = Mathf.Abs(center) * 0.01f;
		linear.bounciness = 1e-05f;
		linear.spring = 0f;
		linear.damper = 1e30f;
		joint.linearLimit = linear;

		
	}
    //assigns motors to the appropriate joint
	private void SetXDrives()
	{

        //if the node has a joint and driver
		if (GetSkeletalJoint() != null && GetSkeletalJoint().cDriver != null)
		{
			if (GetSkeletalJoint().cDriver.GetDriveType().IsPneumatic())
			{

				PneumaticDriverMeta pneum = GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>();
				if(pneum != null)
				{
				float psiToNMm2 = 0.00689475728f;
				JointDrive drMode = new JointDrive();
				drMode.mode = JointDriveMode.Velocity;
				drMode.maximumForce = (psiToNMm2 * pneum.pressurePSI) * (Mathf.PI * Mathf.Pow((pneum.widthMM / 2), 2));
				joint.xDrive = drMode;
				}
				
			} else if (GetSkeletalJoint().cDriver.GetDriveType().IsMotor())
			{
				JointDrive drMode = new JointDrive();
				drMode.mode = JointDriveMode.Velocity;
				drMode.maximumForce = 100.0f;
				joint.angularXDrive = drMode;
			}	
		}	
	}

	
	
	//creates the configurable joint then preforms the appropriate alterations based on the joint type
	public void CreateJoint()
	{

        
		if (joint != null || GetSkeletalJoint() == null)
		{
			return;			
		}
				
		//SkeletalJoint_Base GetSkeletalJoint() = GetSkeletalJoint();
		//this is the conditional for Identified wheels
		if (GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
		{
			//if the mesh contains information which identifies it as a wheel then create a wheel collider.
			wheel = GetSkeletalJoint().cDriver != null ? GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() : null;
			if (IsWheel) 
				FlipNorms();

			RotationalJoint_Base nodeR = (RotationalJoint_Base)GetSkeletalJoint();
					
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
            
			if (IsWheel)
			{
				CreateWheel(nodeR);
				subCollider.GetComponent<MeshCollider>().convex = false;
			}
			
					
		} else if (GetSkeletalJoint().GetJointType() == SkeletalJointType.CYLINDRICAL)
		{
			CylindricalJoint_Base nodeC = (CylindricalJoint_Base)GetSkeletalJoint();
						
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
				if (GetSkeletalJoint().cDriver != null && GetSkeletalJoint().cDriver.GetDriveType().IsPneumatic())
				{
					JointDrive drMode = new JointDrive();
					drMode.mode = JointDriveMode.Velocity;
					drMode.maximumForce = 100.0f;
					joint.xDrive = drMode;	
				}
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
			
			
		} else if (GetSkeletalJoint().GetJointType() == SkeletalJointType.LINEAR)
		{
			LinearJoint_Base nodeL = (LinearJoint_Base)GetSkeletalJoint();
			
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
		SetXDrives();
	}		
		
	//loads the bxda format meshes
	public void CreateMesh(string filePath)
	{
		BXDAMesh mesh = new BXDAMesh();
		mesh.ReadFromFile(filePath);
		
		auxFunctions.ReadMeshSet(mesh.meshes, delegate(int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
		{
			//new gameobject is made for the submesh

			subObject = new GameObject(unityObject.name + " Subpart" + id);
			//it is passively assigned as a child to the root transform 
			subObject.transform.parent = unityObject.transform;
			subObject.transform.position = new Vector3(0, 0, 0);

			subObject.AddComponent <MeshFilter>();
			subObject.GetComponent<MeshFilter>().mesh = meshu;
			subObject.AddComponent <MeshRenderer>();
			Material[] matls = new Material[meshu.subMeshCount];
			for (int i = 0; i < matls.Length; i++)
			{
				uint val = sub.surfaces [i].hasColor ? sub.surfaces [i].color : 0xFFFFFFFF;
				Color color = new Color32((byte)(val & 0xFF), (byte)((val >> 8) & 0xFF), (byte)((val >> 16) & 0xFF), (byte)((val >> 24) & 0xFF));
				if (sub.surfaces [i].transparency != 0)
				{
					color.a = sub.surfaces [i].transparency;
				} else if (sub.surfaces [i].translucency != 0)
				{
					color.a = sub.surfaces [i].translucency;
				}
				if (color.a == 0)   // No perfectly transparent things plz.
				{
					color.a = 1;
				}
				matls [i] = new Material((Shader)Shader.Instantiate(Shader.Find((color.a != 1 ? "Transparent/" : "") + (sub.surfaces[i].specular > 0 ? "Specular" : "Diffuse"))));
				matls [i].SetColor("_Color", color);
                if (sub.surfaces[i].specular > 0)
                {
                    matls[i].SetFloat("_Shininess", sub.surfaces[i].specular);
                }
			}
			subObject.GetComponent<MeshRenderer>().materials = matls;

			if (!unityObject.GetComponent<Rigidbody>())
			{
				unityObject.AddComponent<Rigidbody>();
                unityObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
			}
		});	
				
		auxFunctions.ReadMeshSet(mesh.colliders, delegate(int id, BXDAMesh.BXDASubMesh useless, Mesh meshu)
		{


			//Debug.Log (unityObject.name + " " + id + " tris: " + meshu.triangles.Length / 3 + " Vertices: " + meshu.vertexCount);
			subCollider = new GameObject(unityObject.name + " Subcollider" + id);
			subCollider.transform.parent = unityObject.transform;
			subCollider.transform.position = new Vector3(0, 0, 0);
			if (meshu.triangles.Length == 0 && meshu.vertices.Length == 2)
			{
				BoxCollider box = subCollider.AddComponent<BoxCollider>();
				Vector3 center = (meshu.vertices [0] + meshu.vertices [1]) * 0.5f;
				box.center = center;
				box.size = meshu.vertices [1] - center;
			} else
			{
			
				subCollider.AddComponent<MeshCollider>().sharedMesh = meshu;
				//Debug.Log(IsWheel);
				subCollider.GetComponent<MeshCollider>().convex = true;
                meshCollider = subCollider.GetComponent<MeshCollider>();

				
			}
         
		});
	
		Rigidbody rigidB = unityObject.GetComponent<Rigidbody>();
		rigidB.mass = mesh.physics.mass;
		rigidB.centerOfMass = auxFunctions.ConvertV3(mesh.physics.centerOfMass);
				
		bxdPhysics = mesh.physics;
		// Free mesh.
        mesh = null;
	}

	public ConfigurableJoint GetConfigJoint()
	{
		return joint != null ? joint : null;
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
		return centerOfMass;
	}

	public static BXDVector3 comLOL(RigidNode_Base kk) {
		if (kk is UnityRigidNode && ((UnityRigidNode)kk).bxdPhysics != null)
		{
			return ((UnityRigidNode)kk).bxdPhysics.centerOfMass;
		}
		return null;//breakit
	}

	public void FlipNorms()
	{
		//TotalCenterOfMass(unityObject.transform.parent.gameObject);
		//Debug.Log(unityObject.transform.parent.gameObject);
		if (GetParent() != null && GetSkeletalJoint() != null && GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
		{
			Vector3 com = auxFunctions.ConvertV3(comLOL(GetParent()));
			RotationalJoint_Base rJoint = (RotationalJoint_Base)GetSkeletalJoint();
			Vector3 diff = auxFunctions.ConvertV3(rJoint.basePoint) - com;
			Debug.DrawLine(unityObject.transform.parent.localToWorldMatrix * auxFunctions.ConvertV3(rJoint.basePoint), unityObject.transform.parent.localToWorldMatrix * com, Color.red);
			double dot = Vector3.Dot(diff, auxFunctions.ConvertV3(rJoint.axis));
			if (dot < 0)
			{
				Debug.Log("Invert " + unityObject.name);
				//unityObject.GetComponent<WheelCollider>().transform.Rotate(new Vector3(0,90,0));
				//wCollider.transform.Rotate(new Vector3(0,90,0));
				//wCollider.transform.localRotation *= Quaternion.FromToRotation(
				//	new Vector3(rJoint.axis.x,rJoint.axis.y,rJoint.axis.z), new Vector3(-rJoint.axis.x,-rJoint.axis.y,-rJoint.axis.z));
				rJoint.axis = rJoint.axis.Multiply(-1);
			}
		}
	}


}
