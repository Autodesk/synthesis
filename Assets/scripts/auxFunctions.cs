using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class auxFunctions
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
                int[] cpy = new int[sub.surfaces[i].indicies.Length];
                Array.Copy(sub.surfaces[i].indicies, cpy, cpy.Length);
                unityMesh.SetTriangles(cpy, i);
            }
            if (normals != null)
            {
                unityMesh.RecalculateNormals();
            }
            handleMesh(j, sub, unityMesh);
        }
    }
	
    public static void OrientRobot(List<GameObject> wheelcolliders, Transform parent)
	{
		Quaternion q = new Quaternion();
		List<Vector3> wheels = new List<Vector3>();

        Vector3 center = new Vector3(0, 0, 0);
        foreach (GameObject collider in wheelcolliders)
        {
            wheels.Add(collider.transform.position);
            center += collider.transform.position;
        }
        center /= wheels.Count;

        for (int i = 0; i < wheels.Count; i++)
        {
            int min = i;
            for (int j = i + 1; j < wheels.Count; j++)
                if ((wheelcolliders[min].transform.position - center).magnitude > (wheelcolliders[j].transform.position - center).magnitude)
                    min = j;
            GameObject tmp = wheelcolliders[i];
            wheelcolliders[i] = wheelcolliders[min];
            wheelcolliders[min] = tmp;
        }

        Vector3 com = auxFunctions.TotalCenterOfMass(parent.gameObject);
		Vector3 a = wheels [0] - wheels [1];
		Vector3 b = a;

		for(int i = 2; Mathf.Abs(Vector3.Dot(a,b)/(a.magnitude*b.magnitude)) > .9f && i < wheels.Count; i++) 
			b = wheels[0] - wheels[i];
		Vector3 norm = Vector3.Cross(a,b).normalized;


        norm.y *= Mathf.Sign(norm.y * com.y);
		q.SetFromToRotation (norm, new Vector3(0,Math.Sign ((norm - wheels[0]).y),0));

        parent.position = new Vector3(parent.position.x, parent.position.y + .1f, parent.position.z);
	}
    public static void IgnoreCollisionDetection(List<Collider> meshColliders)
    {
        for(int i = 0; i < meshColliders.Count; i++)
        {
            for (int j = i + 1; j < meshColliders.Count; j++)
            {
                try
                {
                    Physics.IgnoreCollision(meshColliders[i], meshColliders[j], true);
                }
                catch
                {
                }
            }     
         }
    }

    /// <summary>
    /// Computes the total center of mass for all children of this game object.
    /// </summary>
    /// <param name="gameObj">The game object</param>
    /// <returns>The worldwide center of mass</returns>
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
}


