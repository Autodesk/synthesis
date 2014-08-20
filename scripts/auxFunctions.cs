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
                    return new Vector3((float) x * 0.01f, (float) y * 0.01f, (float) z * 0.01f);
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
        return new Vector3((float) vector.x * 0.01f, (float) vector.y * 0.01f, (float) vector.z * 0.01f);
	}
	
	public static void OrientRobot(List<WheelCollider> wheelcolliders, Transform parent)
	{

		Quaternion q = new Quaternion();
		List<Vector3> wheels = new List<Vector3>();

		foreach (WheelCollider collider in wheelcolliders) 
		{
			wheels.Add(collider.transform.position);

		}
		Vector3 com = UnityRigidNode.TotalCenterOfMass(parent.gameObject);
		Vector3 a = wheels [0] - wheels [1];
		Vector3 b = a;

		for(int i = 2; Mathf.Abs(Vector3.Dot(a,b)-(a.magnitude*b.magnitude)) < .1f; i++) 
			b = wheels[0] - wheels[i];

		Vector3 norm = Vector3.Cross(a,b).normalized;
		//norm.y *= Mathf.Sign (norm.y * com.y);
		//TODO make sure robot never spawns upside down


		q.SetFromToRotation (norm, Vector3.up);
		parent.localRotation *= q;

      

	
	
	
//		Vector3 norm = Vector3.Cross((wheels[0] - wheels[1]),(wheels[0] - wheels[2]));
//		Debug.Log(norm);
//		
//		Vector3 com = UnityRigidNode.TotalCenterOfMass(parent.gameObject);
//		Vector3 above = Vector3.Cross((wheels[0] - com),norm);
//		//norm = norm * ((above.y < 0) ? 1 : -1);
//	
//		norm = norm * (Mathf.Sign(norm.y) != Mathf.Sign(above.y) ? -1:1);
//
//		Quaternion q = new Quaternion();
//		q.SetFromToRotation(norm, Vector3.up);
//		parent.rotation *= q;	
//		Debug.Log("Orientation Complete: " + parent.name);	

	}
	
	
	// Creates a bounding box for the entire gameobject which is then used to position the robot with a raycast
	public static void placeRobotJustAboveGround (Transform parent) 
	{
		Vector3 center = Vector3.zero;
		for(int i = 0; i < parent.childCount; i++)
		{
			Vector3 subCenter = Vector3.zero;
			foreach (Transform child in parent.GetChild(i))
			{
				if(child.renderer != null)
				{
					subCenter += child.gameObject.renderer.bounds.center;
				}
			}
			subCenter /= parent.GetChild(i).childCount;
			center += subCenter;	
		}
		center /= parent.childCount;
		Bounds parentBounds = new Bounds(center, Vector3.zero);
		for(int i = 0; i < parent.childCount; i++)	
		{
			foreach(Transform child in parent.GetChild(i))
			{
				if(child.renderer != null)
				{
					parentBounds.Encapsulate(child.renderer.bounds);
				}
			}
		}
		
		Vector3 above = parentBounds.min - parentBounds.center;
		float yValue =  above.y - parentBounds.center.y;
		parentBounds.center = new Vector3(parentBounds.center.x, parentBounds.center.y + yValue, parentBounds.center.z);
		// Uses a raycast to find the distance from a given point to the floor
		RaycastHit hit = new RaycastHit();
		Physics.Raycast(parentBounds.center, Vector3.down, out hit);
		float distanceToFloor = hit.distance;
		
		// It then translates the robot down
		parent.localPosition = new Vector3(parent.localPosition.x, -(distanceToFloor), parent.localPosition.z);
	}
}


