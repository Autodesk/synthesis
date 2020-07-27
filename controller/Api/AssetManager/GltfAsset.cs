using System;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.VirtualFileSystem;
using glTFLoader;
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

namespace SynthesisAPI.AssetManager
{
    public class GltfAsset : Asset
    {
        public GltfAsset(string name, Guid owner, Permissions perm, string sourcePath)
        {
            Init(name, owner, perm, sourcePath);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed");
            }

            return model;
        }

        public Design ImportDesign(ModelRoot modelRoot)
        {
            Design design = new Design();
            JsonDictionary extras = (JsonDictionary)modelRoot.Extras;

            // this is the root ---> modelRoot.DefaultScence.VisualChildren
            foreach (Node child in modelRoot.DefaultScene.VisualChildren)
            {
                design.RootOccurence = ExportOccurrenceFromNode(child);
                design.RootOccurence.Transform = ExportMatrix(child.LocalMatrix);
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
                occurrence.Transform = ExportMatrix(child.LocalMatrix);
                occurrence.ChildOccurences.Add(ExportOccurrenceFromNode(child));
            }

            return occurrence;
        }

        private Design.Matrix3D ExportMatrix(System.Numerics.Matrix4x4 matrix4x4)
        {
            Matrix3D matrix3D = new Matrix3D(matrix4x4.M11, matrix4x4.M12, matrix4x4.M13, matrix4x4.M14, matrix4x4.M21, matrix4x4.M22, matrix4x4.M23, matrix4x4.M24, matrix4x4.M31, matrix4x4.M32, matrix4x4.M33, matrix4x4.M34, matrix4x4.M41, matrix4x4.M42, matrix4x4.M43, matrix4x4.M44);

            return matrix3D;
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
            var jointType = joint.Type;
            var jointMotion = joint.JointMotion;
            var jointRotationVector = jointMotion.RotationVector;
            var jointSlideVector = jointMotion.SlideVector;
            var jointSecondaryVector = jointMotion.SecondarySlideVector;

            if (jointDict.ContainsKey("revoluteJointMotion"))
            {
                double rotationValue;

                jointRotationVector.Y = (double)jointDict.Get<JsonDictionary>("revoluteJointMotion").Get<JsonDictionary>("rotationAxisVector").Get<decimal>("y");
                rotationValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("revoluteJointMotion"), "rotationValue");

                jointMotion = new RevoluteJointMotion(jointRotationVector, rotationValue);
            }

            if (jointDict.ContainsKey("sliderJointMotion"))
            {
                double slideValue;

                // CHECK vectors x, y, z if they exist; current test models don't have x, y, z
                jointRotationVector.Y = (double)jointDict.Get<JsonDictionary>("sliderJointMotion").Get<JsonDictionary>("slideDirectionVector").Get<decimal>("y");
                slideValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("sliderJointMotion"), "slideValue");
                jointMotion = new SliderJointMotion(jointRotationVector, slideValue);
            }

            if (jointDict.ContainsKey("cylindricalJointMotion"))
            {
                double rotationValue;
                double slideValue;

                jointRotationVector.Y = (double)jointDict.Get<JsonDictionary>("cylindricalJointMotion").Get<JsonDictionary>("rotationAxisVector").Get<decimal>("y");
                rotationValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("cylindricalJointMotion"), "rotationValue");
                slideValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("cylindricalJointMotion"), "slideValue");
                jointMotion = new CylindricalJointMotion(jointRotationVector, rotationValue, slideValue);
            }

            if (jointDict.ContainsKey("pinSlotJointMotion"))
            {
                double rotationValue;
                double slideValue;

                jointRotationVector.Y = (double)jointDict.Get<JsonDictionary>("pinSlotJointMotion").Get<JsonDictionary>("rotationAxisVector").Get<decimal>("y");
                jointSlideVector.Y = (double)jointDict.Get<JsonDictionary>("pinSlotJointMotion").Get<JsonDictionary>("slideDirectionVector").Get<decimal>("y");
                rotationValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("pinSlotJointMotion"), "rotationValue");
                slideValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("pinSlotJointMotion"), "slideValue");

                jointMotion = new PinSlotJointMotion(jointRotationVector, jointSlideVector, rotationValue, slideValue);
            }

            if (jointDict.ContainsKey("planarJointMotion"))
            {
                double primarySlideValue;
                double secondarySlideValue;
                double rotationValue;

                jointRotationVector.Y = (double)jointDict.Get<JsonDictionary>("planarJointMotion").Get<JsonDictionary>("normalDirectionVector").Get<decimal>("y");
                jointSlideVector.Y = (double)jointDict.Get<JsonDictionary>("planarJointMotion").Get<JsonDictionary>("primarySlideDirectionVector").Get<decimal>("y");
                jointSecondaryVector.Y = (double)jointDict.Get<JsonDictionary>("planarJointMotion").Get<JsonDictionary>("secondarySlideDirectionVector").Get<decimal>("y");
                primarySlideValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("planarJointMotion"), "primarySlideValue");
                secondarySlideValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("planarJointMotion"), "secondarySlideValue");
                rotationValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("planarJointMotion"), "rotationValue");

                jointMotion = new PlanarJointMotion(jointRotationVector, jointSlideVector, jointSecondaryVector, primarySlideValue, secondarySlideValue, rotationValue);
            }

            if (jointDict.ContainsKey("ballJointMotion"))
            {
                double rollValue;
                double pitchValue;
                double yawValue;

                jointRotationVector.Y = (double)jointDict.Get<JsonDictionary>("ballJointMotion").Get<JsonDictionary>("rollDirectionVector").Get<decimal>("y");
                jointSlideVector.Y = (double)jointDict.Get<JsonDictionary>("ballJointMotion").Get<JsonDictionary>("pitchDirectionVector").Get<decimal>("y");
                jointSecondaryVector.Y = (double)jointDict.Get<JsonDictionary>("ballJointMotion").Get<JsonDictionary>("yawDirectionVector").Get<decimal>("y");
                rollValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("ballJointMotion"), "rollValue");
                pitchValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("ballJointMotion"), "pitchValue");
                yawValue = (double)HasJointDetails(jointDict.Get<JsonDictionary>("ballJointMotion"), "yawValue");

                jointMotion = new PlanarJointMotion(jointRotationVector, jointSlideVector, jointSecondaryVector, rollValue, pitchValue, yawValue);
            }

            //jointMotion.JointVector = jointRotationVector;
            joint.JointMotion = jointMotion;
            joint.Type = jointType;

            return joint;
        }

        private double HasJointDetails(JsonDictionary dict, string key)
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

        private double GetJointVectorValue(JsonDictionary dict, string key, double value)
        {

            return value;
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
