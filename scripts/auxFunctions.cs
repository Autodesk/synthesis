using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class auxFunctions : RigidNode_Base
{

	public delegate void HandleMesh (int id,Mesh mesh);

	public static void ReadMeshSet (List<BXDAMesh.BXDASubMesh> meshes, HandleMesh handleMesh)
	{
		for (int j = 0; j < meshes.Count; j++) {			
			BXDAMesh.BXDASubMesh sub = meshes [j];
			
			
			//takes all of the required information from the API (the API information is within "sub" above)
			Vector3[] vertices = sub.verts == null ? null : ArrayUtilities.WrapArray<Vector3> (
				delegate(double x, double y, double z) {
				return new Vector3 ((float)x, (float)y, (float)z);
			}, sub.verts);
			
			Vector3[] normals = sub.norms == null ? null : ArrayUtilities.WrapArray<Vector3> (
				delegate(double x, double y, double z) {
				return new Vector3 ((float)x, (float)y, (float)z);
			}, sub.norms);
			Color32[] colors = sub.colors == null ? null : ArrayUtilities.WrapArray<Color32> (
				delegate(byte r, byte g, byte b, byte a) {
				return new Color32 (r, g, b, a);
			}, sub.colors);
			Vector2[] uvs = sub.textureCoords == null ? null : ArrayUtilities.WrapArray<Vector2> (
				delegate(double x, double y) {
				return new Vector2 ((float)x, (float)y);
			}, sub.textureCoords);
			
			Mesh unityMesh = new Mesh ();
			unityMesh.vertices = vertices;
			unityMesh.triangles = sub.indicies;
			unityMesh.normals = normals;
			unityMesh.colors32 = colors;
			unityMesh.uv = uvs;
			handleMesh (j, unityMesh);
		}


	}
	
	public static Vector3 ConvertV3 (BXDVector3 vector)
	{
		return new Vector3 ((float)vector.x, (float)vector.y, (float)vector.z);
	}
	
	public static Quaternion FlipRobot(List<Vector3> wheels, Transform parent)
	{
	
		Vector3 norm = Vector3.Cross((wheels[1] - wheels[0]),(wheels[2] - wheels[0]));
		Vector3 com = UnityRigidNode.TotalCenterOfMass(parent.gameObject);
		
		Vector3 above = Vector3.Cross((wheels[0] - com),norm);
		
		
		norm = norm * ((above.y < 0) ? -1 : 1);
		//Debug.Log(above + ": "  + norm);
		
		Quaternion q = new Quaternion();
		q.SetFromToRotation(norm, new Vector3(0,1,0));
		
		
		return q;
		
	}
}


