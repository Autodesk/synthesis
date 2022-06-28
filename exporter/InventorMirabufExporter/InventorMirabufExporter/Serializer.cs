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
                GUID = assemblyDoc.InternalName,
                Name = assemblyDoc.DisplayName,
                Version = 1
            };

            environment.Data.Parts = new Parts();
            environment.Data.Parts.Info = new Info()
            {
                GUID = "rand",
                Version = 1,
                // Name?
            };

            // Assembly > AssemblyData > Part > Part Definition Loop
            for (int i = 0; i < assemblyDoc.ReferencedDocuments.Count; i++)
            {
                //environment.Data.Parts.PartDefinitionsEntry[i] = new PartDefinition();
                PartDefinition def = new PartDefinition();
                def.Info = new Info()
                {
                    GUID = "rand" + i.ToString(),
                    Name = "myrand" + i.ToString(),
                    Version = 1
                };

                def.PhysicalData = new PhysicalProperties()
                {
                    Density = 1,
                    Mass = 1,
                    Volume = 1,
                    Area = 1,
                    Com = new Vector3()
                    {
                        X = 1,
                        Y = 2,
                        Z = 3
                    }
                };

                // needed?
                def.BaseTransform = new Transform()
                {
                    SpatialMatrix = { 1, 2 }
                };

                for (int j = 0; j < bodyCount; j++)
                {
                    def.Bodies[j] = new Body()
                    {
                        Info = new Info()
                        {
                            GUID = "rand" + j.ToString(),
                            Name = "myrand" + j.ToString(),
                            Version = 1
                        },

                        Part = "part",

                        TriangleMesh = new TriangleMesh()
                        {
                            Info = new Info()
                            {
                                GUID = "rand" + i.ToString(),
                                Name = "myrand" + i.ToString(),
                                Version = 1
                            },

                            MaterialReference = "metal",
                        },

                        AppearanceOverride = "override"
                    };

                    //def.Bodies[j].TriangleMesh.Mesh = new Mesh(); ?

                    def.FrictionOverride = 5;
                    def.MassOverride = 5;
                }

                MessageBox.Show("before add", "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);
                environment.Data.Parts.PartDefinitions.Add("key", def);
                MessageBox.Show("after add", "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);
            }

            // Assembly > AssemblyData > Part > PartInstance Loop
            for(int i = 0; i < instanceCount; i++)
            {
                PartInstance instance = new PartInstance();
                instance.Info = new Info()
                {
                    GUID = "rand" + i.ToString(),
                    Name = "myrand" + i.ToString(),
                    Version = 1
                };

                instance.PartDefinitionReference = "ref";

                instance.Transform = new Transform()
                {
                    SpatialMatrix = { 1, 2 }
                };

                instance.GlobalTransform = new Transform()
                {
                    SpatialMatrix = { 1, 2 }
                };

                for(int j = 0; j < jointCount; j++)
                {
                    instance.Joints[j] = "joint";
                }

                instance.Appearance = "appearance";
                instance.PhysicalMaterial = "physical material";

                environment.Data.Parts.PartInstances.Add("key", instance);
            }

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
