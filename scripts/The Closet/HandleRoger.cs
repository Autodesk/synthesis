using UnityEngine;
using System.Collections;
using System.IO;

//Roger is in the closet


public class HandleRoger : MonoBehaviour 
{
	void loadBXDA(string filepath, Transform trans)
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

	//creates primitive meshes/objects as children of this.transform and removes their colliders
	//these are later turned into robot part meshes
	void generateCubes(int amt)
	{
		GameObject tmp;
		for (int i = 0; i < amt; i++) 
		{
			tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
			tmp.transform.parent = this.transform;
			tmp.transform.position = new Vector3(0,0,0);
			Destroy(tmp.gameObject.collider);
			tmp.name = "part" + i;
		}
	}

	//attaches convex mesh colliders to all children of specified transform object
	//pass this.transform to get all parts
	void attachMeshColliders(Transform parent)
	{
		MeshCollider tmp;
		foreach (Transform child in parent) 
		{
			child.gameObject.AddComponent<MeshCollider>();
			tmp = child.gameObject.GetComponent<MeshCollider>();
			tmp.convex = true;
		}
	}

	void attachRigidBodies(Transform parent)
	{
		foreach (Transform child in parent) 
		{
			child.gameObject.layer = 9;
			child.gameObject.AddComponent<Rigidbody>();
		}
	}
	void ignoreIntermeshCollisions(Transform parent, Transform ignore)
	{
		foreach (Transform child in parent) 
		{
			Physics.IgnoreCollision(ignore.collider, child.collider, true);
		}
	}
	void betaHinge(Transform owner, Transform connected, Vector3 anchorCoor)
	{
		HingeJoint OwnerHinge;
		OwnerHinge = owner.gameObject.AddComponent<HingeJoint> ();
		OwnerHinge.connectedBody = connected.gameObject.rigidbody;
		OwnerHinge.anchor = anchorCoor;
	}

	void Start () 
	{
        //string[] filepaths = {"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_AndyMarkWheelEdgeRightTop.bxda"};
		string[] filepaths = {"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_AndyMarkWheelEdgeRightTop.bxda",
			"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_AndyMarkWheelEdgeRightBottom.bxda",
			"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_AndyMarkWheelCenterRight.bxda",
			"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_AndyMarkWheelEdgeLeftTop.bxda",
			"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_AndyMarkWheelEdgeLeftBottom.bxda",
			"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_AndyMarkWheelCenterLeft.bxda",
			"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_frameouterplateleft_x81.bxda"};
		generateCubes (filepaths.Length);

		for (int i = 0; i < filepaths.Length; i++) 
		{
			loadBXDA (filepaths[i],transform.GetChild(i));
		}

		//transform.GetChild (6).gameObject.AddComponent<MeshCollider> ();
		attachMeshColliders (this.transform);
		Debug.Log (transform.GetChild (1).GetComponent<MeshCollider>().bounds.center);
		for (int i = 0; i < 6; i++) 
		{
			GameObject collider = new GameObject();
			collider.transform.parent = this.transform;
			collider.transform.position = transform.GetChild(i).GetComponent<MeshCollider>().bounds.center;
			collider.AddComponent<WheelCollider>();
			collider.GetComponent<WheelCollider>().radius = 5.2f;
			collider.GetComponent<WheelCollider>().transform.Rotate(90,0,0);
		}
		//attachRigidBodies (this.transform);
		//betaHinge (transform.GetChild (1), transform.GetChild (0), new Vector3 (0, 0, 0));
		transform.Rotate (new Vector3(-90,0,0));

//		Physics.IgnoreLayerCollision (10, 10, true);
//		foreach (Transform child in this.transform) 
//		{
//			child.gameObject.layer = 10;
//		}
		Debug.Log (transform.GetChild (0).gameObject.layer);


		transform.gameObject.AddComponent<Rigidbody> ();
		transform.rigidbody.mass = 120;
	}
	
	void FixedUpdate () 
	{
		for (int i = 7; i < 10; i++) 
		{
			WheelCollider tmp = transform.GetChild(i).GetComponent<WheelCollider>();
			tmp.motorTorque = Input.GetAxis("Vertical")*7-Input.GetAxis("Horizontal")*7;
		}
		for (int i = 10; i < 13; i++) 
		{
			WheelCollider tmp = transform.GetChild(i).GetComponent<WheelCollider>();
			tmp.motorTorque = Input.GetAxis("Vertical")*7+Input.GetAxis("Horizontal")*7;
		}
	}


}
