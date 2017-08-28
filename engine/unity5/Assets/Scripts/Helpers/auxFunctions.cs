using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using BulletUnity;
using Assets.Scripts.Utils;

public class AuxFunctions
{
    public delegate void HandleMesh(int id, BXDAMesh.BXDASubMesh subMesh, Mesh mesh);

    public static void ReadMeshSet(List<BXDAMesh.BXDASubMesh> meshes, HandleMesh handleMesh, bool mirror = false)
    {
        for (int j = 0; j < meshes.Count; j++)
        {
            BXDAMesh.BXDASubMesh sub = meshes[j];
            //takes all of the required information from the API (the API information is within "sub" above)
            Vector3[] vertices = sub.verts == null ? null : ArrayUtilities.WrapArray<Vector3>(
                delegate (double x, double y, double z)
                {
                    return new Vector3((float)x * (mirror ? -0.01f : 0.01f), (float)y * 0.01f, (float)z * 0.01f);
                }, sub.verts);
            Vector3[] normals = sub.norms == null ? null : ArrayUtilities.WrapArray<Vector3>(
                delegate (double x, double y, double z)
                {
                    return new Vector3((float)x, (float)y, (float)z);
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
                if (mirror)
                    Array.Reverse(cpy);
                unityMesh.SetTriangles(cpy, i);
            }
            if (normals != null)
            {
                unityMesh.RecalculateNormals();
            }

            handleMesh(j, sub, unityMesh);
        }
    }

    public static void GetCombinedMesh(List<BXDAMesh.BXDASubMesh> meshes, HandleMesh handleMesh)
    {
        BXDAMesh.BXDASubMesh combinedMesh = new BXDAMesh.BXDASubMesh();
        combinedMesh.verts = new double[0];
        combinedMesh.norms = new double[0];
        combinedMesh.surfaces = new List<BXDAMesh.BXDASurface>();

        foreach (BXDAMesh.BXDASubMesh mesh in meshes)
        {
            double[] oldVertices = combinedMesh.verts;
            double[] newVertices = new double[oldVertices.Length + mesh.verts.Length];
            oldVertices.CopyTo(newVertices, 0);
            mesh.verts.CopyTo(newVertices, oldVertices.Length);

            combinedMesh.verts = newVertices;

            double[] oldNorms = combinedMesh.verts;
            double[] newNorms = new double[oldNorms.Length + mesh.norms.Length];
            oldNorms.CopyTo(newNorms, 0);
            mesh.norms.CopyTo(newNorms, oldNorms.Length);

            combinedMesh.norms = newNorms;

            combinedMesh.surfaces.AddRange(mesh.surfaces);
        }

        List<BXDAMesh.BXDASubMesh> combinedMeshes = new List<BXDAMesh.BXDASubMesh>();
        combinedMeshes.Add(combinedMesh);

        ReadMeshSet(combinedMeshes, delegate (int id, BXDAMesh.BXDASubMesh subMesh, Mesh mesh)
        {
            handleMesh(id, subMesh, mesh);
        });
    }

    /// <summary>
    /// Generates a convex hull collision mesh from the given original mesh.
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    public static Mesh GenerateCollisionMesh(Mesh original, Vector3 offset = default(Vector3), float margin = 0f)
    {
        if (margin > 0f)
        {
            ConvexHullShape tempShape = new ConvexHullShape(Array.ConvertAll(original.vertices, x => x.ToBullet()), original.vertices.Length);
            tempShape.Margin = 0f;

            ShapeHull shapeHull = new ShapeHull(tempShape);
            bool b = shapeHull.BuildHull(0f);

            AlignedVector3Array initialVertices = new AlignedVector3Array();
            for (int i = 0; i < shapeHull.NumVertices; i++)
                initialVertices.Add(shapeHull.Vertices[i]);

            List<BulletSharp.Math.Vector4> planeEquations = GeometryUtilEx.GetPlaneEquationsFromVertices(initialVertices);

            List<BulletSharp.Math.Vector4> shiftedPlaneEquations = new List<BulletSharp.Math.Vector4>();
            
            for (int i = 0; i < planeEquations.Count; i++)
            {
                BulletSharp.Math.Vector4 plane = planeEquations[i];
                plane.W += margin;
                shiftedPlaneEquations.Add(plane);
            }

            List<BulletSharp.Math.Vector3> shiftedVertices = GeometryUtilEx.GetVerticesFromPlaneEquations(shiftedPlaneEquations);

            Vector3[] finalVertices = new Vector3[shiftedVertices.Count];

            Mesh collisionMesh = new Mesh();

            for (int i = 0; i < finalVertices.Length; i++)
                finalVertices[i] = shiftedVertices[i].ToUnity() - offset;

            collisionMesh.vertices = finalVertices;
            collisionMesh.RecalculateBounds();

            return collisionMesh;
        }
        else
        {
            ConvexHullShape tempShape = new ConvexHullShape(Array.ConvertAll(original.vertices, x => x.ToBullet()), original.vertices.Length);
            tempShape.Margin = 0f;

            ShapeHull shapeHull = new ShapeHull(tempShape);
            bool b = shapeHull.BuildHull(0f);

            Mesh collisionMesh = new Mesh();

            Vector3[] vertices = new Vector3[shapeHull.NumVertices];
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = shapeHull.Vertices[i].ToUnity() - offset;

            int[] triangles = new int[shapeHull.NumIndices];
            for (int i = 0; i < triangles.Length; i++)
                triangles[i] = (int)shapeHull.Indices[i];

            collisionMesh.vertices = vertices;
            collisionMesh.triangles = triangles;
            collisionMesh.RecalculateNormals();
            collisionMesh.RecalculateBounds();

            return collisionMesh;
        }
    }

    public static void OrientRobot(List<GameObject> wheelcolliders, Transform parent)
    {
        Quaternion q = new Quaternion();
        List<Vector3> wheels = new List<Vector3>();

        foreach (GameObject collider in wheelcolliders)
            wheels.Add(collider.transform.position);

        if (wheels.Count > 2)
        {
            Vector3 a = wheels[0] - wheels[1];
            Vector3 b = a;

            for (int i = 2; Mathf.Abs(Vector3.Dot(a, b) / (a.magnitude * b.magnitude)) > .9f && i < wheels.Count; i++)
                b = wheels[0] - wheels[i];

            Vector3 norm = Vector3.Cross(a, b).normalized;
            Debug.DrawRay(wheels[0], norm);

            q.SetFromToRotation(norm, Vector3.up);
            parent.localRotation *= q;

            parent.position = new Vector3(parent.position.x, parent.position.y + .1f, parent.position.z);
        }
        //TODO THROW WHEEL EXCEPTION

    }
    public static Boolean rightRobot(List<GameObject> wheelcolliders, Transform parent)
    {
        Quaternion q = new Quaternion();
        List<Vector3> wheels = new List<Vector3>();

        foreach (GameObject collider in wheelcolliders)
            wheels.Add(collider.transform.position);

        Vector3 com = AuxFunctions.TotalCenterOfMass(parent.gameObject);
        Debug.Log(com.y < wheels[0].y);
        q.SetFromToRotation(parent.localToWorldMatrix * Vector3.up, parent.localToWorldMatrix * Vector3.down);
        if (com.y > wheels[0].y)
        {
            return false;
        }
        else
        {
            parent.localRotation *= q;
            return true;
        }
    }

    public static void IgnoreCollisionDetection(List<Collider> meshColliders)
    {
        for (int i = 0; i < meshColliders.Count; i++)
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

    /// <summary>
    /// Mouses the in window.
    /// </summary>
    /// <returns><c>true</c>, if in window was moused, <c>false</c> otherwise.</returns>
    /// <param name="window">Window.</param>
    public static bool MouseInWindow(Rect window)
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Screen.height - Input.mousePosition.y; // Convert mouse coordinates to unity window positions coordinates
        return mouseX > window.x && mouseX < window.x + window.width && mouseY > window.y && mouseY < window.y + window.height;
    }

    public static GameObject FindObject(GameObject parent, string name)
    {
        Component[] trs = parent.GetComponentsInChildren(typeof(Transform), true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return new GameObject("COULDNOTFIND" + name);
    }

    public static GameObject FindObject(string name)
    {
        GameObject[] trs = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject t in trs)
        {
            if (t.name.Contains(name))
            {
                return t.gameObject;
            }
        }
        return new GameObject("COULDNOTFIND" + name);
    }

    public static float ToFeet(float meter)
    {
        return meter * (328.084f) / 100;
    }

    public static float ToMeter(float feet)
    {
        return feet * 30.48f / 100;
    }
}