using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class auxFunctions : RigidNode_Base
{
    public delegate void HandleMesh(int id, BXDAMesh.BXDASubMesh subMesh, Mesh mesh);

    public static void ReadMeshSet(List<BXDAMesh.BXDASubMesh> meshes, HandleMesh handleMesh)
    {
		for (int j = 0; j < meshes.Count; j++)
        {
            BXDAMesh.BXDASubMesh sub = meshes[j];
            //takes all of the required information from the API (the API information is within "sub" above)
            Vector3[] vertices = sub.verts == null ? null : ArrayUtilities.WrapArray<Vector3>(
                delegate(double x, double y, double z)
                {
                    return new Vector3((float) x * .01f, (float) y * .01f, (float) z * .01f);
                }, sub.verts);

            Vector3[] normals = sub.norms == null ? null : ArrayUtilities.WrapArray<Vector3>(
                delegate(double x, double y, double z)
                {
                    return new Vector3((float) x, (float) y, (float) z);
                }, sub.norms);

            Mesh unityMesh = new Mesh();
            unityMesh.vertices = vertices;
            unityMesh.normals = normals;
            unityMesh.uv = new Vector2[vertices.Length];
            unityMesh.subMeshCount = sub.surfaces.Count;
            for (int i = 0; i < sub.surfaces.Count; i++)
            {
                unityMesh.SetTriangles(sub.surfaces[i].indicies, i);
            }
            unityMesh.RecalculateNormals();
            handleMesh(j, sub, unityMesh);
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


