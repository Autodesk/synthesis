using GopherAPI.Other;
using GopherAPI.STL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using GopherAPI.Properties;

namespace GopherAPI.Reader
{
    public class RobotReader
    {
        private byte[] RawFile;
        private List<Section> Sections = new List<Section>();
        private List<RawMesh> RawMeshes = new List<RawMesh>();
        
        public Robot LoadedRobot = new Robot();

        byte MajorV, MinorV, PatchV, InternalV;

        public static Color ParseColor(byte[] rawColor, out bool isDefault)
        {
            if (rawColor.Length != 2)
            {
                throw new Exception("ERROR: Expected 2 bytes");
            }
            BitArray bitArr = new BitArray(rawColor);

            int Red = 0, Green = 0, Blue = 0;

            //Red
            if (bitArr[0])
                Red += 16;
            if (bitArr[1])
                Red += 8;
            if (bitArr[2])
                Red += 4;
            if (bitArr[3])
                Red += 2;
            if (bitArr[4])
                Red += 1;
            //Green
            if (bitArr[5])
                Green += 16;
            if (bitArr[6])
                Green += 8;
            if (bitArr[7])
                Green += 4;
            if (bitArr[8])
                Green += 2;
            if (bitArr[9])
                Green += 1;
            //Blue
            if (bitArr[10])
                Blue += 16;
            if (bitArr[11])
                Blue += 8;
            if (bitArr[12])
                Blue += 4;
            if (bitArr[13])
                Blue += 2;
            if (bitArr[14])
                Blue += 1;

            Red = Red * 8;
            Green = Green * 8;
            Blue = Blue * 8;

            isDefault = bitArr[15];

            return Color.FromArgb(Red, Green, Blue);
        }

        /// <summary>
        /// Step 1/5: Breaks the file up into its constituent modules
        /// </summary>
        public void PreProcess()
        {
            using (var Reader = new BinaryReader(new MemoryStream(RawFile)))
            {
                int pos = 0;
                Section Temp = new Section();

                MajorV = Reader.ReadByte(); pos++;
                MinorV = Reader.ReadByte(); pos++;
                PatchV = Reader.ReadByte(); pos++;
                InternalV = Reader.ReadByte(); pos++;

                while (pos < RawFile.Length)
                {
                    Temp.ID = Reader.ReadUInt32(); pos += 4;
                    Temp.Length = Reader.ReadUInt32(); pos += 4;
                    Temp.Data = Reader.ReadBytes((int)Temp.Length); pos += (int)Temp.Length;
                    Sections.Add(Temp);
                    Temp = new Section();
                }
            }
        }

        /// <summary>
        /// Throws an exception if raw.ID is not 1
        /// </summary>
        /// <param name="raw"></param>
        private void PreProcessSTL(Section raw)
        {
            if(raw.ID != 1)
            {
                throw new Exception("ERROR: Invalid Section passed to RobotReader.PreProcessSTL");
            }
            using (var Reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                UInt32 MeshCount = Reader.ReadUInt32();
                RawMesh temp = new RawMesh();

                for (int i = 0; i < MeshCount; i++)
                {
                    temp.MeshID = Reader.ReadUInt32();

                    temp.TransMat[0, 0] = Reader.ReadSingle();
                    temp.TransMat[1, 0] = Reader.ReadSingle();
                    temp.TransMat[2, 0] = Reader.ReadSingle();
                    temp.TransMat[3, 0] = Reader.ReadSingle();
                    temp.TransMat[0, 1] = Reader.ReadSingle();
                    temp.TransMat[1, 1] = Reader.ReadSingle();
                    temp.TransMat[2, 1] = Reader.ReadSingle();
                    temp.TransMat[3, 1] = Reader.ReadSingle();
                    temp.TransMat[0, 2] = Reader.ReadSingle();
                    temp.TransMat[1, 2] = Reader.ReadSingle();
                    temp.TransMat[2, 2] = Reader.ReadSingle();
                    temp.TransMat[3, 2] = Reader.ReadSingle();
                    temp.TransMat[0, 3] = Reader.ReadSingle();
                    temp.TransMat[1, 3] = Reader.ReadSingle();
                    temp.TransMat[2, 3] = Reader.ReadSingle();
                    temp.TransMat[3, 3] = Reader.ReadSingle();

                    Reader.ReadBytes(80);

                    uint FacetCount = Reader.ReadUInt32();
                    temp.FacetCount = FacetCount;

                    temp.Facets = Reader.ReadBytes(((int)FacetCount * 50) + 4);
                    RawMeshes.Add(temp);
                    temp = new RawMesh();
                }
            }
        }
        
