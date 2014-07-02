using UnityEngine;
using System.Collections;
using System.IO;

public class HandleBXDA : MonoBehaviour 
{
	public static void loadBXDA(string filepath, Transform trans)
	{
		int vCount;
		int tCount;
		
		BinaryReader bxda = new BinaryReader(File.Open(filepath,FileMode.Open));
		
		vCount = bxda.ReadInt32();
		
		Vector3[] vertices = new Vector3[vCount];
		Vector3[] normals = new Vector3[vCount];
		Color32[] colors = new Color32[vCount];
		Vector2[] uvs = new Vector2[vCount];
		
		for (int i = 0; i < vCount; i++)
		{
			vertices[i] = new Vector3((float) bxda.ReadDouble(),(float) bxda.ReadDouble(),(float) bxda.ReadDouble());
			
			normals[i] = new Vector3((float) bxda.ReadDouble(),(float) bxda.ReadDouble(),(float) bxda.ReadDouble());
			
			colors[i] = new Color32(bxda.ReadByte(),bxda.ReadByte(),bxda.ReadByte(),bxda.ReadByte());
			
			uvs[i] = new Vector2((float) bxda.ReadDouble(), (float) bxda.ReadDouble());
		}
		
		tCount = bxda.ReadInt32();
		int[] triangles = new int[tCount*3];
		
		for (int i = 0; i < tCount*3; i++) 
		{
			triangles[i] = bxda.ReadInt32();
		}
		
		Mesh mesh = new Mesh ();
		trans.gameObject.AddComponent("Mesh Filter");
		trans.gameObject.AddComponent ("MeshRender");
		trans.GetComponent<MeshFilter>().mesh = mesh;
		
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.colors32 = colors;
		mesh.uv = uvs;
		
	}


}
