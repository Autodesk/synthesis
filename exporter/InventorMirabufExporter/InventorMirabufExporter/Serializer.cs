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
        Random rand = new Random(); // temp
        uint version = 4; // temp
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

        public void Setup(AssemblyDocument assemblyDoc)
        {
            environment.Info = new Info()
            {
                GUID = assemblyDoc.DisplayName.Substring(0, assemblyDoc.DisplayName.LastIndexOf('.')),
                Name = assemblyDoc.DisplayName.Substring(0, assemblyDoc.DisplayName.LastIndexOf('.')),
                Version = version
            };

            environment.Data.Parts = new Parts();
            environment.Data.Parts.Info = new Info()
            {
                GUID = assemblyDoc.InternalName.Trim('{', '}'),
                Version = version,
                // Name?
            };

            // Part Definition For Assembly Document
            PartDefinition asmDef = new PartDefinition()
            {
                Info = new Info()
                {
                    GUID = assemblyDoc.InternalName.Trim('{', '}'),
                    Name = assemblyDoc.DisplayName.Substring(0, assemblyDoc.DisplayName.LastIndexOf('.')),
                    Version = version
                },

                PhysicalData = new PhysicalProperties()
                {
                    Density = assemblyDoc.ComponentDefinition.DefaultMaterial.Density,
                    Mass = assemblyDoc.ComponentDefinition.MassProperties.Mass,
                    Volume = assemblyDoc.ComponentDefinition.MassProperties.Volume,
                    Area = assemblyDoc.ComponentDefinition.MassProperties.Area,
                    Com = new Vector3()
                    {
                        X = (float)assemblyDoc.ComponentDefinition.MassProperties.CenterOfMass.X,
                        Y = (float)assemblyDoc.ComponentDefinition.MassProperties.CenterOfMass.Y,
                        Z = (float)assemblyDoc.ComponentDefinition.MassProperties.CenterOfMass.Z
                    }
                }
            };

            // Part Instance For Assembly Document
            PartInstance asmInstance = new PartInstance()
            {
                Info = new Info()
                {
                    GUID = assemblyDoc.InternalName.Trim('{', '}'),
                    Name = assemblyDoc.DisplayName.Substring(0, assemblyDoc.DisplayName.LastIndexOf('.')),
                    Version = version
                },
                PartDefinitionReference = assemblyDoc.InternalName.Trim('{', '}')
            };

            // Add Assembly Doc Part Definition & Instance
            environment.Data.Parts.PartDefinitions.Add(asmDef.Info.GUID, asmDef);
            environment.Data.Parts.PartInstances.Add(asmInstance.Info.GUID, asmInstance);

            // Assembly > AssemblyData > Part > Part Definition Loop
            for (int i = 0; i < assemblyDoc.ReferencedDocuments.Count; i++)
            {
                var docRef = assemblyDoc.ReferencedDocuments[i+1];

                try { MessageBox.Show(docRef.DisplayName, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }

                //environment.Data.Parts.PartDefinitionsEntry[i] = new PartDefinition();
                PartDefinition def = new PartDefinition();
                def.Info = new Info()
                {
                    GUID = docRef.InternalName.Trim('{', '}'),
                    Name = docRef.DisplayName.Substring(0, docRef.DisplayName.LastIndexOf('.')),
                    Version = version
                };

                try { MessageBox.Show($"Part Def {i} GUID: " + def.Info.GUID, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
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
                        GUID = doc.ComponentDefinition.SurfaceBodies[j].Name, // should be unique to solid
                        Name = doc.ComponentDefinition.SurfaceBodies[j].Name,
                        Version = version
                    };

                    solid.Part = doc.InternalName.Trim('{', '}'); // refers to PartDefinition GUID

                    solid.TriangleMesh = new TriangleMesh()
                    {
                        Info = new Info()
                        {
                            GUID = doc.ComponentDefinition.SurfaceBodies[j].Name, // should refer to solid GUID
                            Name = doc.ComponentDefinition.SurfaceBodies[j].Name, // refers to name of solid
                            Version = version
                        },

                        MaterialReference = doc.ComponentDefinition.Material.Name, // name of part material

                        Mesh = new Mesh()
                    };

                    int FacetCount, VertexCount;
                    var VertexIndices = new int[] { };
                    var VertexCoordinates = new double[] { };
                    var NormalVectors = new double[] { };
                    var TextureCoordinates = new double[] { };

                    doc.ComponentDefinition.SurfaceBodies[j].CalculateFacetsAndTextureMap(1, out VertexCount, out FacetCount, out VertexCoordinates, out NormalVectors, out VertexIndices, out TextureCoordinates);

                    // print vertex coordinates
                    try { MessageBox.Show("VCoords: " + VertexCoordinates.Length, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                    catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }

                    foreach (double vert in VertexCoordinates)
                        solid.TriangleMesh.Mesh.Verts.Add((float)vert);

                    foreach (double normal in NormalVectors)
                        solid.TriangleMesh.Mesh.Normals.Add((float)normal);

                    foreach (double uv in TextureCoordinates)
                        solid.TriangleMesh.Mesh.Uv.Add((float)uv);

                    foreach (int index in VertexIndices)
                        solid.TriangleMesh.Mesh.Indices.Add(index-1);

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

                ComponentOccurrencesEnumerator occurence = assemblyDoc.ComponentDefinition.Occurrences.AllReferencedOccurrences[doc];

                try { MessageBox.Show("Occurences: " + occurence.Count, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }

                // Assembly > AssemblyData > Part > PartInstance Loop
                for (int j = 1; j <= occurence.Count; j++)
                {
                    PartInstance instance = new PartInstance();
                    instance.Info = new Info()
                    {
                        GUID = occurence[j].Name,
                        Name = occurence[j].Name,
                        Version = version
                    };

                    instance.PartDefinitionReference = docRef.InternalName.Trim('{', '}');

                    instance.Transform = new Transform();
                    instance.GlobalTransform = new Transform();

                    double[] matrix = new double[16];

                    occurence[j].Transformation.GetMatrixData(ref matrix);

                    for (int k = 0; k < matrix.Length; k++)
                    {
                        // Transform & GlobalTransform same for now...
                        instance.Transform.SpatialMatrix.Add((float)matrix[k]);
                        instance.GlobalTransform.SpatialMatrix.Add((float)matrix[k]);
                    }

                    /*
                    // Transform Debugging (requires engine testing)
                    try { MessageBox.Show($"Matrix: " + instance.Transform.SpatialMatrix, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                    catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                    */

                    // PartInstance > Joints?

                    // not needed?
                    //instance.Appearance = "appearance";

                    instance.PhysicalMaterial = doc.ComponentDefinition.Material.Name;

                    environment.Data.Parts.PartInstances.Add(instance.Info.GUID, instance);
                }
            }

            // Assembly > AssemblyData > Parts > UserData?

            // Assembly > AssemblyData > Joints
            environment.Data.Joints = new Mirabuf.Joint.Joints();
            environment.Data.Joints.Info = new Info()
            {
                GUID = "completelyrandomjointguid",
                Version = version
            };

            Mirabuf.Joint.Joint jointDef = new Mirabuf.Joint.Joint()
            {
                Info = new Info()
                {
                    GUID = "groundedJointGUID",
                    Name = "grounded",
                    Version = version
                }

                // Potentially More Definition Data Here
            };

            environment.Data.Joints.JointDefinitions.Add(jointDef.Info.Name, jointDef);

            Mirabuf.Joint.JointInstance jointInstance = new Mirabuf.Joint.JointInstance()
            {
                Info = new Info()
                {
                    GUID = "uniqueJointInstanceGUID",
                    Name = "grounded",
                    Version = version
                },

                JointReference = jointDef.Info.GUID,

                Parts = new GraphContainer()
            };

            // Assembly > AssemblyData > Materials
            environment.Data.Materials = new Mirabuf.Material.Materials();

            environment.Data.Materials.Info = new Info()
            {
                GUID = "uniqueMaterialsGUID",
                Version = version
            };

            try { MessageBox.Show($"Total Materials: " + assemblyDoc.MaterialAssets.Count, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
            catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }

            // Assembly > AssemblyData > Materials > Physical Materials
            for (int i = 1; i <= assemblyDoc.MaterialAssets.Count; i++)
            {
                Mirabuf.Material.PhysicalMaterial material = new Mirabuf.Material.PhysicalMaterial();

                material.Info = new Info()
                {
                    GUID = "unique" + rand.Next(1, 100), // can be same as name?
                    Name = assemblyDoc.MaterialAssets[i].Name,
                    Version = version
                };

                try { MessageBox.Show($"Material {i} Name: " + material.Info.Name, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }

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

                environment.Data.Materials.PhysicalMaterials.Add(material.Info.Name, material);
            }

            // Assembly > AssemblyData > Materials > Appearances
            for (int i = 1; i <= assemblyDoc.AppearanceAssets.Count; i++)
            {
                Mirabuf.Material.Appearance appearance = new Mirabuf.Material.Appearance();

                appearance.Info = new Info()
                {
                    GUID = "unique" + rand.Next(1, 100), // can be same as name?
                    Name = assemblyDoc.AppearanceAssets[i].Name,
                    Version = version
                };

                appearance.Albedo = new Mirabuf.Color()
                {
                    R = 127,
                    G = 127,
                    B = 127,
                    A = 127
                };

                /*
                // Get Actual Appearance Data
                byte red, green, blue;
                assemblyDoc.RenderStyles[assemblyDoc.AppearanceAssets[i]].GetAmbientColor(out red, out green, out blue);

                appearance.Albedo.R = red;
                appearance.Albedo.G = green;
                appearance.Albedo.B = blue;


                try { MessageBox.Show($"Appearance {i} Colors: " + "R: " + appearance.Albedo.R +
                    System.Environment.NewLine + "G: " + appearance.Albedo.G +
                    System.Environment.NewLine + "B: " + appearance.Albedo.B, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                catch (Exception e) { MessageBox.Show(e.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK); }
                */

                appearance.Roughness = 0.5;
                appearance.Metallic = 0.5;
                appearance.Specular = 0.5;

                environment.Data.Materials.Appearances.Add(appearance.Info.GUID, appearance);
            }

            // Assembly > Data > Signals?

            MessageBox.Show("Referenced Docs: " + assemblyDoc.ReferencedDocuments.Count.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);

            //environment.Data.Parts.PartDefinitions.Add();

            // Assembly > Dynamic
            environment.Dynamic = false;

            /*
            // needed?
            // Assembly > Physical Data
            environment.PhysicalData = new PhysicalProperties()
            { 
                Density = 1,
                Mass = assemblyDoc.ComponentDefinition.MassProperties.Mass,
                Volume = assemblyDoc.ComponentDefinition.MassProperties.Volume,
                Area = 1,
                Com = new Vector3()
                { 
                    X = 1,
                    Y = 1,
                    Z = 1
                }
            };
            */

            // Assembly > Design Hierarchy
            environment.DesignHierarchy = new GraphContainer();
            Node node = new Node();
            node.Value = assemblyDoc.InternalName.Trim('{', '}');

            // Recursion?
            for (int i = 0; i < assemblyDoc.ReferencedDocuments.Count; i++)
            {
                PartDocument doc = (PartDocument)assemblyDoc.ReferencedDocuments[i + 1];
                ComponentOccurrencesEnumerator occurence = assemblyDoc.ComponentDefinition.Occurrences.AllReferencedOccurrences[doc];

                for (int j = 0; j < occurence.Count; j++)
                {
                    Node subNode = new Node();
                    subNode.Value = occurence[j+1].Name;
                    node.Children.Add(subNode);

                    // Assembly > Data > Joints > JointInstances > Parts > Nodes
                    // Check For Grounded Joint
                    if (occurence[j+1].Grounded)
                    {
                        MessageBox.Show("Grounded Occurence: " + occurence[j+1].Name, "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);
                        jointInstance.Parts.Nodes.Add(subNode);
                    }

                    // UserData?
                }
            }

            // Assembly > Data > Joints > JointInstances
            environment.Data.Joints.JointInstances.Add(jointInstance.Info.Name, jointInstance);

            environment.DesignHierarchy.Nodes.Add(node);

            // Assembly > Design Hierarchy > Node > UserData?

            // Assembly > JointHierarchy
            environment.JointHierarchy = new GraphContainer();
            Node grounded = new Node() { Value = "ground" };
            environment.JointHierarchy.Nodes.Add(grounded);

            /*
            // needed?
            // Assembly > Transform
            environment.Transform = new Transform()
            {
                SpatialMatrix = { 1, 2 }
            };
            */

            Serialize();
        }

        public void Serialize()
        {
            try
            {
                using (var output = System.IO.File.Create
                    (System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
                    + slash + "Autodesk" + slash + "Synthesis" + slash + "Mira" + slash + "Fields" + slash + "test.mira"))
                {
                    environment.WriteTo(output);
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.ToString(), "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);
            }
        }
    }
}
