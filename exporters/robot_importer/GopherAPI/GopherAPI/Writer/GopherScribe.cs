using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GopherAPI.Reader;
using System.Drawing;
using System.Collections;
using GopherAPI.Properties;
using GopherAPI.STL;

namespace GopherAPI.Writer
{
    public static class GopherScribe
    {
        private static byte[] STLH = new byte[] 
        {
            0x53, 0x54, 0x4c, 0x42, 0x20, 0x41, 0x54, 0x46,
            0x20, 0x32, 0x2e, 0x30, 0x2e, 0x30, 0x2e, 0x39,
            0x30, 0x30, 0x30, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        };
        private static byte[] FILEH = new byte[]
        {
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        };
        private static byte[] BitToByte(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
        private static bool[] IntToBits(int n)
        {
            var ret = new bool[5];

            if (n >= 32)
            {
                throw new ArgumentOutOfRangeException("n", "ERROR: Converted Color Value must be between 0 and 31");
            }

            if (n >= 16)
            {
                ret[0] = true;
                n -= 16;
            }
            if (n >= 8)
            {
                ret[1] = true;
                n -= 8;
            }
            if (n >= 4)
            {
                ret[2] = true;
                n -= 4;
            }
            if (n >= 2)
            {
                ret[3] = true;
                n -= 2;
            }
            if (n >= 1)
            {
                ret[4] = true;
                n -= 1;
            }
            return ret;
        }

        /// <summary>
        /// Converts a System.Drawing.Color into a 2-bit STL color
        /// </summary>
        /// <param name="ARGB"></param>
        /// <param name="IsDefault"></param>
        /// <returns></returns>
        public static byte[] ConvertColor(Color ARGB, bool IsDefault)
        {
            BitArray BitArr = new BitArray(16);
            int R = ARGB.R / 8, G = ARGB.G / 8, B = ARGB.B / 8;

            var index = 0;
            foreach (var b in IntToBits(R))
            {
                BitArr.Set(index, b);
                index++;
            }
            foreach (var b in IntToBits(G))
            {
                BitArr.Set(index, b);
                index++;
            }
            foreach (var b in IntToBits(B))
            {
                BitArr.Set(index, b);
                index++;
            }
            BitArr.Set(15, IsDefault);

            return BitToByte(BitArr);
        }

        #region PreWrite
        /// <summary>
        /// Converts thumbnail into a Section
        /// </summary>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
        public static Section PreWrite(Bitmap thumbnail)
        {
            if (thumbnail == null)
                return new Section { IsEmpty = true };
            ImageConverter converter = new ImageConverter();
            byte[] data = (byte[])converter.ConvertTo(thumbnail, typeof(byte[]));
            return new Section
            {
                ID = SectionType.IMAGE,
                Length = (uint)data.Length,
                Data = data
            };
        }

        /// <summary>
        /// Converts an array of Meshes into a Section
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns></returns>
        public static Section PreWrite(STLMesh[] meshes)
        {
            if (meshes.Length == 0)
                return new Section { IsEmpty = true };
            MemoryStream stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((uint)meshes.Length); //number of meshes
                foreach (var mesh in meshes)
                {
                    writer.Write(mesh.TransMat.Binary);
                    writer.Write(STLH);
                    writer.Write(mesh.Facets.Length);
                    foreach(var facet in mesh.Facets)
                    {
                        writer.Write(facet.Binary);
                        writer.Write(ConvertColor(mesh.PartColor, mesh.IsDefault));
                    }
                    writer.Write(mesh.AttributeID);
                }
                return new Section
                {
                    ID = SectionType.STL,
                    Length = (uint)stream.Length,
                    Data = stream.ToArray()
                };
            }

        }

        /// <summary>
        /// Converts a List of Meshes into a Section
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns></returns>
        public static Section PreWrite(List<STLMesh> meshes)
        {
            return PreWrite(meshes.ToArray());
        }

