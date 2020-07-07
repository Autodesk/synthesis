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

                // "Full_Robot_Rough_v10_1593496385.glb
                model = ModelRoot.ReadGLB(stream, settings);
                //model = ModelRoot.Load("MultiDepthHierarchy_v9_1593489237.glb", settings);

                // ImportDesign to trigger recursive calls
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

            // this is the root ---> modelRoot.DefaultScence.VisualChildren
            foreach (Node child in modelRoot.DefaultScene.VisualChildren)
            {
                // todo: ImportComponents needs ALL components
                //Components = ImportComponents(child);

                design.RootOccurence = ImportOccurence(child);
            }

            return design;
        }

        //private IDictionary<int, Design.Component> ImportComponents(Node node)
        //{
        //    foreach (Node child in node.VisualChildren)
        //    {
        //        ImportComponents(Components.Add(node.VisualChildren.GetEnumerator, child));
        //    }
        //    return Components;

        //}

        public Occurence ImportOccurence(Node node)
        {
            Occurence occurence = new Occurence();

            foreach (Node child in node.VisualChildren)
            {
                occurence.ChildOccurences.Add(ImportOccurence(child));
            }

            return occurence;
        }
    }
}
