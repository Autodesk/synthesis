using System.Collections.Generic;
using System;
using System.Drawing;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using GopherAPI.STL;
using GopherAPI.Other;
using System.Diagnostics;

namespace GopherAPI.Reader
{
    public class FieldReader
    {
        private byte[] RawFile;

        private List<Section> Sections = new List<Section>();
        private List<RawMesh> RawMeshes = new List<RawMesh>();

        public Field LoadedField = new Field();

        /// <summary>
        /// Step 1/5: Breaks the file up into its constituent modules
        /// </summary>
        public void PreProcess()
        {
            using (var Reader = new BinaryReader(new MemoryStream(RawFile)))
            {
                //TODO: Implement Version Checker
                Reader.ReadBytes(80);
                while (true)
                {
                    Section Temp = new Section();
                    try
                    {
                        Temp.ID = (SectionType)Reader.ReadUInt32();
                        Temp.Length = Reader.ReadUInt32();
                        if ((uint)Temp.ID > 5)
                            Debug.WriteLine("WARNING: Bad Section ID detected");
                        Temp.Data = Reader.ReadBytes((int)Temp.Length);
                    }
                    catch (EndOfStreamException e)
                    { Console.WriteLine("End Of Stream at iteration " + (Sections.Count + 1).ToString()); break; }
                    Sections.Add(Temp);
                }
            }
        }

        /// <summary>
        /// Throws an exception if raw.ID is not 1
        /// </summary>
        /// <param name="raw"></param>
        private void PreProcessSTL(Section raw)
        {
            if (raw.ID != (SectionType)1)
            {
                throw new Exception("ERROR: Invalid Section passed to RobotReader.PreProcessSTL");
            }
            using (var Reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                uint MeshCount = Reader.ReadUInt32();
                for (uint i = 0; i < MeshCount; i++)
                {
                    RawMesh temp = new RawMesh();
                    //temp.MeshID = Reader.ReadUInt32();

                    temp.FacetCount = Reader.ReadUInt32();
                    temp.Facets = Reader.ReadBytes((int)(50 * temp.FacetCount));
                    
                    RawMeshes.Add(temp);
                }
            }
        }

        /// <summary>
        /// Step 2/5: Breaks up the STL Module(s) into their constituent meshes
        /// </summary>
        public void PreProcessSTL()
        {
            foreach (var sect in Sections)
            {
                if (sect.ID == (SectionType)1)
                    PreProcessSTL(sect);
            }
        }

