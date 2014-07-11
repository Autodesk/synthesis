using UnityEngine;
using System.Collections;

public class UnityRigidNode : RigidNode_Base
{
		protected GameObject unityObject, collider;
		protected ConfigurableJoint joint;
		protected WheelDriverMeta wheel;
		private BXDAMesh mesh;
		private SoftJointLimit low, high;
		private float upper, lower;

		public void CreateTransform (Transform root)
		{
				unityObject = new GameObject ();
				unityObject.transform.parent = root;
				unityObject.transform.position = new Vector3 (0, 0, 0);
				//Destroy (gameObject.collider);
				unityObject.name = base.GetModelFileName (); 
		}

		private ConfigurableJoint ConfigJointInternal (Vector3 parentPos, Vector3 childPos, Vector3 axis)
		{
		
				GameObject rigid = ((UnityRigidNode)GetParent ()).unityObject;
				if (!rigid.gameObject.GetComponent<Rigidbody> ()) {
						rigid.gameObject.AddComponent<Rigidbody> ();
				}
				Rigidbody rigidB = rigid.gameObject.GetComponent<Rigidbody> ();
				//rigidB.isKinematic = true;
				joint = unityObject.gameObject.AddComponent<ConfigurableJoint> ();
				
				joint.connectedBody = rigidB;
				//joint.enableCollision = false;
		
				//configures the joint
				joint.anchor = parentPos;
				joint.connectedAnchor = childPos;
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
						joint = ConfigJointInternal (ConvertV3 (nodeR.parentBase), ConvertV3 (nodeR.childBase), ConvertV3 (nodeR.parentNormal));
						
						lower = nodeR.angularLimitLow * (180.0f / Mathf.PI);
						upper = nodeR.angularLimitHigh * (180.0f / Mathf.PI);
						joint.angularXMotion = nodeR.hasAngularLimit != false ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Free;
						//Debug.Log ("Limits: " + lower + " " + upper);
						if (joint.angularXMotion == ConfigurableJointMotion.Limited) {
								low.limit = lower;
								high.limit = upper;
								joint.lowAngularXLimit = low;
								joint.highAngularXLimit = high;
						}
						
						
						wheel = nodeX.cDriver != null ? nodeX.cDriver.GetInfo<WheelDriverMeta> () : null;
						if (wheel != null && wheel.position != WheelPosition.NO_WHEEL) {
								CreateWheel (nodeR);
						}
					
				} else if (nodeX.GetJointType () == SkeletalJointType.LINEAR) {
						LinearJoint_Base nodeL = (LinearJoint_Base)nodeX;
						
						joint = ConfigJointInternal (ConvertV3 (nodeL.parentBase), ConvertV3 (nodeL.childBase), ConvertV3 (nodeL.parentNormal));
						
						joint.xMotion = nodeL.parentNormal.x != 1 ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Limited;
						joint.yMotion = nodeL.parentNormal.y != 1 ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Limited;
						joint.zMotion = nodeL.parentNormal.z != 1 ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Limited;
				}
					
		}

		public void CreateMesh (string filePath)
		{
		

				mesh = new BXDAMesh ();
				mesh.ReadBXDA (filePath);
		
				int mCount = mesh.meshes.Count;
		
				//Init.generateCubes (mCount, gameObject.transform, gameObject.transform.name + " Subpart");
		
				for (int j = 0; j < mCount; j++) {
						GameObject subObject = new GameObject (unityObject.name + " Subpart" + j);
						subObject.transform.parent = unityObject.transform;
						subObject.transform.position = new Vector3 (0, 0, 0);

						BXDAMesh.BXDASubMesh sub = mesh.meshes [j];
				
			
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
				
						//Debug.Log (vertices [2]);
			
			
						//Seems to be mostly useless since they are already added by default
						subObject.AddComponent <MeshFilter> ();
						subObject.AddComponent <MeshRenderer> ();
						subObject.GetComponent<MeshFilter> ().mesh = unityMesh;
						subObject.GetComponent<MeshRenderer> ().material = new Material (Shader.Find ("Diffuse"));
				
						unityMesh.vertices = vertices;
						unityMesh.triangles = sub.indicies;
						unityMesh.normals = normals;
						unityMesh.colors32 = colors;
						unityMesh.uv = uvs;

						subObject.AddComponent<MeshCollider> ().convex = true;
				}

				if (!unityObject.GetComponent<Rigidbody> ()) {
						unityObject.AddComponent<Rigidbody> ();
				}
				Rigidbody rigidB = unityObject.GetComponent<Rigidbody> ();
				rigidB.mass = mesh.physics.mass;
				rigidB.centerOfMass = ConvertV3 (mesh.physics.centerOfMass);
		}
	
		private void CreateWheel (RotationalJoint_Base center)
		{
				//Debug.Log (wheel.position);
				collider = new GameObject (unityObject.name + " Collider");
				collider.transform.parent = GetParent () != null ? ((UnityRigidNode)GetParent ()).unityObject.transform : unityObject.transform;
				collider.transform.position = ConvertV3 (center.childBase);
				collider.AddComponent<WheelCollider> ();
				collider.GetComponent<WheelCollider> ().radius = wheel.radius + (wheel.radius * 0.5f);
				collider.GetComponent<WheelCollider> ().transform.Rotate (0, 0, 0);
				
				//parent.Rotate (new Vector3 (0, 90, 0));
			
				//I want the grandfather to have a rigidbody
				//unityObject.transform.gameObject.AddComponent<Rigidbody> ();
				//unityObject.transform.rigidbody.mass = 120;
		}

		public static Vector3 ConvertV3 (BXDVector3 vector)
		{
				Vector3 vectorUnity = new Vector3 ((float)vector.x, (float)vector.y, (float)vector.z);
				return vectorUnity;
		}







		public int GetPortA ()
		{
				if (base.GetSkeletalJoint () == null || base.GetSkeletalJoint ().cDriver == null) {
						return -1;			
				}
				return GetSkeletalJoint ().cDriver.portA;
		}

		public int GetPortB ()
		{
				if (joint != null || base.GetSkeletalJoint () == null || base.GetSkeletalJoint ().cDriver == null) {
						return -1;			
				}
				return GetSkeletalJoint ().cDriver.portB;
		}
		public WheelCollider GetWheelCollider() {
			return collider != null ? collider.GetComponent<WheelCollider> () : null;
		}

}

public class UnityRigidNodeFactory : RigidNodeFactory
{
		public RigidNode_Base Create ()
		{
				return new UnityRigidNode ();
		}
}

