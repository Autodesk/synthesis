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
                        return new Vector3((float)x * (mirror ? -1: 1), (float)y, (float)z);
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
            throw new Exception("Game object not found. Parent: " + parent.name.ToString() + " Name: " + name);
        }

        public static GameObject FindObject(string name)
        {
            GameObject[] trs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject t in trs)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            throw new Exception("Game object not found (Is it disabled?). Name: " + name);
        }

        /// <summary>
        /// Finds the first <see cref="GameObject"/> with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject FindGameObject(string name)
        {
            IEnumerable<GameObject> gameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.name.Equals(name));
            return gameObjects.Any() ? gameObjects.First() : null; ;
        }

        /// <summary>
        /// Based on a solution provided by the Unity Wiki (http://wiki.unity3d.com/index.php/3d_Math_functions).
        /// Finds the closest points on two lines.
        /// </summary>
        /// <param name="closestPointLine1"></param>
        /// <param name="closestPointLine2"></param>
        /// <param name="linePoint1"></param>
        /// <param name="lineVec1"></param>
        /// <param name="linePoint2"></param>
        /// <param name="lineVec2"></param>
        /// <returns></returns>
        public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);

            float d = a * e - b * b;

            // Check if lines are parallel
            if (d == 0.0f)
                return false;

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        /// <summary>
        /// Finds the best unit normal that fits the set of points provided.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Vector3 BestFitUnitNormal(Vector3[] points, out Vector3 centroid)
        {
            Debug.Assert(points.Length >= 2);

            Vector3 displacement, result = centroid = displacement = Vector3.zero;
            
            foreach (Vector3 p in points)
                centroid += p;

            centroid /= points.Length;

            foreach (Vector3 p in points)
                displacement += Abs(p - centroid);

            int min = 0;
            for (int i = 1; i < 3; i++)
                if (displacement[i] < displacement[min])
                    min = i;

            result[min] = 1f;
            return result;
        }

        /// <summary>
        /// Returns a new <see cref="Vector3"/> with each component set to the absolute value
        /// of its corresponding component in the <see cref="Vector3"/> provided.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z));
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