        /// <summary>
        /// Converts a STLAttribute array into a Section
        /// </summary>
        /// <param name="attrib"></param>
        /// <returns></returns>
        public static Section PreWrite(STLAttribute[] attributes)
        {
            if (attributes.Length == 0)
                return new Section { IsEmpty = true };
            MemoryStream stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var attrib in attributes)
                {
                    switch (attrib.Type)
                    {
                        case AttribType.BOX_COLLIDER:
                            writer.Write(attrib.AttributeID);
                            writer.Write((ushort)attrib.Type);
                            writer.Write(attrib.XScale);
                            writer.Write(attrib.YScale);
                            writer.Write(attrib.ZScale);
                            writer.Write(attrib.Friction);
                            if (attrib.IsDynamic)
                            {
                                writer.Write(true);
                                writer.Write(attrib.Mass);
                            }
                            else
                                writer.Write(false);
                            break;
                        case AttribType.SPHERE_COLLIDER:
                            writer.Write(attrib.AttributeID);
                            writer.Write((ushort)attrib.Type);
                            writer.Write(attrib.GScale);
                            writer.Write(attrib.Friction);
                            if (attrib.IsDynamic)
                            {
                                writer.Write(true);
                                writer.Write(attrib.Mass);
                            }
                            else
                                writer.Write(false);
                            break;
                        case AttribType.MESH_COLLIDER:
                            writer.Write(attrib.AttributeID);
                            writer.Write((ushort)attrib.Type);
                            writer.Write(attrib.Friction);
                            if (attrib.IsDynamic)
                            {
                                writer.Write(true);
                                writer.Write(attrib.Mass);
                            }
                            else
                                writer.Write(false);
                            break;
                    }
                }
                return new Section
                {
                    ID = SectionType.STL_ATTRIBUTE,
                    Length = (uint)stream.Length,
                    Data = stream.ToArray()
                };
            }
        }

        /// <summary>
        /// Converts a STLAttribute List into a Section
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static Section PreWrite(List<STLAttribute> attributes)
        {
            return PreWrite(attributes);
        }

        /// <summary>
        /// Converts a Joint array into a Section
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        public static Section PreWrite(Joint[] joints)
        {
            if (joints.Length == 0)
                return new Section { IsEmpty = true };
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(joints.Length);
                foreach (var joint in joints)
                {
                    writer.Write(joint.JointID);
                    writer.Write((ushort)joint.Type);
                    writer.Write(joint.ParentID);
                    writer.Write(joint.ChildID);
                    writer.Write(joint.DefVector[0]);
                    writer.Write(joint.DefVector[1]);
                    writer.Write(joint.DefVector[2]);
                    writer.Write(joint.RelativePoint[0]);
                    writer.Write(joint.RelativePoint[1]);
                    writer.Write(joint.RelativePoint[2]);
                    writer.Write(joint.ProFreedomFactor);
                    writer.Write(joint.RetroFreedomFactor);
                }
                return new Section
                {
                    ID = SectionType.JOINT,
                    Length = (uint)stream.Length,
                    Data = stream.ToArray()
                };
            }

        }

        /// <summary>
        /// Converts a Joint List into a Section
        /// </summary>
        /// <param name="joints"></param>
        /// <returns></returns>
        public static Section PreWrite(List<Joint> joints)
        {
            return PreWrite(joints.ToArray());
        }

        /// <summary>
        /// Converts an array of JointAttributes into a Section
        /// </summary>
        /// <param name="jointAttributes"></param>
        /// <returns></returns>
        public static Section PreWrite(IJointAttribute[] jointAttributes)
        {
            if (jointAttributes.Length == 0)
                return new Section { IsEmpty = true };
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(jointAttributes.Length);
                foreach (var ja in jointAttributes)
                {
                    switch (ja.GetJAType())
                    {
                        case JAType.NONE:
                            var ND = (NoDriver)ja;
                            writer.Write(ND.JointID);
                            break;
                        case JAType.MOTOR:
                            var M = (Motor)ja;
                            writer.Write(M.JointID);
                            writer.Write(M.IsCAN);
                            writer.Write(M.MotorPort);
                            if (M.HasLimits)
                            {
                                writer.Write(true);
                                writer.Write((ushort)M.Friction);
                            }
                            else
                                writer.Write(false);
                            if (M.IsDriveWheel)
                            {
                                writer.Write(true);
                                writer.Write((ushort)M.Friction);
                            }
                            else
                                writer.Write(false);
                            writer.Write(M.InputGear);
                            writer.Write(M.OutputGear);
                            break;
                        case JAType.SERVO:
                            var S = (Servo)ja;
                            writer.Write(S.JointID);
                            writer.Write(S.MotorPort);
                            if (S.HasLimits)
                            {
                                writer.Write(true);
                                writer.Write((ushort)S.Friction);
                            }
                            else
                                writer.Write(false);
                            break;
                        case JAType.BUMPER_PNUEMATIC:
                            var BP = (BumperPnuematic)ja;
                            writer.Write(BP.JointID);
                            writer.Write(BP.SolenoidPortOne);
                            writer.Write(BP.SolenoidPortTwo);
                            if (BP.HasLimits)
                            {
                                writer.Write(true);
                                writer.Write((ushort)BP.Friction);
                            }
                            else
                                writer.Write(false);
                            writer.Write((ushort)BP.InternalDiameter);
                            writer.Write((ushort)BP.Pressure);
                            break;
                        case JAType.RELAY_PNUEMATIC:
                            var RP = (RelayPnuematic)ja;
                            writer.Write(RP.JointID);
                            if (RP.HasLimits)
                            {
                                writer.Write(true);
                                writer.Write((ushort)RP.Friction);
                            }
                            else
                                writer.Write(false);
                            writer.Write((ushort)RP.InternalDiameter);
                            writer.Write((ushort)RP.Pressure);
                            break;
                        case JAType.WORM_SCREW:
                            var WS = (WormScrew)ja;
                            writer.Write(WS.JointID);
                            writer.Write(WS.IsCAN);
                            if (WS.HasLimits)
                            {
                                writer.Write(true);
                                writer.Write((ushort)WS.Friction);
                            }
                            else
                                writer.Write(false);
                            break;
                        case JAType.DUAL_MOTOR:
                            var DM = (DualMotor)ja;
                            writer.Write(DM.JointID);
                            writer.Write(DM.IsCAN);
                            writer.Write(DM.PortOne);
                            writer.Write(DM.PortTwo);
                            if (DM.HasLimits)
                            {
                                writer.Write(true);
                                writer.Write((ushort)DM.Friction);
                            }
                            else
                                writer.Write(false);
                            if (DM.IsDriveWheel)
                            {
                                writer.Write(true);
                                writer.Write((ushort)DM.Friction);
                            }
                            else
                                writer.Write(false);
                            writer.Write(DM.InputGear);
                            writer.Write(DM.OutputGear);
                            break;
                        case JAType.ELEVATOR:
                            var E = (Elevator)ja;
                            writer.Write(E.JointID);
                            writer.Write(E.IsCAN);
                            if (E.HasLimits)
                            {
                                writer.Write(true);
                                writer.Write((ushort)E.Friction);
                            }
                            else
                                writer.Write(false);
                            if (E.HasBrake)
                            {
                                writer.Write(true);
                                writer.Write(E.BrakePortOne);
                                writer.Write(E.BrakePortTwo);
                            }
                            writer.Write((ushort)E.Stages);
                            writer.Write(E.InputGear);
                            writer.Write(E.OutputGear);
                            break;
                    }
                }
                return new Section
                {
                    ID = SectionType.JOINT_ATTRIBUTE,
                    Length = (uint)stream.Length,
                    Data = stream.ToArray()
                };
            }
        }

        /// <summary>
        /// Converts a List of JointAttributes into a Section
        /// </summary>
        /// <param name="jointAttributes"></param>
        /// <returns></returns>
        public static Section PreWrite(List<IJointAttribute> jointAttributes)
        {
            return PreWrite(jointAttributes.ToArray());
        }
        #endregion

        ///// <summary>
        ///// Writes one section to a specified stream
        ///// </summary>
        ///// <param name="section"></param>
        ///// <param name="stream"></param>
        //public static void WriteSection(Section section, Stream stream)
        //{
        //    using (BinaryWriter writer = new BinaryWriter(stream))
        //    {
        //        writer.Write((ushort)section.ID);
        //        writer.Write(section.Length);
        //        writer.Write(section.Data);
        //    }
        //}

        /// <summary>
        /// Writes a field to a specified stream
        /// </summary>
        /// <param name="field"></param>
        /// <param name="stream"></param>
        public static void WriteField(Field field, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(0x01);
                writer.Write(0x00);
                writer.Write(0x00);
                writer.Write(0x00);

                writer.Write(FILEH);
                Section MeshSection = PreWrite(field.Meshes.ToArray());
                Section AttribSection = PreWrite(field.Attributes.ToArray());
                Section ImageSection = PreWrite(field.Thumbnail);
                Section JointSection = PreWrite(field.Joints.ToArray());
                Section JointAttribSection = PreWrite(field.JointAttributes.ToArray());

                //Write Meshes
                if (!MeshSection.IsEmpty)
                {
                    writer.Write((uint)MeshSection.ID);
                    writer.Write(MeshSection.Length);
                    writer.Write(MeshSection.Data);
                }

                //Write Attributes
                if (!MeshSection.IsEmpty)
                {
                    writer.Write((uint)AttribSection.ID);
                    writer.Write(AttribSection.Length);
                    writer.Write(AttribSection.Data);

                }

                //Write Image
                if (!ImageSection.IsEmpty)
                {
                    writer.Write((uint)ImageSection.ID);
                    writer.Write(ImageSection.Length);
                    writer.Write(ImageSection.Data);
                }

                //Write Joints
                if (!JointSection.IsEmpty)
                {
                    writer.Write((uint)JointSection.ID);
                    writer.Write(JointSection.Length);
                    writer.Write(JointSection.Data);
                }

                //Write JointAttributes
                if (!JointAttribSection.IsEmpty)
                {
                    writer.Write((uint)JointAttribSection.ID);
                    writer.Write(JointAttribSection.Length);
                    writer.Write(JointAttribSection.Data);
                }
            }
        }

        /// <summary>
        /// Writes a Robot to a specified stream
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="stream"></param>
        public static void WriteRobot(Robot robot, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(0x01);
                writer.Write(0x00);
                writer.Write(0x00);
                writer.Write(0x00);

                writer.Write(FILEH);
                Section MeshSection = PreWrite(robot.Meshes.ToArray());
                Section AttribSection = PreWrite(robot.Attributes.ToArray());
                Section ImageSection = PreWrite(robot.Thumbnail);
                Section JointSection = PreWrite(robot.Joints.ToArray());
                Section JointAttribSection = PreWrite(robot.JointAttributes.ToArray());

                //Write Meshes
                if (!MeshSection.IsEmpty)
                {
                    writer.Write((uint)MeshSection.ID);
                    writer.Write(MeshSection.Length);
                    writer.Write(MeshSection.Data);
                }

                //Write Attributes
                if (!MeshSection.IsEmpty)
                {
                    writer.Write((uint)AttribSection.ID);
                    writer.Write(AttribSection.Length);
                    writer.Write(AttribSection.Data);

                }

                //Write Image
                if (!ImageSection.IsEmpty)
                {
                    writer.Write((uint)ImageSection.ID);
                    writer.Write(ImageSection.Length);
                    writer.Write(ImageSection.Data);
                }

                //Write Joints
                if (!JointSection.IsEmpty)
                {
                    writer.Write((uint)JointSection.ID);
                    writer.Write(JointSection.Length);
                    writer.Write(JointSection.Data);
                }

                //Write JointAttributes
                if (!JointAttribSection.IsEmpty)
                {
                    writer.Write((uint)JointAttribSection.ID);
                    writer.Write(JointAttribSection.Length);
                    writer.Write(JointAttribSection.Data);
                }
            }
        }
    }
}
