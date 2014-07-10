using UnityEngine;
using System.Collections;

public class UnityRigidNode : RigidNode_Base
{
		protected GameObject unityObject;
		protected ConfigurableJoint joint;
		private BXDAMesh mesh;
		protected WheelDriverMeta wheel;

		public void CreateTransform (Transform root)
		{
				unityObject = new GameObject ();
				unityObject.transform.parent = root;
				unityObject.transform.position = new Vector3 (0, 0, 0);
				//Destroy (gameObject.collider);
				unityObject.name = base.GetModelFileName ();
		}

		public void CreateJoint ()
		{
				if (joint != null || base.GetSkeletalJoint () == null) {
						return;			
				}
				SkeletalJoint_Base nodeX = GetSkeletalJoint ();
				
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
					
					
						wheel = nodeX.cDriver != null ? nodeX.cDriver.GetInfo<WheelDriverMeta> () : null;
						Debug.Log ("testWheel: " + wheel);
					
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
						joint.anchor = parentC;
						joint.connectedAnchor = childC;
						joint.axis = parentN;
						//joint.secondaryAxis = new Vector3 (0, 0, 1);
					
						joint.angularXMotion = ConfigurableJointMotion.Free;
						joint.angularYMotion = ConfigurableJointMotion.Locked;
						joint.angularZMotion = ConfigurableJointMotion.Locked;
						joint.xMotion = ConfigurableJointMotion.Locked;
						joint.yMotion = ConfigurableJointMotion.Locked;
						joint.zMotion = ConfigurableJointMotion.Locked;
					
						
					if (wheel != null && wheel.position != WheelPosition.NO_WHEEL) {
						CreateWheel(nodeR);
						joint.angularXMotion = ConfigurableJointMotion.Locked;
						
					}
					
					 	
						

					
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

						subObject.AddComponent<MeshCollider>().convex = true;
		}
		}
	
		private void CreateWheel(RotationalJoint_Base center)
		{
			//Debug.Log (wheel.position);
			GameObject collider = new GameObject (unityObject.name + " Collider");
			collider.transform.parent = unityObject.transform;
			collider.transform.position = Init.ConvertV3(center.childBase);
			collider.AddComponent<WheelCollider> ();
			collider.GetComponent<WheelCollider> ().radius = wheel.radius + 0.2f;
			collider.GetComponent<WheelCollider> ().transform.Rotate (0, 0, 0);

			//parent.Rotate (new Vector3 (0, 90, 0));
			
			//I want the grandfather to have a rigidbody
			//unityObject.transform.gameObject.AddComponent<Rigidbody> ();
			unityObject.transform.rigidbody.mass = 120;
	}	
}

public class UnityRigidNodeFactory : RigidNodeFactory
{
		public RigidNode_Base Create ()
		{
				return new UnityRigidNode ();
		}
}
