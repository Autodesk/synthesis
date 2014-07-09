using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//IMPORTANT: Will only move sidways by pushing the left and right arrow key
//wheel colliders are positioned so procedurally :)
//But... it work like omniwheels for the left arrow key!!

public class OmniWheelsMovingSideways : MonoBehaviour {

	public static bool isWheel = true;
	public int Length;
	
	//	private ConfigurableJoint hTopLeft;
	//	private ConfigurableJoint hTopRight;
	//	private ConfigurableJoint hBottomLeft;
	//	private ConfigurableJoint hBottomRight;
	//	
	//	// These will hold the names of keyboard keys. Each string will correspond to a function.
	//	private string forward;
	//	private string backward;
	//	private string left;
	//	private string right;
	
	
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
			tmp.transform.position = new Vector3(70,-70,30);
			tmp.transform.Rotate (new Vector3 (270, 180, 180));
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
			//	child.gameObject.AddComponent<Rigidbody>();
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
		//	OwnerHinge.connectedBody = connected.gameObject.rigidbody;
		OwnerHinge.anchor = anchorCoor;
	}
	
	void Start () 
	{
		
		//string[] filepaths = {"C:\\Users\\t_defap\\Documents\\skele_andy_mark_chassis\\skele_andy_mark_chassis\\node_AndyMarkWheelEdgeRightTop.bxda"};
		string[] filepaths = {
			"C:\\Users\\t_grahj\\My Documents\\4WheelRobot\\node_0.bxda",
			"C:\\Users\\t_grahj\\My Documents\\4WheelRobot\\node_1.bxda",
			"C:\\Users\\t_grahj\\My Documents\\4WheelRobot\\node_2.bxda",
			"C:\\Users\\t_grahj\\My Documents\\4WheelRobot\\node_3.bxda",
			"C:\\Users\\t_grahj\\My Documents\\4WheelRobot\\node_4.bxda",
			"C:\\Users\\t_grahj\\My Documents\\4WheelRobot\\node_5.bxda"};
		//"C:\Users\t_grahj\Documents\4WheelRobot\node_0.bxda"};
		generateCubes (filepaths.Length);
		
		Length = filepaths.Length / 2;
		
		for (int i = 0; i < filepaths.Length; i++) {
			loadBXDA (filepaths [i], transform.GetChild (i));
		}
		
		//transform.GetChild (6).gameObject.AddComponent<MeshCollider> ();
		
		
		attachMeshColliders (this.transform);
		Debug.Log (transform.GetChild (1).GetComponent<MeshCollider> ().bounds.center);
		for (int i = 0; i < 5; i++) {
			//			if (i == 5){
			//				ConfigurableJoint arm = tmp.AddComponent<ConfigurableJoint> ();
			//				arm.targetPosition = transform.GetChild(i).GetComponent<MeshCollider> ().bounds.center;
			//				JointDrive drive = new JointDrive();
			//				drive.mode = JointDriveMode.Position;
			//			 
			GameObject collider = new GameObject ();
			collider.transform.parent = this.transform;
			collider.transform.position = transform.GetChild (i).GetComponent<MeshCollider> ().bounds.center;
			collider.AddComponent<WheelCollider> ();
			collider.GetComponent<WheelCollider> ().radius = 1;
			collider.GetComponent<WheelCollider> ().transform.Rotate (45, 0, 45);

			switch(i)
			{
			case 1:
				collider.GetComponent<WheelCollider> ().transform.Rotate (45, 0, 45);
				break;
			case 4:
				collider.GetComponent<WheelCollider> ().transform.Rotate (45, 0, 45);
				break; 
			case 2:
				collider.GetComponent<WheelCollider> ().transform.Rotate (315, 0, 315);
				break;
			case 3:
				collider.GetComponent<WheelCollider> ().transform.Rotate (315, 0, 315);
				break;
			}
		
			collider.name = "wheelCollider" + i;
			
		}
		attachRigidBodies (this.transform);
		//betaHinge (transform.GetChild (1), transform.GetChild (0), new Vector3 (0, 0, 0));
	
		//		Physics.IgnoreLayerCollision (10, 10, true);
		//		foreach (Transform child in this.transform) 
		//		{
		//			child.gameObject.layer = 10;
		//		}
		//	Debug.Log (transform.GetChild (0).gameObject.layer);
		
		
		transform.gameObject.AddComponent<Rigidbody> ();
		transform.rigidbody.mass = 120;
		
	}
	
	void FixedUpdate () 
	{
		//		for (int i = 7; i < Length+7 ; i++) 
		//		{
		//			WheelCollider tmp = transform.GetChild(i).GetComponent<WheelCollider>();
		//			tmp.motorTorque = Input.GetAxis("Vertical")*7-Input.GetAxis("Horizontal")*7;
		//			Debug.Log(tmp.motorTorque);
		//		}
		//		for (int i = Length +8; i < ((Length*2)+7); i++) 
		//		{
		//			WheelCollider tmp = transform.GetChild(i).GetComponent<WheelCollider>();
		//			tmp.motorTorque = Input.GetAxis("Vertical")*7+Input.GetAxis("Horizontal")*7;
		//		}
		for (int i = 1; i < 4; i++)
		{
			//personalized for 4 wheel drive (if anyone wants the original code for moving directly forward and turning (etc) more smoothly than 
			//the code above which is commented out - I have that too - it works really well)

			float speed = 10;
			float rotationalSpeed = 10;
			WheelCollider tmp = transform.GetChild(i).GetComponent<WheelCollider>();
			//			float translation = Input.GetAxis ("Vertical")*speed;
			//			float rotation = Input.GetAxis ("Horizontal") *rotationalSpeed;
			float translation = -(Input.GetAxis ("Vertical")) * speed * Time.deltaTime;
			float rotation = -(Input.GetAxis ("Horizontal")) * rotationalSpeed * Time.deltaTime;
			transform.Translate (0,0, translation);
			transform.Rotate (0, rotation, 0);
			switch(i)
			{
			case 1:
				tmp.motorTorque = (translation*rotation);
				break;
			case 4:
				tmp.motorTorque = (translation*rotation);
				break; 
			case 2:
				tmp.motorTorque = translation;
				break;
			case 3:
				tmp.motorTorque = translation;
				break;
			}
		}
	}
}
