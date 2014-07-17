using UnityEngine;
using System.Collections;

public class UnityRigidNode : RigidNode_Base
{
		protected GameObject unityObject, collider, subObject;
		protected ConfigurableJoint joint;
		protected WheelDriverMeta wheel;
		private BXDAMesh mesh;
		private SoftJointLimit low, high, linear;
		private float center, current;

		//The root transform for the whole object model is determined in this constructor passively
		public void CreateTransform (Transform root)
		{
				unityObject = new GameObject ();
				unityObject.transform.parent = root;
				unityObject.transform.position = new Vector3 (0, 0, 0);
				unityObject.name = base.GetModelFileName (); 
		}

		//creates a uniform configurable joint which can be altered through conditionals.
		private ConfigurableJoint ConfigJointInternal (Vector3 pos, Vector3 axis)
		{
		
				GameObject rigid = ((UnityRigidNode)GetParent ()).unityObject;
				if (!rigid.gameObject.GetComponent<Rigidbody> ()) {
						rigid.gameObject.AddComponent<Rigidbody> ();
				}
				Rigidbody rigidB = rigid.gameObject.GetComponent<Rigidbody> ();
				joint = unityObject.gameObject.AddComponent<ConfigurableJoint> ();
				
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
				return joint;
		}
		//creates a wheel collider and centers it on the current transform
		private void CreateWheel (RotationalJoint_Base center)
		{
				collider = new GameObject (unityObject.name + " Collider");
				collider.transform.parent = GetParent () != null ? ((UnityRigidNode)GetParent ()).unityObject.transform : unityObject.transform;
				collider.transform.position = ConvertV3 (center.basePoint);
				collider.AddComponent<WheelCollider> ();
				collider.GetComponent<WheelCollider> ().radius = wheel.radius + (wheel.radius * 0.15f);
				collider.GetComponent<WheelCollider> ().transform.Rotate (90, 0, 0);
		
				//I want the grandfather to have a rigidbody
				
		}

		//converts inventor's limit information to the modular system unity uses (180/-180)
		private void AngularLimit (float[] limit)
		{
				
				for (int i = 0; i < limit.Length; i++) {
						if ((limit [2] - limit [1]) >= Mathf.Abs (360.0f)) {
								joint.angularXMotion = ConfigurableJointMotion.Free;
								return;
						}	
						limit [i] = (Mathf.Abs (limit [i]) > 180.0f) ? 360.0f - Mathf.Abs (limit [i]) : limit [i];  
						//Debug.Log ("Value: " + limit [i] + " limit index: " + i);
				}
				low.limit = limit [0] == limit [1] ? limit [0] - limit [1] : limit [1];
				high.limit = limit [0] == limit [2] ? limit [0] - limit [2] : limit [2];
				joint.lowAngularXLimit = low;
				joint.highAngularXLimit = high;	
		}
		
		private void LinearLimit (float[] limit)
		{
				center = (limit [0] - limit [1]) / 2.0f;
				current = limit [2] - center;

				subObject.transform.position = joint.axis * current;
				linear.limit = Mathf.Abs (center);
				joint.linearLimit = linear;
	
		}
	
