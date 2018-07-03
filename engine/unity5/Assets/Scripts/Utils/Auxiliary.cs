using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using BulletUnity;
using System.Linq;

namespace Synthesis.Utils
{
    public static class Auxiliary
    {
        public const float DegToRad = Mathf.PI / 180f;
        public const float RadToDeg = 1f / DegToRad;

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

        public static Mesh GetScaledCopy(this Mesh mesh, float xScale, float yScale, float zScale)
        {
            Mesh copy = UnityEngine.Object.Instantiate(mesh);

            Vector3[] vertices = new Vector3[copy.vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = copy.vertices[i];
                vertex.x = vertex.x * xScale;
                vertex.y = vertex.y * yScale;
                vertex.z = vertex.z * zScale;
                vertices[i] = vertex;
            }

            copy.vertices = vertices;

            copy.RecalculateNormals();
            copy.RecalculateBounds();

            return copy;
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

        public static GameObject FindGameObject(string name)
        {
            return Resources.FindObjectsOfTypeAll<GameObject>().First(o => o.name.Equals(name));
        }

        public static float ToFeet(float meter)
        {
            return meter * (328.084f) / 100;
        }

        public static float ToMeter(float feet)
        {
            return feet * 30.48f / 100;
        }

        public static float ToRadians(float deg)
        {
            return deg * DegToRad;
        }

        public static float ToDegrees(float rad)
        {
            return rad * RadToDeg;
        }
    }
}