using System;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.VirtualFileSystem;
using SharpGLTF;
using SharpGLTF.Schema2;
using System.Threading;
using SynthesisAPI.Utilities;
using System.Xml.Serialization;
using SynthesisAPI.EnvironmentManager;
using static SynthesisAPI.EnvironmentManager.Design;
using System.Linq;
using SharpGLTF.Memory;
using UnityEngine.Assertions.Must;
using SharpGLTF.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using SynthesisAPI.EnvironmentManager.Bundles;
using MathNet.Spatial.Euclidean;

namespace SynthesisAPI.AssetManager
{
    public class GltfAsset : Asset
    {
        public Transform Transform { get; private set; }

        public GltfAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);
        }

        public override IEntry Load(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;

            ModelRoot model = null;
            bool tryFix = false;
            model = GetModelInfo(model, stream, tryFix);

            return this;
        }

        private ModelRoot GetModelInfo(ModelRoot model, MemoryStream stream, bool tryFix = false)
        {
            try
            {
                var settings = tryFix ? SharpGLTF.Validation.ValidationMode.TryFix : SharpGLTF.Validation.ValidationMode.Strict;

                model = ModelRoot.ReadGLB(stream, settings);
                ImportDesign(model);
                ImportObject(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed");
            }

            return model;
        }

        /// <summary>
        /// Parses the elements of a gltf model from a root node and traverses through the model tree graph.
        /// </summary>
        /// <param name="modelRoot"></param>
        /// <returns></returns>
        public Design ImportDesign(ModelRoot modelRoot)
        {
            Design design = new Design();
            JsonDictionary extras = (JsonDictionary)modelRoot.Extras;

            // this is the root ---> modelRoot.DefaultScence.VisualChildren
            foreach (Node child in modelRoot.DefaultScene.VisualChildren)
            {
                design.RootOccurence = ExportOccurrenceFromNode(child);
                //design.RootOccurence.Transform = ExportMatrix(child.LocalMatrix);
            }

            // joints are not attached to RootOccurrence directly but added to Joint dictionary
            foreach (JsonDictionary joint in (JsonList)extras["joints"])
            {
                Design.Joints.Add(ExportJointsFromExtras(joint));
                Console.WriteLine("Debug");
            }

            foreach (SharpGLTF.Schema2.Material material in modelRoot.LogicalMaterials)
            {
                Design.Materials.Add(ExportMaterial(material));
                Design.Appearances.Add(ExportAppearance(material));
            }

            return design;
        }

        /// <summary>
        /// Parses the individual elements of a gltf model.
        /// </summary>
        /// <param name="modelRoot"></param>
        /// <returns></returns>
        public ObjectBundle ImportObject(ModelRoot modelRoot)
        {
            ObjectBundle objectBundle = new ObjectBundle();

            foreach (SharpGLTF.Schema2.Mesh childMesh in modelRoot.LogicalMeshes)
            {
                objectBundle.Mesh = ExportMesh(childMesh);
            }

            // this is the root ---> modelRoot.DefaultScence.VisualChildren
            foreach (Node child in modelRoot.DefaultScene.VisualChildren)
            {
                ExportModelRoot(child);
            }

            return objectBundle;
        }

        public Occurrence ExportOccurrenceFromNode(Node node)
        {
            // if node is already in the Components maps, return it
            //if (Components[node.LogicalIndex].Equals(occurrence.AComponent))
            //{
            //    // return map component
            //}

            // todo:
            // OccurenceHeader
            // IsGrounded
            // Transform
            // ComponentUuid
            // Attributes

            Occurrence occurrence = new Occurrence();

            foreach (Node child in node.VisualChildren)
            {
                occurrence.AComponent = ExportComponentsFromMesh(child.Mesh);
                //occurrence.Transform = ExportMatrix(child.LocalMatrix);
                occurrence.ChildOccurences.Add(ExportOccurrenceFromNode(child));
            }

            return occurrence;
        }

        public EnvironmentManager.Components.Transform ExportModelRoot(Node node)
        {
            EnvironmentManager.Components.Transform synthesisTransform = new EnvironmentManager.Components.Transform();
            ObjectBundle objectBundle = new ObjectBundle();

            foreach (Node child in node.VisualChildren)
            {
                objectBundle.Transform = ExportTransform(child.LocalTransform);
            }

            return synthesisTransform;
        }

        private EnvironmentManager.Components.Transform ExportTransform(SharpGLTF.Transforms.AffineTransform transform)
        {
            EnvironmentManager.Components.Transform synthesisTransform = new EnvironmentManager.Components.Transform();

            // ROTATION
            MathNet.Spatial.Euclidean.Quaternion quaternion;
            quaternion = new MathNet.Spatial.Euclidean.Quaternion(transform.Rotation.W, transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z);

            // SCALE
            MathNet.Spatial.Euclidean.Vector3D vector3D;
            vector3D = new MathNet.Spatial.Euclidean.Vector3D(transform.Scale.X, transform.Scale.Y, transform.Scale.Z);
            synthesisTransform.Scale = vector3D;

            return synthesisTransform;
        }

        //private Design.Matrix3D ExportMatrix(System.Numerics.Matrix4x4 matrix4x4)
        //{
        //    Matrix3D matrix3D = new Matrix3D(matrix4x4.M11, matrix4x4.M12, matrix4x4.M13, matrix4x4.M14, matrix4x4.M21, matrix4x4.M22, matrix4x4.M23, matrix4x4.M24, matrix4x4.M31, matrix4x4.M32, matrix4x4.M33, matrix4x4.M34, matrix4x4.M41, matrix4x4.M42, matrix4x4.M43, matrix4x4.M44);

        //    return matrix3D;
        //}

        private EnvironmentManager.Components.Mesh ExportMesh(SharpGLTF.Schema2.Mesh mesh)
        {
            EnvironmentManager.Components.Mesh synthesisMesh = new EnvironmentManager.Components.Mesh();
            
            foreach (SharpGLTF.Schema2.MeshPrimitive primitive in mesh.Primitives)
            {
                // checks for POSITION or NORMAL vertex as not all designs have both
                if (primitive.VertexAccessors.ContainsKey("POSITION"))
                {
                    Vector3Array vertices;
                    vertices = primitive.GetVertices("POSITION").AsVector3Array();

                    foreach (System.Numerics.Vector3 vertex in vertices)
                    {
                        MathNet.Spatial.Euclidean.Vector3D vector3D;
                        vector3D = new MathNet.Spatial.Euclidean.Vector3D(vertex.X, vertex.Y, vertex.Z);
                        synthesisMesh.Vertices.Add(vector3D);
                    }
                }

                var indices = primitive.GetIndices();

                for (int i = 0; i < indices.Count; i++)
                {
                    synthesisMesh.Triangles.Add((int)indices[i]);
                }
            }

            return synthesisMesh;
        }

        private Design.Component ExportComponentsFromMesh(SharpGLTF.Schema2.Mesh mesh)
        {
            // todo: 
            // export mesh ONLY IF it hasn't been exported yet

            Design.Component component = new Design.Component();

            // todo:
            // ComponentHeader
            // PartNumber
            // BoundingBox
            // MaterialId
            // ComponentPhysicalProperties
            // Attributes

            // MeshBodies
            foreach (SharpGLTF.Schema2.MeshPrimitive meshBody in mesh.Primitives)
            {
                component.MeshBodies.Add(ExportMeshBodiesFromPrimitives(meshBody));
            }

            Components.Add(mesh.LogicalIndex, component);

            return component;
        }

        private MeshBody ExportMeshBodiesFromPrimitives(MeshPrimitive primitive)
        {
            MeshBody meshBody = new MeshBody();

            // checks for POSITION or NORMAL vertex as not all designs have both
            if (primitive.VertexAccessors.ContainsKey("POSITION"))
            {
                meshBody.TriangleMesh.Vertices = primitive.GetVertices("POSITION").AsVector3Array();
                meshBody.Attributes.Add("POSITION", meshBody.TriangleMesh.Vertices);
            }

            if (primitive.VertexAccessors.ContainsKey("NORMAL"))
            {
                meshBody.TriangleMesh.Normals = primitive.GetVertices("NORMAL").AsVector3Array();
            }

            var indices = primitive.GetIndices();

            for (int i = 0; i < indices.Count; i++)
            {
                meshBody.TriangleMesh.Indices.Add((int)indices[i]);
            }

            // todo:
            //Uvs = new List<double>();

            return meshBody;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jointDict">Type of <see cref="JsonDictionary"/></param>
        /// <returns></returns>
        private Design.Joint ExportJointsFromExtras(JsonDictionary jointDict)
        {
            Design.Joint joint = new Design.Joint();

            // HEADER
            var jointHeader = joint.JointHeader;
            jointHeader.Name = jointDict.Get<JsonDictionary>("header").Get<string>("name");
            joint.JointHeader = jointHeader;

            // ORIGIN
            var jointOrigin = joint.Origin;
            jointOrigin.X = (double)jointDict.Get<JsonDictionary>("origin").Get<decimal>("x");
            jointOrigin.Y = (double)jointDict.Get<JsonDictionary>("origin").Get<decimal>("y");
            jointOrigin.Z = (double)jointDict.Get<JsonDictionary>("origin").Get<decimal>("z");
            joint.Origin = jointOrigin;

            // IDs
            joint.OccurenceOneUuid = (string)jointDict["occurrenceOneUUID"];
            joint.OccurenceTwoUuid = (string)jointDict["occurrenceTwoUUID"];

            // JOINT MOTION
            //var jointType = joint.Type;
            var jointMotion = joint.JointMotion;

            jointMotion = GetJointMotion(jointDict, jointMotion);

            joint.JointMotion = jointMotion;
            //joint.Type = jointType;

            return joint;
        }

        private double CheckJointMotionValues(JsonDictionary dict, string key)
        {
            // if this is in the dictionary
            // return it's double values
            // else return default values of 0
            object value;

            if (dict.ContainsKey(key))
            {
                dict.TryGetValue(key, out value);
                return (double)(decimal)value;
            }
            else return 0;
        }

        private Design.JointLimits CheckJointLimits(JsonDictionary dict)
        {
            List<string> jointLimitTypes = new List<string>() { "rotationLimits", "slideLimits", "primarySlideLimits", "primarySlideLimits", "rollLimits", "pitchLimits", "yawLimits" };

            Design.JointLimits jointLimits = new Design.JointLimits();

            foreach (string s in jointLimitTypes)
            {
                if (dict.ContainsKey(s))
                {
                    jointLimits.IsMaximumValueEnabled = dict.Get<JsonDictionary>(s).Get<bool>("isMaximumValueEnabled");
                    jointLimits.IsMinimumValueEnabled = dict.Get<JsonDictionary>(s).Get<bool>("isMinimumValueEnabled");
                    jointLimits.MaximumValue = (double)dict.Get<JsonDictionary>(s).Get<decimal>("maximumValue");
                    jointLimits.MinimumValue = (double)dict.Get<JsonDictionary>(s).Get<decimal>("minimumValue");
                    jointLimits.RestValue = (double)dict.Get<JsonDictionary>(s).Get<decimal>("restValue");
                }
            }

            return jointLimits;
        }

        Design.JointMotion GetJointMotion(JsonDictionary jointDict, JointMotion jointMotion)
        {
            Design.JointLimits primaryJointLimit = new Design.JointLimits();
            Design.JointLimits secondaryJointLimit = new Design.JointLimits();
            Design.JointLimits tertiaryJointLimit = new Design.JointLimits();

            Design.Vector3 rotationVec = new Design.Vector3();
            Design.Vector3 slideVec = new Design.Vector3();
            Design.Vector3 secondaryVec = new Design.Vector3();

            double rotationValue;
            double slideValue;
            double secondarySlideValue;

            if (jointDict.ContainsKey("revoluteJointMotion"))
            {
                jointMotion.Type = (Design.JointMotion.JointType)Enum.Parse(typeof(Design.JointMotion.JointType), "revoluteJointMotion", true);
                rotationVec.Y = (double)jointDict.Get<JsonDictionary>("revoluteJointMotion").Get<JsonDictionary>("rotationAxisVector").Get<decimal>("y");
                rotationValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("revoluteJointMotion"), "rotationValue");

                primaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("revoluteJointMotion"));

                jointMotion = new RevoluteJointMotion(jointMotion.Type, rotationVec, rotationValue, primaryJointLimit);
            }

            if (jointDict.ContainsKey("sliderJointMotion"))
            {
                // CHECK vectors x, y, z if they exist; current test models don't have x, y, z
                jointMotion.Type = (Design.JointMotion.JointType)Enum.Parse(typeof(Design.JointMotion.JointType), "sliderJointMotion", true);
                rotationVec.Y = (double)jointDict.Get<JsonDictionary>("sliderJointMotion").Get<JsonDictionary>("slideDirectionVector").Get<decimal>("y");
                slideValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("sliderJointMotion"), "slideValue");
                primaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("sliderJointMotion"));

                jointMotion = new SliderJointMotion(jointMotion.Type, rotationVec, slideValue, primaryJointLimit);
            }

            if (jointDict.ContainsKey("cylindricalJointMotion"))
            {
                jointMotion.Type = (Design.JointMotion.JointType)Enum.Parse(typeof(Design.JointMotion.JointType), "cylindricalJointMotion", true);

                rotationVec.Y = (double)jointDict.Get<JsonDictionary>("cylindricalJointMotion").Get<JsonDictionary>("rotationAxisVector").Get<decimal>("y");
                rotationValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("cylindricalJointMotion"), "rotationValue");
                slideValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("cylindricalJointMotion"), "slideValue");

                primaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("cylindricalJointMotion"));
                secondaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("cylindricalJointMotion"));

                jointMotion = new CylindricalJointMotion(jointMotion.Type, rotationVec, rotationValue, slideValue, primaryJointLimit, secondaryJointLimit);
            }

            if (jointDict.ContainsKey("pinSlotJointMotion"))
            {
                jointMotion.Type = (Design.JointMotion.JointType)Enum.Parse(typeof(Design.JointMotion.JointType), "pinSlotJointMotion", true);

                rotationVec.Y = (double)jointDict.Get<JsonDictionary>("pinSlotJointMotion").Get<JsonDictionary>("rotationAxisVector").Get<decimal>("y");
                slideVec.Y = (double)jointDict.Get<JsonDictionary>("pinSlotJointMotion").Get<JsonDictionary>("slideDirectionVector").Get<decimal>("y");
                rotationValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("pinSlotJointMotion"), "rotationValue");
                slideValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("pinSlotJointMotion"), "slideValue");

                primaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("pinSlotJointMotion"));
                secondaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("pinSlotJointMotion"));

                jointMotion = new PinSlotJointMotion(jointMotion.Type, rotationVec, slideVec, rotationValue, slideValue, primaryJointLimit, secondaryJointLimit);
            }

            if (jointDict.ContainsKey("planarJointMotion"))
            {
                jointMotion.Type = (Design.JointMotion.JointType)Enum.Parse(typeof(Design.JointMotion.JointType), "planarJointMotion", true);

                rotationVec.Y = (double)jointDict.Get<JsonDictionary>("planarJointMotion").Get<JsonDictionary>("normalDirectionVector").Get<decimal>("y");
                slideVec.Y = (double)jointDict.Get<JsonDictionary>("planarJointMotion").Get<JsonDictionary>("primarySlideDirectionVector").Get<decimal>("y");
                secondaryVec.Y = (double)jointDict.Get<JsonDictionary>("planarJointMotion").Get<JsonDictionary>("secondarySlideDirectionVector").Get<decimal>("y");
                slideValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("planarJointMotion"), "primarySlideValue");
                secondarySlideValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("planarJointMotion"), "secondarySlideValue");
                rotationValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("planarJointMotion"), "rotationValue");

                primaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("planarJointMotion"));
                secondaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("planarJointMotion"));
                tertiaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("planarJointMotion"));

                jointMotion = new PlanarJointMotion(jointMotion.Type, rotationVec, slideVec, secondaryVec, slideValue, secondarySlideValue, rotationValue, primaryJointLimit, secondaryJointLimit, tertiaryJointLimit);
            }

            if (jointDict.ContainsKey("ballJointMotion"))
            {
                double rollValue;
                double pitchValue;
                double yawValue;

                jointMotion.Type = (Design.JointMotion.JointType)Enum.Parse(typeof(Design.JointMotion.JointType), "ballJointMotion", true);

                rotationVec.Y = (double)jointDict.Get<JsonDictionary>("ballJointMotion").Get<JsonDictionary>("rollDirectionVector").Get<decimal>("y");
                slideVec.Y = (double)jointDict.Get<JsonDictionary>("ballJointMotion").Get<JsonDictionary>("pitchDirectionVector").Get<decimal>("y");
                secondaryVec.Y = (double)jointDict.Get<JsonDictionary>("ballJointMotion").Get<JsonDictionary>("yawDirectionVector").Get<decimal>("y");
                rollValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("ballJointMotion"), "rollValue");
                pitchValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("ballJointMotion"), "pitchValue");
                yawValue = (double)CheckJointMotionValues(jointDict.Get<JsonDictionary>("ballJointMotion"), "yawValue");

                primaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("ballJointMotion"));
                secondaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("ballJointMotion"));
                tertiaryJointLimit = CheckJointLimits(jointDict.Get<JsonDictionary>("ballJointMotion"));

                jointMotion = new PlanarJointMotion(jointMotion.Type, rotationVec, slideVec, secondaryVec, rollValue, pitchValue, yawValue, primaryJointLimit, secondaryJointLimit, tertiaryJointLimit);
            }

            return jointMotion;
        }


        private Design.Material ExportMaterial(SharpGLTF.Schema2.Material gltfMaterial)
        {
            Design.Material SynthesisMaterial = new Design.Material();

            // string Id
            // string AppearanceId
            // MaterialProperties Properties

            SynthesisMaterial.Name = gltfMaterial.Name;

            return SynthesisMaterial;
        }

        private Design.Appearance ExportAppearance(SharpGLTF.Schema2.Material gltfAppearance)
        {
            Design.Appearance synthesisAppearance = new Design.Appearance();

            // todo:
            // string Id
            // string Name
            // bool HasTexture
            // AppearanceProperties Properties

            synthesisAppearance.Name = gltfAppearance.Name;

            foreach (SharpGLTF.Schema2.MaterialChannel materialProp in gltfAppearance.Channels)
            {
                synthesisAppearance.Properties = ExportAppearanceProperties(materialProp);
            }

            return synthesisAppearance;
        }

        private Design.AppearanceProperties ExportAppearanceProperties(MaterialChannel gltfAppearanceProp)
        {
            Design.AppearanceProperties synthesisAppearanceProps = new Design.AppearanceProperties();

            // todo:
            // Color Albedo
            // int Glossiness
            // HighlightsMode Highlights
            // int ReflectivityDirect
            // int ReflectivityOblique
            // int Transparency
            // int Translucency
            // int RefractiveIndex
            // Color SelfIlluminationColor
            // int SelfIlluminationLuminance
            // Attributes

            Design.Color albedo = synthesisAppearanceProps.Albedo;
            albedo.Red = (int)gltfAppearanceProp.Parameter.X;
            albedo.Green = (int)gltfAppearanceProp.Parameter.Y;
            albedo.Red = (int)gltfAppearanceProp.Parameter.Z;
            albedo.Alpha = (int)gltfAppearanceProp.Parameter.W;
            synthesisAppearanceProps.Albedo = albedo;

            return synthesisAppearanceProps;
        }

        public dynamic RecursiveThingy(dynamic parentObject, Dictionary<string, object> dict)
        {
            foreach (KeyValuePair<string, object> childData in dict)
            {
                PropertyInfo[] props = parentObject.GetType().GetProperties();
                PropertyInfo property = Array.Find(props, x => x.PropertyType.Name.ToLower().Equals(childData.Key.ToLower()));

                object childObj = property.GetValue(parentObject);

                if (childData.GetType() == typeof(Dictionary<string, object>))
                {
                    childObj = RecursiveThingy(childObj, (Dictionary<string, object>)childData.Value);
                }
                else
                {
                    childObj = childData.Value;
                }

                property.SetValue(parentObject, childObj);
            }
            return parentObject;
        }
    }

    public static class JsonDictionaryExtensions
    {
        public static T Get<T>(this JsonDictionary dict, string k) => (T)dict[k];
    }
}
