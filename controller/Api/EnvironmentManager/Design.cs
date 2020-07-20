using SharpGLTF.IO;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthesisAPI.EnvironmentManager
{
    public class Design
    {
        public DesignMeta MetaData { get; set; }

        public static IDictionary<int, Component> Components { get; set; }
        public Occurrence RootOccurence { get; set; }
        public static IList<Joint> Joints { get; set; }
        public IList<Material> Materials { get; set; }
        public IList<Appearance> Appearances { get; set; }

        public Design()
        {
            DesignMeta metaData = new DesignMeta();

            MetaData = metaData;
            Components = new Dictionary<int, Component>();
            RootOccurence = new Occurrence();
            Joints = new List<Joint>();
            Materials = new List<Material>();
            Appearances = new List<Appearance>();
        }

        #region Construction Data

        public class Occurrence
        {
            public Header OccurenceHeader { get; set; }
            public bool IsGrounded { get; set; }
            public Matrix3D Transform { get; set; }
            public string ComponentUuid { get; set; }
            public Component AComponent { get; set; }
            public Joint AJoint { get; set; }
            public IList<Occurrence> ChildOccurences { get; set; }

            public IDictionary<string, object> Attributes { get; set; }

            public Occurrence()
            {
                OccurenceHeader = new Header();
                Transform = new Matrix3D();
                ChildOccurences = new List<Occurrence>();
                Attributes = new Dictionary<string, object>();
            }
        }

        public class Joint
        {
            public Header JointHeader { get; set; }
            public Vector3 Direction { get; set; }
            public Vector3 Origin { get; set; }
            public JointType Type { get; set; }
            public bool IsLocked { get; set; }
            public bool IsFlipped { get; set; }

            public string OccurenceOneUuid { get; set; }
            public string OccurenceTwoUuid { get; set; }

            public IDictionary<string, object> Attributes { get; set; }

            /// <summary>
            /// Default constructor to auto assign non-nullable types
            /// </summary>
            public Joint()
            {
                JointHeader = new Header();
                Direction = new Vector3();
                Origin = new Vector3();
                Type = JointType.RigidJointType;
                Attributes = new Dictionary<string, object>();
            }

            public enum JointType
            {
                RigidJointType = 0,
                RevoluteJointType = 1,
                SliderJointType = 2,
                CylindricalJointType = 3,
                PinSlotJointType = 4,
                PlanarJointType = 5,
                BallJointType = 6
            }
        }

        #endregion

        #region Core Data

        public class Component
        {
            public Header ComponentHeader { get; set; }
            public string PartNumber { get; set; }
            public BoundingBox3D BoundingBox { get; set; }
            public string MaterialId { get; set; }
            public PhysicalProperties ComponentPhysicalProperties { get; set; }
            public List<MeshBody> MeshBodies { get; set; }

            public Dictionary<string, object> Attributes { get; set; }

            /// <summary>
            /// Default constructor to auto assign non-nullable types
            /// </summary>
            public Component()
            {
                ComponentHeader = new Header();
                BoundingBox = new BoundingBox3D();
                ComponentPhysicalProperties = new PhysicalProperties();
                MeshBodies = new List<MeshBody>();
                Attributes = new Dictionary<string, object>();
            }
        }

        public class MeshBody
        {
            public Header MeshHeader { get; set; }
            public string AppearanceId { get; set; }
            public string MaterialId { get; set; }
            public PhysicalProperties MeshPhysicalProperties { get; set; }
            public BoundingBox3D BoundingBox { get; set; }
            public TriangleMesh TriangleMesh { get; set; }

            public Dictionary<string, object> Attributes { get; set; }

            /// <summary>
            /// Default constructor to auto assign non-nullable types
            /// </summary>
            public MeshBody()
            {
                MeshHeader = new Header();
                MeshPhysicalProperties = new PhysicalProperties();
                BoundingBox = new BoundingBox3D();
                TriangleMesh = new TriangleMesh();
                Attributes = new Dictionary<string, object>();
            }
        }

        public class TriangleMesh
        {
            public Vector3Array Vertices { get; set; }
            public Vector3Array Normals { get; set; }
            public IList<double> Uvs { get; set; }
            public IList<int> Indices { get; set; }

            /// <summary>
            /// Default constructor to auto assign non-nullable types
            /// </summary>
            public TriangleMesh()
            {
                Vertices = new Vector3Array();
                Normals = new Vector3Array();
                Uvs = new List<double>();
                Indices = new List<int>();
            }

            public UnityEngine.Vector3[] GetVertices() => ToVector3Array(Vertices);
            public UnityEngine.Vector3[] GetNormals() => ToVector3Array(Normals);
            public UnityEngine.Vector2[] GetUvs() => ToVector2Array(Uvs);

            /// <summary>
            /// Converts this triangle mesh to a Unity mesh
            /// </summary>
            /// <returns>Unity mesh for unity things</returns>
            public UnityEngine.Mesh ToUnityMesh()
            {
                UnityEngine.Mesh mesh = new UnityEngine.Mesh
                {
                    vertices = GetVertices(),
                    uv = GetUvs().ToArray(),
                    triangles = Indices.ToArray(), // Why this doesn't throw an Exception confuses me
                    normals = GetNormals().ToArray()
                };
                return mesh;
            }

            public static implicit operator UnityEngine.Mesh(TriangleMesh mesh) => mesh.ToUnityMesh();

            private static UnityEngine.Vector3[] ToVector3Array(Vector3Array list)
            {
                if (list.Count % 3 != 0)
                    throw new Exception("Incomplete vector3 detected");
                if (list.Count < 1)
                    throw new Exception("List is empty");

                UnityEngine.Vector3[] vectors = new UnityEngine.Vector3[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    vectors[i] = new UnityEngine.Vector3(list[i].X, list[i].Y, list[i].Z);
                }

                return vectors;
            }
            private static UnityEngine.Vector2[] ToVector2Array(IList<double> list)
            {
                if (list.Count % 2 != 0)
                    throw new Exception("Incomplete vector2 detected");
                if (list.Count < 1)
                    throw new Exception("List is empty");

                UnityEngine.Vector2[] vectors = new UnityEngine.Vector2[list.Count / 2];

                for (int i = 0; i < list.Count - 1; i += 2)
                    vectors[i / 2] = (new UnityEngine.Vector2((float)list[i], (float)list[i + 1]));

                return vectors;
            }
        }

        #endregion

        #region Additional Data

        public class Material
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string AppearanceId { get; set; }
            public MaterialProperties Properties { get; set; }

            public Material()
            {
                Properties = new MaterialProperties();
            }
        }

        public class Appearance
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool HasTexture { get; set; }
            public AppearanceProperties Properties { get; set; }

            /// <summary>
            /// Default constructor to auto assign non-nullable types
            /// </summary>
            public Appearance()
            {
                Properties = new AppearanceProperties();
            }
        }

        public class PhysicalProperties
        {
            public double Density { get; set; }
            public double Mass { get; set; }
            public double Volume { get; set; }
            public double Area { get; set; }
            public Vector3 CenterOfMass { get; set; }

            /// <summary>
            /// Default constructor to auto assign non-nullable types
            /// </summary>
            public PhysicalProperties()
            {
                CenterOfMass = new Vector3();
            }
        }

        public class AppearanceProperties
        {
            public Color Albedo { get; set; }
            public int Glossiness { get; set; }
            public HighlightsMode Highlights { get; set; }
            public int ReflectivityDirect { get; set; }
            public int ReflectivityOblique { get; set; }
            public int Transparency { get; set; }
            public int Translucency { get; set; }
            public int RefractiveIndex { get; set; }
            public Color SelfIlluminationColor { get; set; }
            public int SelfIlluminationLuminance { get; set; }
            public int SelfIlluminationColorTemp { get; set; }
            public IDictionary<string, object> Attributes;

            /// <summary>
            /// Default constructor to auto assign non-nullable types
            /// </summary>
            public AppearanceProperties()
            {
                Albedo = new Color();
                Highlights = HighlightsMode.NonMetallic;
                SelfIlluminationColor = new Color();
                Attributes = new Dictionary<string, object>();
            }

            public enum HighlightsMode
            {
                Metallic = 0, NonMetallic = 1
            }
        }

        public class MaterialProperties
        {
            public int Density { get; set; }
            public int YieldStrength { get; set; }
            public int TensileStrength { get; set; }

            public IDictionary<string, object> Attributes { get; set; }

            public MaterialProperties()
            {
                Attributes = new Dictionary<string, object>();
            }
        }

        public class BoundingBox3D
        {
            public Vector3 MaxPoint { get; set; }
            public Vector3 MinPoint { get; set; }

            /// <summary>
            /// Default constructor to auto assign non-nullable types
            /// </summary>
            public BoundingBox3D()
            {
                MaxPoint = new Vector3();
                MinPoint = new Vector3();
            }
        }

        #endregion

        #region Utility

        public struct Header
        {
            public string Uuid { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string RevisionId { get; set; }
        }

        public struct Color
        {
            public int Red { get; set; }
            public int Green { get; set; }
            public int Blue { get; set; }
            public int Alpha { get; set; }

            public Color(int red, int green, int blue, int alpha = 255)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alpha;
            }

            public static implicit operator UnityEngine.Color(Color color) =>
                new UnityEngine.Color((float)color.Red / 255.0f, (float)color.Green / 255.0f,
                    (float)color.Blue / 255.0f, (float)color.Alpha / 255.0f);
        }

        public class Matrix3D
        {
            //private double[] _cells = new double[16];
            //public double[] Cells
            //{
            //    get => _cells;
            //}

            private float M11;
            private float M12;
            private float M13;
            private float M14;
            private float M21;
            private float M22;
            private float M23;
            private float M24;
            private float M31;
            private float M32;
            private float M33;
            private float M34;
            private float M41;
            private float M42;
            private float M43;
            private float M44;

            public IList<float> Cells { get; set; }
            public Matrix3D()
            {
                Cells = new List<float>();
            }

            public Matrix3D(float M11, float M12, float M13, float M14, float M21, float M22, float M23, float M24, float M31, float M32, float M33, float M34, float M41, float M42, float M43, float M44)
            {
                this.M11 = M11;
                this.M12 = M12;
                this.M13 = M13;
                this.M14 = M14;
                this.M21 = M21;
                this.M22 = M22;
                this.M23 = M23;
                this.M24 = M24;
                this.M31 = M31;
                this.M32 = M32;
                this.M33 = M33;
                this.M34 = M34;
                this.M41 = M41;
                this.M42 = M42;
                this.M43 = M43;
                this.M44 = M44;
            }
        }

        //public float[] Cells { get; set; }

        public struct Vector3
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public Vector3(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public static implicit operator UnityEngine.Vector3(Vector3 vec) =>
                new UnityEngine.Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
            public static implicit operator Vector3(UnityEngine.Vector3 vec) =>
                new Vector3(vec.x, vec.y, vec.z);
        }
        public struct Vector2
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Vector2(double x, double y)
            {
                X = x;
                Y = y;
            }

            public static implicit operator UnityEngine.Vector2(Vector2 vec) =>
                new UnityEngine.Vector2((float)vec.X, (float)vec.Y);
            public static implicit operator Vector2(UnityEngine.Vector2 vec) =>
                new Vector2(vec.x, vec.y);
        }

        #endregion
    }
}