		//creates the configurable joint then preforms the appropriate alterations based on the joint type
		public void CreateJoint ()
		{
				if (joint != null || base.GetSkeletalJoint () == null) {
						return;			
				}
				SkeletalJoint_Base nodeX = GetSkeletalJoint ();
				
				//this is the conditional for Identified wheels
				if (nodeX.GetJointType () == SkeletalJointType.ROTATIONAL) {
					
						RotationalJoint_Base nodeR = (RotationalJoint_Base)nodeX;
						
						//takes the x, y, and z axis information from a custom vector class to unity's vector class
						joint = ConfigJointInternal (ConvertV3 (nodeR.basePoint), ConvertV3 (nodeR.axis));
						joint.angularXMotion = !nodeR.hasAngularLimit ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited;
						
						if (joint.angularXMotion == ConfigurableJointMotion.Limited) {
								float[] aLimit = {
										nodeR.currentAngularPosition * (180.0f / Mathf.PI),
										nodeR.angularLimitLow * (180.0f / Mathf.PI),
										nodeR.angularLimitHigh * (180.0f / Mathf.PI)
								};

								AngularLimit (aLimit);
							
						}
						//if the mesh contains information which identifies it as a wheel then create a wheel collider.
						wheel = nodeX.cDriver != null ? nodeX.cDriver.GetInfo<WheelDriverMeta> () : null;
						if (wheel != null && wheel.type != WheelType.NOT_A_WHEEL) {
								
								//don't worry, I'm a doctor
								JointDrive drMode = new JointDrive ();
								drMode.mode = JointDriveMode.Velocity;
								CreateWheel (nodeR);
								joint.angularXDrive = drMode;	
						}
					
				} else if (nodeX.GetJointType () == SkeletalJointType.CYLINDRICAL) {
						CylindricalJoint_Base nodeC = (CylindricalJoint_Base)nodeX;
						
						joint = ConfigJointInternal (ConvertV3 (nodeC.basePoint), ConvertV3 (nodeC.axis));
						
	
						joint.xMotion = ConfigurableJointMotion.Limited;
						joint.angularXMotion = !nodeC.hasAngularLimit ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited;

						Debug.Log ("Start: " + nodeC.linearLimitStart + " End: " + nodeC.linearLimitEnd);
						float[] lLimit = {
								nodeC.linearLimitEnd,
								nodeC.linearLimitStart,
								nodeC.currentLinearPosition
						};
						LinearLimit (lLimit);
						

						Debug.Log ("Center: " + center + " Current Distance: " + current);
						//if (joint.angularXMotion == ConfigurableJointMotion.Limited) {
						float[] aLimit = {
								nodeC.currentAngularPosition * (180.0f / Mathf.PI),
								nodeC.angularLimitLow * (180.0f / Mathf.PI),
								nodeC.angularLimitHigh * (180.0f / Mathf.PI)
						};
						AngularLimit (aLimit);
						//Debug.Log (low.limit + " " + high.limit);
											
			
						//	}
						
				} else if (nodeX.GetJointType () == SkeletalJointType.LINEAR) {
						LinearJoint_Base nodeL = (LinearJoint_Base)nodeX;
			
						joint = ConfigJointInternal (ConvertV3 (nodeL.basePoint), ConvertV3 (nodeL.axis));
			
			
						joint.xMotion = ConfigurableJointMotion.Limited;

						float[] lLimit = {
								nodeL.linearLimitLow,
								nodeL.linearLimitHigh,
								nodeL.currentLinearPosition
						};
						LinearLimit (lLimit);
						
				}
					
		}
		//loads the bxda format meshes
		public void CreateMesh (string filePath)
		{
		

				mesh = new BXDAMesh ();
				mesh.ReadBXDA (filePath);
		
				int mCount = mesh.meshes.Count;

		
				for (int j = 0; j < mCount; j++) {
						//new gameobject is made for the submesh
						subObject = new GameObject (unityObject.name + " Subpart" + j);
						//it is passively assigned as a child to the root transform 
						subObject.transform.parent = unityObject.transform;
						subObject.transform.position = new Vector3 (0, 0, 0);

						BXDAMesh.BXDASubMesh sub = mesh.meshes [j];
				
						//takes all of the required information from the API (the API information is within "sub" above)
						Vector3[] vertices = ArrayUtilities.WrapArray<Vector3> (
				delegate(double x, double y, double z) {
								return new Vector3 ((float)x, (float)y, (float)z);
						}, sub.verts);
						Vector3[] normals = ArrayUtilities.WrapArray<Vector3> (
				delegate(double x, double y, double z) {
								return new Vector3 ((float)x, (float)y, (float)z);
						}, sub.norms);
						Color32[] colors = ArrayUtilities.WrapArray<Color32> (
				delegate(byte r, byte g, byte b, byte a) {
								return new Color32 (r, g, b, a);
						}, sub.colors);
						Vector2[] uvs = ArrayUtilities.WrapArray<Vector2> (
				delegate(double x, double y) {
								return new Vector2 ((float)x, (float)y);
						}, sub.textureCoords);
			
						Mesh unityMesh = new Mesh ();
				
						
			
			
						//this the gameobject which contains the components
						subObject.AddComponent <MeshFilter> ();
						subObject.AddComponent <MeshRenderer> ();
						subObject.GetComponent<MeshFilter> ().mesh = unityMesh;
						subObject.GetComponent<MeshRenderer> ().material = new Material (Shader.Find ("Diffuse"));
						//the mesh is strictly for rendering | for serious, it would be irresponsible to give joints to children
						unityMesh.vertices = vertices;
						unityMesh.triangles = sub.indicies;
						unityMesh.normals = normals;
						unityMesh.colors32 = colors;
						unityMesh.uv = uvs;

						subObject.AddComponent<BoxCollider> ();
				}
				//if the object doesn't have a rigidbody then attach one
				if (!unityObject.GetComponent<Rigidbody> ()) {
						unityObject.AddComponent<Rigidbody> ();
				}
				Rigidbody rigidB = unityObject.GetComponent<Rigidbody> ();
				rigidB.mass = mesh.physics.mass;
				rigidB.centerOfMass = ConvertV3 (mesh.physics.centerOfMass);
				//rigidB.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}


		//These are all of the public functions which have varying uses. Mostly "get" functions, but convertV3 is especially useful.
		

		//converts BXDVectors to the unity vector3 type
		public static Vector3 ConvertV3 (BXDVector3 vector)
		{
				return new Vector3 ((float)vector.x, (float)vector.y, (float)vector.z);
		}

		//portA used mostly for drive controls. Allows for proper motor simulation
		public int GetPortA ()
		{
				if (base.GetSkeletalJoint () == null || base.GetSkeletalJoint ().cDriver == null) {
						return -1;			
				}
				return GetSkeletalJoint ().cDriver.portA;
		}
		
		public WheelCollider GetWheelCollider ()
		{
				return collider != null ? collider.GetComponent<WheelCollider> () : null;
		}

		public ConfigurableJoint GetConfigJoint ()
		{
				return joint != null ? joint : null;
		}

}
//This allows the rigidnode_base to be loaded with unityrigidnode_base information
public class UnityRigidNodeFactory : RigidNodeFactory
{
		public RigidNode_Base Create ()
		{
				return new UnityRigidNode ();
		}
}

