using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts.FEA
{
    public static class ReplayExporter
    {
        /// <summary>
        /// Saves a replay from the given file name, field path, robot path, Trackers, and contacts.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fieldPath"></param>
        /// <param name="robotPath"></param>
        /// <param name="trackers"></param>
        /// <param name="contacts"></param>
        public static void Write(string fileName, string fieldPath, string robotPath, List<Tracker> trackers, List<List<ContactDescriptor>> contacts)
        {
            using (XmlWriter writer = XmlWriter.Create(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Synthesis\\Replays\\" + fileName + ".replay",
                new XmlWriterSettings()))
            {
                writer.WriteStartElement("replay");
                writer.WriteAttributeString("length", Tracker.Length.ToString());

                List<Tracker> robotTrackers = trackers.Where(x => x.transform.parent.name.Equals("Robot")).ToList();
                List<Tracker> fieldTrackers = trackers.Except(robotTrackers).ToList();

                WriteField(writer, fieldPath, fieldTrackers);
                WriteRobot(writer, robotPath, robotTrackers);
                WriteContacts(writer, contacts);

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes a field with the given XmlWriter and Trackers.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="fieldPath"></param>
        /// <param name="trackers"></param>
        private static void WriteField(XmlWriter writer, string fieldPath, List<Tracker> trackers)
        {
            writer.WriteStartElement("field");
            writer.WriteAttributeString("path", fieldPath);

            WriteTrackers(writer, trackers);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a robot with the given XmlWriter and Trackers.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="robotPath"></param>
        /// <param name="trackers"></param>
        private static void WriteRobot(XmlWriter writer, string robotPath, List<Tracker> trackers)
        {
            writer.WriteStartElement("robot");
            writer.WriteAttributeString("path", robotPath);

            WriteTrackers(writer, trackers);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the Trackers using the provided XmlWriter.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="trackers"></param>
        private static void WriteTrackers(XmlWriter writer, List<Tracker> trackers)
        {
            IFormatter formattter = new BinaryFormatter();

            using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
            {
                foreach (Tracker t in trackers)
                    foreach (StateDescriptor s in t.States)
                        formattter.Serialize(bw.BaseStream, s);

                WriteMemory(writer, (MemoryStream)bw.BaseStream);
            }
        }

        /// <summary>
        /// Writes the list of contacts with the given XmlWriter.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="contacts"></param>
        private static void WriteContacts(XmlWriter writer, List<List<ContactDescriptor>> contacts)
        {
            writer.WriteStartElement("contacts");

            List<List<ContactDescriptor>> filteredContacts = contacts.Where(x => x != null && x.Count > 0).ToList();

            IFormatter formatter = new BinaryFormatter();

            using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(filteredContacts.Count);

                int i = 0;

                foreach (List<ContactDescriptor> l in filteredContacts)
                {
                    bw.Write(l.Count);

                    int j = 0;

                    foreach (ContactDescriptor c in l)
                    {
                        bw.Write(c.AppliedImpulse);
                        formatter.Serialize(bw.BaseStream, c.Position);

                        string name = c.RobotBody.name;
                        int startIndex = name.IndexOf('_');

                        bw.Write(int.Parse(name.Substring(startIndex + 1, name.Length - startIndex - name.IndexOf('.'))));

                        j++;
                    }

                    i++;
                }

                WriteMemory(writer, (MemoryStream)bw.BaseStream);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the given MemoryStream with the provided XmlWriter with compression and Base64 encoding.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="ms"></param>
        private static void WriteMemory(XmlWriter writer, MemoryStream ms)
        {
            byte[] uncompressedBuffer = ms.ToArray();

            MemoryStream result = new MemoryStream();

            using (DeflateStream ds = new DeflateStream(result, CompressionMode.Compress))
                ds.Write(uncompressedBuffer, 0, uncompressedBuffer.Length);

            byte[] compressedBuffer = result.ToArray();

            writer.WriteBase64(compressedBuffer, 0, compressedBuffer.Length);
        }
    }
}
