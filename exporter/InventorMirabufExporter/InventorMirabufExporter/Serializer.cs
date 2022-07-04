using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using System.Windows.Forms;
using Inventor;
using Mirabuf;

namespace InventorMirabufExporter
{
    public class Serializer
    {
        Assembly environment;
        char slash = System.IO.Path.DirectorySeparatorChar;

        public Serializer()
        {
            environment = new Assembly();
            //environment.Info = new Info();
            environment.Data = new AssemblyData();
            environment.PhysicalData = new PhysicalProperties();
            environment.DesignHierarchy = new GraphContainer();
            environment.JointHierarchy = new GraphContainer();
            environment.Transform = new Transform();
        }

        public void Setup(_Document assemblyDoc)
        {
            environment.Info = new Info()
            {
                GUID = assemblyDoc.DisplayName.Substring(0, assemblyDoc.DisplayName.LastIndexOf('.')),
                Name = assemblyDoc.DisplayName.Substring(0, assemblyDoc.DisplayName.LastIndexOf('.')),
                Version = 1
            };

            environment.Data.Parts = new Parts();
            environment.Data.Parts.Info = new Info()
            {
                GUID = assemblyDoc.InternalName.Trim('{', '}'),
                Version = 1,
                // Name?
            };

            // Assembly > AssemblyData > Part > Part Definition Loop
            for (int i = 0; i < assemblyDoc.ReferencedDocuments.Count; i++)
            {
                var docRef = assemblyDoc.ReferencedDocuments[i+1];

                try { MessageBox.Show(docRef.DisplayName, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                catch(Exception e) {MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                
                //environment.Data.Parts.PartDefinitionsEntry[i] = new PartDefinition();
                PartDefinition def = new PartDefinition();
                def.Info = new Info()
                {
                    GUID = docRef.InternalName.Trim('{', '}'),
                    Name = docRef.DisplayName.Substring(0, docRef.DisplayName.LastIndexOf('.')),
                    Version = 1
                };

                try { MessageBox.Show(def.Info.GUID, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }

                PartDocument doc = (PartDocument)docRef;

                /*
                if (docRef.DocumentType == DocumentTypeEnum.kPartDocumentObject)
                {
                    doc = (PartDocument)docRef;
                }
                */

                def.PhysicalData = new PhysicalProperties()
                {
                    Density = doc.ComponentDefinition.Material.Density,
                    Mass = doc.ComponentDefinition.MassProperties.Mass,
                    Volume = doc.ComponentDefinition.MassProperties.Volume,
                    Area = doc.ComponentDefinition.MassProperties.Area,
                    Com = new Vector3()
                    {
                        X = (float)doc.ComponentDefinition.MassProperties.CenterOfMass.X,
                        Y = (float)doc.ComponentDefinition.MassProperties.CenterOfMass.Y,
                        Z = (float)doc.ComponentDefinition.MassProperties.CenterOfMass.Z
                    }
                };

                // Mass Properties Debugging
                //try 
                //{ 
                //    MessageBox.Show("Density: " + def.PhysicalData.Density.ToString()
                //        + System.Environment.NewLine + "Mass: " + def.PhysicalData.Mass.ToString()
                //        + System.Environment.NewLine + "Volume: " + def.PhysicalData.Volume.ToString()
                //        + System.Environment.NewLine + "Area: " + def.PhysicalData.Area.ToString()
                //        , "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);

                //    MessageBox.Show("X: " + def.PhysicalData.Com.X.ToString()
                //        + System.Environment.NewLine + "Y: " + def.PhysicalData.Com.Y.ToString()
                //        + System.Environment.NewLine + "Z: " + def.PhysicalData.Com.Z.ToString()
                //        , "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);
                //}
                //catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }

                /*
                // needed?
                // Assembly > Assembly Data > PartDefinition > Transform
                def.BaseTransform = new Transform()
                {
                    SpatialMatrix = { 1, 2 }
                };
                */

                try { MessageBox.Show("BODY COUNT: " + doc.ComponentDefinition.SurfaceBodies.Count, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                // Assembly > AssemblyData > Part > PartDefinition > Body
                for (int j = 1; j < doc.ComponentDefinition.SurfaceBodies.Count + 1; j++)
                {
                    // independent instantiation better for debugging
                    Body solid = new Body();

                    solid.Info = new Info()
                    {
                        GUID = doc.InternalName.Trim('{', '}'), // what should this be?
                        Name = doc.ComponentDefinition.SurfaceBodies[j].Name,
                        Version = 1
                    };

                    solid.Part = doc.InternalName.Trim('{', '}'); // refers to PartDefinition GUID

                    solid.TriangleMesh = new TriangleMesh()
                    {
                        Info = new Info()
                        {
                            GUID = doc.InternalName.Trim('{', '}'), // refers to PartDefinition GUID
                            Name = doc.ComponentDefinition.SurfaceBodies[j].Name, // refers to name of solid
                            Version = 1
                        },

                        MaterialReference = doc.ComponentDefinition.Material.Name, // name of part material
                        
                        // Mesh Data Here...
                    };

                    // might need some tweaking
                    solid.AppearanceOverride = doc.ComponentDefinition.SurfaceBodies[j].Appearance.Name;

                    /*
                    // Body Properties Debugging
                    try { MessageBox.Show("SOLID: " + solid.Info.Name
                        + System.Environment.NewLine + "TriMesh Material: " + solid.TriangleMesh.MaterialReference
                        + System.Environment.NewLine + "Appearance Override: " + solid.AppearanceOverride
                        , "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                    catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                    */

                    //def.Bodies[j].TriangleMesh.Mesh = new Mesh(); ?

                    def.Bodies.Add(solid);
                    def.FrictionOverride = 5;
                    def.MassOverride = 5;
                }

                environment.Data.Parts.PartDefinitions.Add(def.Info.GUID, def);

                /*
                ComponentOccurrencesEnumerator leafOccurences = doc.ComponentDefinition.Occurrences.AllReferencedOccurrences[i];

                foreach(ComponentOccurrence compOcc in leafOccurences)
                {
                    try { MessageBox.Show("compOcc: " + compOcc.Name, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                    catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                }
                */

                AssemblyDocument asmDoc = (AssemblyDocument)assemblyDoc;
                ComponentOccurrencesEnumerator occurence = asmDoc.ComponentDefinition.Occurrences.AllReferencedOccurrences[doc];

                try { MessageBox.Show("Occurences: " + occurence.Count, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }

                Random rand = new Random();

                // Assembly > AssemblyData > Part > PartInstance Loop
                for (int j = 1; j <= occurence.Count; j++)
                {
                    PartInstance instance = new PartInstance();
                    instance.Info = new Info()
                    {
                        GUID = "idk" + rand.Next(1, 100), // what should this be?
                        Name = occurence[j].Name,
                        Version = 1
                    };

                    instance.PartDefinitionReference = docRef.InternalName;

                    instance.Transform = new Transform()
                    {
                        SpatialMatrix = { 1, 2 }
                    };

                    instance.GlobalTransform = new Transform()
                    {
                        SpatialMatrix = { 1, 2 }
                    };

                    /*
                    // needed?
                    // PartInstance > Joints
                    for (int j = 0; j < jointCount; j++)
                    {
                        instance.Joints[j] = "joint";
                    }
                    */

                    // needed?
                    //instance.Appearance = "appearance";

                    instance.PhysicalMaterial = doc.ComponentDefinition.Material.Name;

                    environment.Data.Parts.PartInstances.Add(instance.Info.GUID, instance);
                }
            }

            /*

            // Assembly > AssemblyData > Parts > UserData?

            // Assembly > AssemblyData > Joints?

            environment.Data.Materials = new Mirabuf.Material.Materials();
            for(int i = 0; i < partCount; i++)
            {
                Mirabuf.Material.PhysicalMaterial material = new Mirabuf.Material.PhysicalMaterial();

                material.Info = new Info()
                {
                    GUID = "rand" + i.ToString(),
                    Name = "myrand" + i.ToString(),
                    Version = 1
                };

                material.Description = "material";

                material.Thermal = new Mirabuf.Material.PhysicalMaterial.Types.Thermal()
                {
                    ThermalConductivity = 1,
                    SpecificHeat = 1,
                    ThermalExpansionCoefficient = 1
                };

                material.Mechanical = new Mirabuf.Material.PhysicalMaterial.Types.Mechanical()
                { 
                    YoungMod = 1,
                    PoissonRatio = 1,
                    ShearMod = 1,
                    Density = 1,
                    DampingCoefficient = 1
                };

                material.Strength = new Mirabuf.Material.PhysicalMaterial.Types.Strength()
                { 
                    YieldStrength = 1,
                    TensileStrength = 1,
                    ThermalTreatment = false
                };

                material.DynamicFriction = 0.5f;
                material.StaticFriction = 0.5f;
                material.Restitution = 0.5f;
                // Deformable?
                // matType?

                environment.Data.Materials.PhysicalMaterials.Add("key", material);
            }

            for(int i = 0; i < partCount; i++)
            {
                Mirabuf.Material.Appearance appearance = new Mirabuf.Material.Appearance();

                appearance.Info = new Info()
                {
                    GUID = "rand" + i.ToString(),
                    Name = "myrand" + i.ToString(),
                    Version = 1
                };

                appearance.Albedo = new Mirabuf.Color()
                {
                    R = 1,
                    G = 1,
                    B = 1,
                    A = 1
                };

                appearance.Roughness = 1;
                appearance.Metallic = 1;
                appearance.Specular = 1;

                environment.Data.Materials.Appearances.Add("key", appearance);
            }

            // Assembly > Data > Signals?

            MessageBox.Show(assemblyDoc.ReferencedDocuments.Count.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);

            //environment.Data.Parts.PartDefinitions.Add();

            environment.Dynamic = false;

            environment.PhysicalData = new PhysicalProperties()
            { 
                Density = 1,
                Mass = 1,
                Volume = 1,
                Area = 1,
                Com = new Vector3()
                { 
                    X = 1,
                    Y = 1,
                    Z = 1
                }
            };

            environment.DesignHierarchy = new GraphContainer();

            for(int i = 0; i < partCount; i++)
            {
                environment.DesignHierarchy.Nodes[i] = new Node();

                environment.DesignHierarchy.Nodes[i].Value = "value";

                // UserData?

                // Recursion?
                for(int j = 0; j < childCount; j++)
                {
                    environment.DesignHierarchy.Nodes[i].Children[j] = new Node();
                    environment.DesignHierarchy.Nodes[i].Children[j].Value = "childValue";
                    // UserData?
                }
            }

            environment.Transform = new Transform()
            {
                SpatialMatrix = { 1, 2 }
            };

            */
        }

        public void Serialize()
        {
            try
            {
                using (var output = System.IO.File.Create
                    (System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
                    + slash + "Autodesk" + slash + "Synthesis" + slash + "Mira" + slash + "test.mira"))
                {
                    environment.WriteTo(output);
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);
            }
        }

        private void test()
        {
            Assembly asm = new Assembly();
            asm.Info.GUID = "test";
        }
    }
}