        /// <summary>
        /// Step 3/5: Parses the .STL
        /// </summary>
        public void ProcessMeshes()
        {
            foreach (var rawMesh in RawMeshes)
            {
                List<Facet> tempFacets = new List<Facet>();
                Color TempColor = new Color(); bool TempIsDefault = false;
                uint TempAttID = 0;
                using (var Reader = new BinaryReader(new MemoryStream(rawMesh.Facets)))
                {
                    bool IsFirst = true; //Don't ask. I know it's messy :(
                    for (int i = 0; i < rawMesh.FacetCount; i++)
                    {
                        tempFacets.Add(new Facet(new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle()),
                            new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle()),
                            new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle()),
                            new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle())));
                        if (IsFirst)
                        {
                            TempColor = RobotReader.ParseColor(Reader.ReadBytes(2), out TempIsDefault);
                            IsFirst = false;
                        }
                        else
                            Reader.ReadBytes(2);
                    }
                    //TempAttID = Reader.ReadUInt32();
                }
                LoadedField.Meshes.Add(new STLMesh(rawMesh.MeshID, tempFacets.ToArray(), TempColor, TempIsDefault, TempAttID, null/*rawMesh.TransMat*/));

            }
        }

        public void ProcessMeshes2()
        {
            foreach(var section in Sections)
            {
                if(section.ID == (SectionType)1)
                {
                    PreProcessSTL(section);
                }
            }
            ProcessMeshes();
        }

        /// <summary>
        /// Throws an exception if raw.ID is not 2
        /// </summary>
        /// <param name="raw"></param>
        private void ProcessAttributes(Section raw)
        {
            if (raw.ID != (SectionType)2)
            {
                throw new Exception("ERROR: Invalid Section passed to ProcessAttributes");
            }
            using (var Reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                uint AttribCount = Reader.ReadUInt32();

                for (uint i = 0; i < AttribCount; i++)
                {
                    uint TempAttID = Reader.ReadUInt32();
                    Properties.AttribType TempType = (Properties.AttribType)Reader.ReadUInt16();
                    bool TempIsDynamic;

                    switch (TempType)
                    {
                        case Properties.AttribType.BOX_COLLIDER:
                            float TempX = Reader.ReadSingle(), TempY = Reader.ReadSingle(), TempZ = Reader.ReadSingle();
                            float TempFriction = Reader.ReadSingle();
                            TempIsDynamic = Reader.ReadBoolean();
                            if (TempIsDynamic)
                            {
                                LoadedField.Attributes.Add(new Properties.STLAttribute(TempType, TempAttID, TempFriction, TempIsDynamic, Reader.ReadSingle(), TempX, TempY, TempZ, null));
                            }
                            else
                            {
                                LoadedField.Attributes.Add(new Properties.STLAttribute(TempType, TempAttID, TempFriction, TempIsDynamic, null, TempX, TempY, TempZ, null));

                            }
                            break;
                        case Properties.AttribType.SPHERE_COLLIDER:
                            long PosCor = Reader.BaseStream.Position;
                            float TempG = Reader.ReadSingle();
                            float TempFric = Reader.ReadSingle();
                            TempIsDynamic = Reader.ReadBoolean();
                            if (TempIsDynamic)
                            {
                                LoadedField.Attributes.Add(new Properties.STLAttribute(TempType, TempAttID, TempFric, TempIsDynamic, Reader.ReadSingle(), null, null, null, TempG));
                            }
                            else
                            {
                                LoadedField.Attributes.Add(new Properties.STLAttribute(TempType, TempAttID, TempFric, TempIsDynamic, null, null, null, null, TempG));
                                if (Reader.BaseStream.Position != PosCor + 9)
                                {
                                    Console.WriteLine("\nWut");
                                }
                            }

                            break;
                        case Properties.AttribType.MESH_COLLIDER:
                            float TempF = Reader.ReadSingle();
                            TempIsDynamic = Reader.ReadBoolean();
                            if (TempIsDynamic)
                            {
                                LoadedField.Attributes.Add(new Properties.STLAttribute(TempType, TempAttID, TempF, TempIsDynamic, Reader.ReadSingle(), null, null, null, null));
                            }
                            else
                            {
                                LoadedField.Attributes.Add(new Properties.STLAttribute(TempType, TempAttID, TempF, TempIsDynamic, null, null, null, null, null));
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Step 4/5: Parses the raw attribute data
        /// </summary>
        public void ProcessAttributes()
        {
            foreach (var sect in Sections)
            {
                if (sect.ID == (SectionType)2)
                    ProcessAttributes(sect);
            }
        }

        /// <summary>
        /// Throws an exception if raw.ID is not 3
        /// </summary>
        /// <param name="raw"></param>
        private void ProcessJoints(Section raw)
        {
            if (raw.ID != (SectionType)3)
            {
                throw new Exception("ERROR: Invalid Section passed to ProcessJoints");
            }

            using (var Reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                uint JointCount = Reader.ReadUInt32();

                for (uint i = 0; i < JointCount; i++)
                {
                    byte[] GenericData = Reader.ReadBytes(46);

                    LoadedField.Joints.Add(new Properties.Joint(GenericData));
                }
            }
        }

        /// <summary>
        /// Step 5/5: Parses the raw Joint data.
        /// </summary>
        public void ProcessJoints()
        {
            foreach (var sect in Sections)
            {
                if (sect.ID == (SectionType)3)
                    ProcessJoints(sect);
            }
        }

        /// <summary>
        /// Step 1.5/5: Parses thumbnail image
        /// </summary>
        public void ProcessImage()
        {
            foreach(var sect in Sections)
            {
                if (sect.ID == 0)
                {
                    LoadedField.Thumbnail = new Bitmap(new MemoryStream(sect.Data)); 
                }
            }
        }

        /// <summary>
        /// This just executes all of the loading functions in the correct order. However, you can't do a loading bar if you use this function. 
        /// </summary>
        public void LoadField()
        {
            PreProcess();
            ProcessImage();
            PreProcessSTL();
            ProcessMeshes();
            ProcessAttributes();
            ProcessJoints();
        }

        public FieldReader(string path)
        {
            using (var Reader = new BinaryReader(File.Open(path, FileMode.Open), Encoding.Default))
            {
                RawFile = Reader.ReadBytes((int)Reader.BaseStream.Length);
            }
        }
    }
}
