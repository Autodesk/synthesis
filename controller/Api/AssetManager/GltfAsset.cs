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
            }

            // joints are not attached to RootOccurrence directly but added to Joint dictionary
            foreach (JsonDictionary joint in (JsonList)extras["joints"])
            {
                Design.Joints.Add(ExportJointsFromExtras(joint));
                Console.WriteLine("Debug");
            }

            return design;
        }

        public Occurrence ExportOccurrenceFromNode(Node node)
        {
            // if node is in the Components maps, return it
            //if (Components[node.LogicalIndex].Equals(node.LogicalIndex))
            //{
            //    // return map component
            //} // else export the node

            Occurrence occurrence = new Occurrence();

            // todo:
            // OccurenceHeader
            // IsGrounded
            // Transform
            // ComponentUuid
            // Attributes

            //occurrence.AComponent = ExportComponentsFromMesh(node.Mesh);

            foreach (Node child in node.VisualChildren)
            {
                occurrence.AComponent = ExportComponentsFromMesh(child.Mesh);
                occurrence.ChildOccurences.Add(ExportOccurrenceFromNode(child));
            }

            return occurrence;
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

        private Design.Joint ExportJointsFromExtras(JsonDictionary jointDict)
        {
            Design.Joint joint = new Design.Joint();

            // todo:
            // JointHeader = new Header();
            // Direction = new Vector3();
            // Origin = new Vector3();
            // Type = JointType.RigidJointType;
            // Attributes = new Dictionary<string, object>();

            //RecursiveThingy(joint, jointDict);

            foreach (KeyValuePair<string, object> data in jointDict)
            {
                int i = 0;
                //RecursiveThingy(data, jointDict);
                foreach (KeyValuePair<string, object> childData in jointDict)
                {
                    switch (childData.Key)
                    {
                        case "header":
                            var jointHeader = joint.JointHeader;
                            jointHeader.Name = (string)(childData.Value as Dictionary<string, object>)["name"];
                            joint.JointHeader = jointHeader;
                            break;
                        case "origin":
                            var jointOrigin = joint.Origin;
                            jointOrigin.X = (double)(decimal)(childData.Value as Dictionary<string, object>)["x"];
                            jointOrigin.Y = (double)(decimal)(childData.Value as Dictionary<string, object>)["y"];
                            jointOrigin.Z = (double)(decimal)(childData.Value as Dictionary<string, object>)["z"];
                            joint.Origin = jointOrigin;
                            break;
                        //case "revoluteJointMotion":
                        //    var jointType = joint.Type;
                        //    joint.Type = (Design.Joint.JointType)(int)(childData.Value as Dictionary<string, object>)["y"];
                        //    joint.Type = jointType;
                        //    break;
                        case "occurrenceOneUUID":
                            //joint.OccurenceOneUuid = (string)(childData.Value as Dictionary<string, object>)["occurrenceOneUUID"];
                            joint.OccurenceOneUuid = (string)childData.Value;
                            break;
                        case "occurrenceTwoUUID":
                            joint.OccurenceTwoUuid = (string)childData.Value;
                            break;
                        default:
                            break;
                    }
                    i++;
                }
            }

            return joint;
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
}
