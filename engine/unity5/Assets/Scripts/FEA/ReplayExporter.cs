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
        public static void Write(string fileName, string fieldPath, List<Tracker> trackers, List<List<ContactDescriptor>> contacts)
        {
            using (XmlWriter writer = XmlWriter.Create(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Synthesis\\Replays\\" + fileName + ".replay",
                new XmlWriterSettings()))
            {
                writer.WriteStartElement("replay");
                writer.WriteAttributeString("length", Tracker.Length.ToString());

                List<Tracker> gamePieceTrackers = trackers.Where(x => x.gameObject.name.EndsWith("(Clone)")).ToList();
                List<Tracker> fieldTrackers = GameObject.Find("Field").GetComponentsInChildren<Tracker>().Except(gamePieceTrackers).ToList();

                WriteField(writer, fieldPath, fieldTrackers);

                foreach (Robot r in UnityEngine.Object.FindObjectsOfType<Robot>())
                    WriteRobot(writer, r.RobotDirectory, r.GetComponentsInChildren<Tracker>().ToList());

                WriteGamePieces(writer, gamePieceTrackers);
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
            int uncompressedLength;
            byte[] trackersBuffer = GetTrackersBuffer(trackers, out uncompressedLength);

            writer.WriteStartElement("field");
            writer.WriteAttributeString("path", fieldPath);
            writer.WriteAttributeString("ulength", uncompressedLength.ToString());
            writer.WriteAttributeString("clength", trackersBuffer.Length.ToString());

            writer.WriteBase64(trackersBuffer, 0, trackersBuffer.Length);

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
            int uncompressedLength;
            byte[] trackersBuffer = GetTrackersBuffer(trackers, out uncompressedLength);

            writer.WriteStartElement("robot");
            writer.WriteAttributeString("path", robotPath);
            writer.WriteAttributeString("ulength", uncompressedLength.ToString());
            writer.WriteAttributeString("clength", trackersBuffer.Length.ToString());

            writer.WriteBase64(trackersBuffer, 0, trackersBuffer.Length);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes game piece information with the given XmlWriter and Trackers.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="trackers"></param>
        private static void WriteGamePieces(XmlWriter writer, List<Tracker> trackers)
        {
            Dictionary<string, List<Tracker>> sortedTrackers = new Dictionary<string, List<Tracker>>();

            foreach (Tracker t in trackers)
            {
                string name = t.gameObject.name.Remove(t.gameObject.name.IndexOf("(Clone)"));

                if (!sortedTrackers.ContainsKey(name))
                    sortedTrackers[name] = new List<Tracker>();

                sortedTrackers[name].Add(t);
            }

            foreach (KeyValuePair<string, List<Tracker>> k in sortedTrackers)
            {
                int uncompressedLength;
                byte[] trackersBuffer = GetTrackersBuffer(k.Value, out uncompressedLength);

                writer.WriteStartElement("gamepiece");
                writer.WriteAttributeString("name", k.Key);
                writer.WriteAttributeString("ulength", uncompressedLength.ToString());
                writer.WriteAttributeString("clength", trackersBuffer.Length.ToString());

                writer.WriteBase64(trackersBuffer, 0, trackersBuffer.Length);

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes the given list of contacts with the provided XmlWriter.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="contacts"></param>
        private static void WriteContacts(XmlWriter writer, List<List<ContactDescriptor>> contacts)
        {
            int uncompressedLength;
            byte[] contactsBuffer = GetContactsBuffer(contacts, out uncompressedLength);

            writer.WriteStartElement("contacts");
            writer.WriteAttributeString("ulength", uncompressedLength.ToString());
            writer.WriteAttributeString("clength", contactsBuffer.Length.ToString());

            writer.WriteBase64(contactsBuffer, 0, contactsBuffer.Length);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a compressed byte buffer from the given List of Trackers.
        /// </summary>
        /// <param name="trackers"></param>
        private static byte[] GetTrackersBuffer(List<Tracker> trackers, out int uncompressedLength)
        {
            IFormatter formattter = new BinaryFormatter();

            using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
            {
                foreach (Tracker t in trackers)
                    foreach (StateDescriptor s in t.States.Reverse())
                        formattter.Serialize(bw.BaseStream, s);

                return CompressMemoryStream((MemoryStream)bw.BaseStream, out uncompressedLength);
            }
        }

        /// <summary>
        /// Returns a compressed byte buffer representing the contacts list.
        /// </summary>
        /// <param name="contacts"></param>
        private static byte[] GetContactsBuffer(List<List<ContactDescriptor>> contacts, out int uncompressedLength)
        {
            IFormatter formatter = new BinaryFormatter();

            using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(contacts.Count);

                foreach (List<ContactDescriptor> l in contacts.AsEnumerable().Reverse())
                {
                    if (l == null || l.Count == 0)
                    {
                        bw.Write(0);
                        continue;
                    }

                    bw.Write(l.Count);

                    foreach (ContactDescriptor c in l)
                    {
                        bw.Write(c.AppliedImpulse);
                        formatter.Serialize(bw.BaseStream, c.Position);
                        bw.Write(c.RobotBody.transform.GetSiblingIndex());
                    }
                }

                return CompressMemoryStream((MemoryStream)bw.BaseStream, out uncompressedLength);
            }
        }

        /// <summary>
        /// Generates a byte buffer from the given MemoryStream with deflation compression.
        /// </summary>
        /// <param name="ms"></param>
        private static byte[] CompressMemoryStream(MemoryStream ms, out int uncompressedLength)
        {
            byte[] uncompressedBuffer = ms.ToArray();

            uncompressedLength = uncompressedBuffer.Length;

            MemoryStream result = new MemoryStream();

            using (DeflateStream ds = new DeflateStream(result, CompressionMode.Compress))
                ds.Write(uncompressedBuffer, 0, uncompressedBuffer.Length);

            return result.ToArray();
        }
    }
}