        /// <summary>
        /// Step 2/5: Breaks up the STL Module(s) into their constituent meshes
        /// </summary>
        public void PreProcessSTL()
        {
            foreach(var sect in Sections)
            {
                if (sect.ID == 1)
                    PreProcessSTL(sect);
            }
        }

        /// <summary>
        /// Step 3/5: Parses the raw mesh data
        /// </summary>
        public void ProcessMeshes()
        {
            foreach(var rawMesh in RawMeshes)
            {
                List<Facet> tempFacets = new List<Facet>();
                Color TempColor = new Color(); bool TempIsDefault = false;
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
                            TempColor = ParseColor(Reader.ReadBytes(2), out TempIsDefault);
                            IsFirst = false;
                        }
                        else
                            Reader.ReadBytes(2);
                    }
                }
                LoadedRobot.Meshes.Add(new Mesh(rawMesh.MeshID, tempFacets.ToArray(), TempColor, TempIsDefault, uint.MaxValue, rawMesh.TransMat));

            }
        }



        /// <summary>
        /// Throws an exception if raw.ID is not 2
        /// </summary>
        /// <param name="raw"></param>
        private void ProcessAttributes(Section raw)
        {
            if (raw.ID != 2)
            {
                throw new Exception("ERROR: Invalid Section passed to PreProcessAttribs");
            }
            using (var Reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                uint AttribCount = Reader.ReadUInt32();

                for (uint i = 0; i < AttribCount; i++)
                {
                    uint TempAttID = Reader.ReadUInt32();
                    AttribType TempType = (AttribType)Reader.ReadUInt16();
                    bool TempIsDynamic;

                    switch (TempType)
                    {
                        case AttribType.BOX_COLLIDER:
                            float TempX = Reader.ReadSingle(), TempY = Reader.ReadSingle(), TempZ = Reader.ReadSingle();
                            float TempFriction = Reader.ReadSingle();
                            TempIsDynamic = Reader.ReadBoolean();
                            if (TempIsDynamic)
                            {
                                LoadedRobot.Attributes.Add(new Properties.Attribute(TempType, TempAttID, TempFriction, TempIsDynamic, Reader.ReadSingle(), TempX, TempY, TempZ, null));
                            }
                            else
                            {
                                LoadedRobot.Attributes.Add(new Properties.Attribute(TempType, TempAttID, TempFriction, TempIsDynamic, null, TempX, TempY, TempZ, null));

                            }
                            break;
                        case AttribType.SPHERE_COLLIDER:
                            float TempG = Reader.ReadSingle();
                            float TempFric = Reader.ReadSingle();
                            TempIsDynamic = Reader.ReadBoolean();
                            if (TempIsDynamic)
                            {
                                LoadedRobot.Attributes.Add(new Properties.Attribute(TempType, TempAttID, TempFric, TempIsDynamic, Reader.ReadSingle(), null, null, null, TempG));
                            }
                            else
                            {
                                LoadedRobot.Attributes.Add(new Properties.Attribute(TempType, TempAttID, TempFric, TempIsDynamic, null, null, null, null, TempG));
                            }

                            break;
                        case AttribType.MESH_COLLIDER:
                            float TempF = Reader.ReadSingle();
                            TempIsDynamic = Reader.ReadBoolean();
                            if (TempIsDynamic)
                            {
                                LoadedRobot.Attributes.Add(new Properties.Attribute(TempType, TempAttID, TempF, TempIsDynamic, Reader.ReadSingle(), null, null, null, null));
                            }
                            else
                            {
                                LoadedRobot.Attributes.Add(new Properties.Attribute(TempType, TempAttID, TempF, TempIsDynamic, null, null, null, null, null));
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
                if (sect.ID == 2)
                    ProcessAttributes(sect);
            }
        }

        /// <summary>
        /// Throws an exception if raw.ID is not 3
        /// </summary>
        /// <param name="raw"></param>
        private void ProcessJoints(Section raw)
        {
            if (raw.ID != 3)
            {
                throw new Exception("ERROR: Invalid Section passed to ProcessJoints");
            }

            using (var Reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                uint JointCount = Reader.ReadUInt32();

                for (uint i = 0; i < JointCount; i++)
                {
                    byte[] GenericData = Reader.ReadBytes(46);

                    uint TempJointID = Reader.ReadUInt32();

                    JAType Type = (JAType)Reader.ReadUInt16();
                    bool HasLimits;
                    switch (Type)
                    {
                        default:
                            LoadedRobot.Joints.Add(new Joint(GenericData, new NoDriver()));
                            break;
                        case JAType.MOTOR:
                            bool PortType = Reader.ReadBoolean();
                            float Port = Reader.ReadSingle();

                            HasLimits = Reader.ReadBoolean();

                            if (HasLimits)
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new Motor(PortType, Port, HasLimits, (Friction)Reader.ReadUInt16(),
                                    Reader.ReadBoolean(), (Wheel)Reader.ReadUInt16(), Reader.ReadUInt16(), Reader.ReadUInt16())));
                            }
                            else
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new Motor(PortType, Port, HasLimits, Friction.NO_LIMITS,
                                    Reader.ReadBoolean(), (Wheel)Reader.ReadUInt16(), Reader.ReadUInt16(), Reader.ReadUInt16())));
                            }
                            break;
                        case JAType.SERVO:
                            float CANPort = Reader.ReadSingle();
                            HasLimits = Reader.ReadBoolean();

                            if (HasLimits)
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new Servo(CANPort, HasLimits, (Friction)Reader.ReadUInt16())));
                            }
                            else
                                LoadedRobot.Joints.Add(new Joint(GenericData, new Servo(CANPort, HasLimits, Friction.NO_LIMITS)));
                            break;
                        case JAType.BUMPER_PNUEMATIC:
                            float Port1 = Reader.ReadSingle();
                            float Port2 = Reader.ReadSingle();
                            HasLimits = Reader.ReadBoolean();

                            if (HasLimits)
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new BumperPnuematic(Port1, Port2, HasLimits, (Friction)Reader.ReadUInt16(),
                                    (InternalDiameter)Reader.ReadUInt16(), (Pressure)Reader.ReadUInt16())));
                            }
                            else
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new BumperPnuematic(Port1, Port2, HasLimits, Friction.NO_LIMITS,
                                    (InternalDiameter)Reader.ReadUInt16(), (Pressure)Reader.ReadUInt16())));
                            }
                            break;
                        case JAType.RELAY_PNUEMATIC:
                            float relayPort = Reader.ReadSingle();
                            HasLimits = Reader.ReadBoolean();
                            if (HasLimits)
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new RelayPnuematic(relayPort, HasLimits, (Friction)Reader.ReadUInt16(),
                                    (InternalDiameter)Reader.ReadUInt16(), (Pressure)Reader.ReadUInt16())));
                            }
                            else
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new RelayPnuematic(relayPort, HasLimits, Friction.NO_LIMITS,
                                    (InternalDiameter)Reader.ReadUInt16(), (Pressure)Reader.ReadUInt16())));
                            }

                            break;
                        case JAType.WORM_SCREW:
                            PortType = Reader.ReadBoolean();
                            Port = Reader.ReadSingle();
                            HasLimits = Reader.ReadBoolean();
                            if (HasLimits)
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new WormScrew(PortType, Port, HasLimits, (Friction)Reader.ReadUInt16())));
                            }
                            else
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new WormScrew(PortType, Port, HasLimits, Friction.NO_LIMITS)));
                            }
                            break;
                        case JAType.DUAL_MOTOR:
                            PortType = Reader.ReadBoolean();
                            Port1 = Reader.ReadSingle();
                            Port2 = Reader.ReadSingle();

                            HasLimits = Reader.ReadBoolean();

                            if (HasLimits)
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new DualMotor(PortType, Port1, Port2, HasLimits, (Friction)Reader.ReadUInt16(),
                                    Reader.ReadBoolean(), (Wheel)Reader.ReadUInt16(), Reader.ReadUInt16(), Reader.ReadUInt16())));
                            }
                            else
                            {
                                LoadedRobot.Joints.Add(new Joint(GenericData, new DualMotor(PortType, Port1, Port2, HasLimits, Friction.NO_LIMITS,
                                    Reader.ReadBoolean(), (Wheel)Reader.ReadUInt16(), Reader.ReadUInt16(), Reader.ReadUInt16())));
                            }
                            break;
                        case JAType.ELEVATOR:
                            PortType = Reader.ReadBoolean();
                            Port = Reader.ReadSingle();
                            HasLimits = Reader.ReadBoolean();
                            Friction frict;
                            if (HasLimits)
                                frict = (Friction)Reader.ReadUInt16();
                            else
                                frict = Friction.NO_LIMITS;
                            bool HasBrakes = Reader.ReadBoolean();
                            float BrakePort1, BrakePort2;
                            if (HasBrakes)
                            {
                                BrakePort1 = Reader.ReadSingle();
                                BrakePort2 = Reader.ReadSingle();
                            }
                            else
                            {
                                BrakePort1 = float.MaxValue;
                                BrakePort2 = float.MaxValue;
                            }
                            Stages stages = (Stages)Reader.ReadUInt16();
                            float GearIn = Reader.ReadSingle();
                            float GearOut = Reader.ReadSingle();

                            LoadedRobot.Joints.Add(new Joint(GenericData, new Elevator(PortType, Port, HasLimits, frict, HasBrakes, BrakePort1, BrakePort2, stages, GearIn, GearOut)));
                            break;
                    }

                }
            }
        }

        /// <summary>
        /// Step 5/5: Parses the raw Joint and Joint Attribute data.
        /// </summary>
        public void ProcessJoints()
        {
            foreach (var sect in Sections)
            {
                if (sect.ID == 3)
                    ProcessJoints(sect);
            }
        }

        /// <summary>
        /// Step 1.5/5: Parses thumbnail image (not technically needed to load the robot).
        /// </summary>
        public void ProcessImage()
        {
            foreach (var sect in Sections)
            {
                if (sect.ID == 0)
                {
                    LoadedRobot.Thumbnail = new Bitmap(new MemoryStream(sect.Data));
                }
            }
        }

        /// <summary>
        /// Does all of the reading steps in order. However, you can't really do a loading bar if you do it this way. Seeing as each file will be around 125MB, I expect the time it takes to load will be at least 5 seconds.
        /// </summary>
        public void ReadRobot()
        {
            PreProcess();
            ProcessImage();
            PreProcessSTL();
            ProcessMeshes();
            ProcessAttributes();
            ProcessJoints();
        }

        /// <summary>
        /// Reads the file into a byte array and disposes the stream.
        /// </summary>
        /// <param name="path"></param>
        public RobotReader(string path)
        {
            using (var Reader = new BinaryReader(File.Open(path, FileMode.Open), Encoding.Default))
            {
                RawFile = Reader.ReadBytes(int.MaxValue);
            }
        }
    }
}