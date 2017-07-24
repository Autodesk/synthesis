using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using GopherAPI;
using GopherAPI.STL;
using GopherAPI.Nodes;
using GopherAPI.Other;
using GopherAPI.Nodes.Joint;
using GopherAPI.Nodes.Colliders;

namespace GopherAPI.Reader
{
    internal class GopherReader_Base
    {
        private byte[] rawFile;
        internal List<Section> sections = new List<Section>();
        internal List<RawMesh> rawMeshes = new List<RawMesh>();

        /// <summary>
        /// Loads given file into memory
        /// </summary>
        /// <param name="path">The file to be opened</param>
        internal GopherReader_Base(string path)
        {
            if (Path.GetExtension(path).ToLower() != ".field" || Path.GetExtension(path).ToLower() != ".robot")
                throw new ArgumentException("ERROR: path given was not a field or robot file", "path");
            Gopher.ProgressCallback("Loading " + Path.GetFileName(path) + "into memory");
            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                rawFile = reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }
        private GopherReader_Base() { }
        
        /// <summary>
        /// Step 1: Breaks up rawFile into its constituent Sections
        /// </summary>
        internal void PreProcess()
        {
            using (var reader = new BinaryReader(new MemoryStream(rawFile)))
            {
                reader.ReadBytes(80);
                while (true)
                {
                    var temp = new Section
                    {
                        ID = (SectionType)reader.ReadUInt32(),
                        Data = reader.ReadBytes((int)reader.ReadUInt32())
                    };
                    try
                    {
                        reader.PeekChar();
                        continue;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Reads a thumbnail from an image section
        /// </summary>
        /// <param name="raw"></param>
        /// <returns>a Bitmap object</returns>
        internal Bitmap ProcessThumbnail(Section raw)
        {
            if (raw.ID != SectionType.IMAGE)
                throw new ArgumentException("ERROR: Non image section passed to ProcessThumbnail", "raw");
            return new Bitmap(new MemoryStream(raw.Data));
        }

        /// <summary>
        /// Reads a thumbnail from an image section
        /// </summary>
        /// <returns>a Bitmap object</returns>
        internal Bitmap ProcessThumbnail()
        {
            foreach(var section in sections)
            {
                if (section.ID == SectionType.IMAGE)
                    return ProcessThumbnail(section);
            }
            throw new Exception("ERROR: No thumbnail found in file");
        }

        /// <summary>
        /// Step 2: Processes all of the non facet information into RawMesh structs
        /// </summary>
        /// <param name="raw">Section to be passed</param>
        internal void PreProcessSTL(Section raw)
        {
            if (raw.ID != SectionType.STL)
                throw new ArgumentException("Non-STL section passed to PreProcessSTL", "raw");
            using (var reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                var meshCount = reader.ReadUInt32();
                for(int i = 0; i < meshCount; i++)
                {
                    var rawTemp = new RawMesh();
                    
                    rawTemp.MeshID = reader.ReadUInt32(); //Read ID of the mesh

                    rawTemp.TransMat = new TransformationMatrix(new float[]
                    {
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
                    }); //Read Transformation Matrix

                    reader.ReadBytes(80); //Skip over the STL header

                    rawTemp.FacetCount = reader.ReadUInt32(); //Read number of facets

                    rawTemp.Facets = reader.ReadBytes((int)(rawTemp.FacetCount * 50)); //Read all facets

                    rawTemp.AttribID = reader.ReadUInt32(); //Read the id of the attribute/collider this mesh uses
                }
            }
        }

        /// <summary>
        /// Step 2: Processes all of the non facet information into RawMesh structs
        /// </summary>
        internal void PreProcessSTL()
        {
            foreach(var section in sections)
            {
                if (section.ID == SectionType.STL)
                    PreProcessSTL(section);
            }
        }
        
        /// <summary>
        /// Step 3: Processes the facets of all the RawMeshes and adds the PreProcessed data into an STLMesh object
        /// </summary>
        /// <returns>A list of STLMeshes to be used by the RobotReader and FieldReader objects</returns>
        internal List<STLMesh> ProcessSTL()
        {
            var ret = new List<STLMesh>();
            if (rawMeshes.Count == 0)
                throw new Exception("ERROR: No preprocessed meshes found. Have you PreProcessed your STL?");
            foreach(var raw in rawMeshes)
            {
                var meshTemp = new STLMesh
                {
                    MeshID = raw.MeshID,
                    TransMat = raw.TransMat,
                    AttributeID = raw.AttribID
                }; //initializes an STLMesh with all of the known mesh data from the rawMesh

                //Process all of the facets
                List<Facet> facetsTemp = new List<Facet>();
                for(uint i = 0; i < raw.FacetCount; i++)
                {
                    using (var reader = new BinaryReader(new MemoryStream(raw.Facets)))
                    {
                        //Not as messy as it looks. Reads the 12 floats that constitute the facets
                        facetsTemp.Add(new Facet(
                            new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                            new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                            new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                            new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                            GopherColors.ParseColor(reader.ReadBytes(2), out bool isD), isD));
                    }
                }
                ret.Add(meshTemp);
            }

            return ret;
        }

        /// <summary>
        /// Step 4: Processes the joints in the passed section
        /// </summary>
        /// <param name="raw"></param>
        /// <returns>A list of GopherJoint_Base containing all the joints in the file</returns>
        internal List<GopherJoint_Base> ProcessJoints(Section raw)
        {
            if (raw.ID != SectionType.JOINT)
                throw new ArgumentException("ERROR: Non joint section passed to ProcessJoints", "raw");
            List<GopherJoint_Base> jointsTemp = new List<GopherJoint_Base>();
            using (var reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                uint jointCount = reader.ReadUInt32();
                for (uint i = 0; i < jointCount; i++)
                {
                    uint tempID = reader.ReadUInt32();
                    GopherJointType jt = (GopherJointType)reader.ReadUInt16();

                    //Prepare for epic switch statement
                    switch (jt)
                    {
                        case GopherJointType.ROTATIONAL:
                            jointsTemp.Add(new RotationalJoint
                            {
                                Meta = new JointMeta(tempID, reader.ReadUInt32(), reader.ReadUInt32()),
                                NormalVector = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                RelativePoint = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                AngularFreedomFactor = new Vec2(reader.ReadSingle(), reader.ReadSingle())
                            }); 
                            break;
                        case GopherJointType.LINEAR:
                            jointsTemp.Add(new LinearJoint
                            {
                                Meta = new JointMeta(tempID, reader.ReadUInt32(), reader.ReadUInt32()),
                                DefiningVector = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                ConnectionPoint = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                LinearFreedomFactor = new Vec2(reader.ReadSingle(), reader.ReadSingle())
                            });
                            break;
                        case GopherJointType.CYLINDRICAL:
                            jointsTemp.Add(new CylindricalJoint
                            {
                                Meta = new JointMeta(tempID, reader.ReadUInt32(), reader.ReadUInt32()),
                                NormalVector = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                DefiningVector = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                RelativePoint = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                ConnectionPoint = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                LinearFreedomFactor = new Vec2(reader.ReadSingle(), reader.ReadSingle()),
                                AngularFreedomFactor = new Vec2(reader.ReadSingle(), reader.ReadSingle())
                            });
                            break;
                        case GopherJointType.PLANAR:
                            jointsTemp.Add(new PlanarJoint
                            {
                                Meta = new JointMeta(tempID, reader.ReadUInt32(), reader.ReadUInt32()),
                                PlanarVector = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                ConnectionPoint = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                TransFreedomFactorPar = new Vec2(reader.ReadSingle(), reader.ReadSingle()),
                                TransFreedomFactorPer = new Vec2(reader.ReadSingle(), reader.ReadSingle())
                            });
                            break;
                        case GopherJointType.BALL:
                            jointsTemp.Add(new BallJoint
                            {
                                Meta = new JointMeta(tempID, reader.ReadUInt32(), reader.ReadUInt32()),
                                ConnectionPoint = new Vec3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                AngularFreedom1 = new Vec2(reader.ReadSingle(), reader.ReadSingle()),
                                AngularFreedom2 = new Vec2(reader.ReadSingle(), reader.ReadSingle()),
                                AngularFreedom3 = new Vec2(reader.ReadSingle(), reader.ReadSingle())

                            });
                            break;
                        default:
                            throw new Exception("ERROR: Improper file formatting (thrown in ProcessJoints)");
                    }

                }
            }
            return jointsTemp;
        }

        /// <summary>
        /// Step 4: Processes the joint section 
        /// </summary>
        /// <returns>A list of GopherJoint_Base containing all the joints in the file</returns>
        internal List<GopherJoint_Base> ProcessJoints()
        {
            foreach(var section in sections)
            {
                if(section.ID == SectionType.JOINT)
                {
                    return ProcessJoints(section);
                }
            }
            throw new Exception("ERROR: No joints in file");
        }
    }
}