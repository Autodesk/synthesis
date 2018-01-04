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
    public static class ReplayImporter
    {
        /// <summary>
        /// Generates replay information from the given file name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fieldPath"></param>
        /// <param name="robotPath"></param>
        /// <param name="fieldStates"></param>
        /// <param name="robotStates"></param>
        /// <param name="contacts"></param>
        public static void Read(string fileName, out string fieldPath,
            out List<FixedQueue<StateDescriptor>> fieldStates, out List<KeyValuePair<string, List<FixedQueue<StateDescriptor>>>> robotStates,
            out Dictionary<string, List<FixedQueue<StateDescriptor>>> gamePieceStates, out List<List<KeyValuePair<ContactDescriptor, int>>> contacts)
        {
            fieldPath = string.Empty;
            fieldStates = new List<FixedQueue<StateDescriptor>>();
            robotStates = new List<KeyValuePair<string, List<FixedQueue<StateDescriptor>>>>();
            gamePieceStates = new Dictionary<string, List<FixedQueue<StateDescriptor>>>();
            contacts = new List<List<KeyValuePair<ContactDescriptor, int>>>();

            using (XmlReader reader = XmlReader.Create(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Synthesis\\Replays\\" + fileName + ".replay"))
            {
                foreach (string name in AllElements(reader))
                {
                    switch (name)
                    {
                        case "field":
                            ReadFieldStates(reader.ReadSubtree(), out fieldStates, out fieldPath);
                            break;
                        case "robot":
                            ReadRobotStates(reader.ReadSubtree(), robotStates);
                            break;
                        case "gamepiece":
                            ReadGamePieces(reader.ReadSubtree(), gamePieceStates);
                            break;
                        case "contacts":
                            ReadContacts(reader.ReadSubtree(), out contacts);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Generates field state information from the given XmlReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fieldStates"></param>
        /// <param name="fieldPath"></param>
        private static void ReadFieldStates(XmlReader reader, out List<FixedQueue<StateDescriptor>> fieldStates, out string fieldPath)
        {
            fieldPath = string.Empty;

            int uncompressedLength = 0;
            byte[] compressedBuffer = null;

            foreach (string name in AllElements(reader))
            {
                switch (name)
                {
                    case "field":
                        fieldPath = reader["path"];
                        uncompressedLength = int.Parse(reader["ulength"]);
                        compressedBuffer = new byte[int.Parse(reader["clength"])];
                        reader.ReadElementContentAsBase64(compressedBuffer, 0, compressedBuffer.Length);
                        break;
                }
            }

            fieldStates = ReadStates(DecompressBuffer(uncompressedLength, compressedBuffer));
        }

        /// <summary>
        /// Generates robot state information from the given XmlReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="robotStates"></param>
        /// <param name="robotPath"></param>
        private static void ReadRobotStates(XmlReader reader, List<KeyValuePair<string, List<FixedQueue<StateDescriptor>>>> robotStates)
        {
            string robotPath = string.Empty;

            int uncompressedLength = 0;
            byte[] compressedBuffer = null;

            foreach (string name in AllElements(reader))
            {
                switch (name)
                {
                    case "robot":
                        robotPath = reader["path"];
                        uncompressedLength = int.Parse(reader["ulength"]);
                        compressedBuffer = new byte[int.Parse(reader["clength"])];
                        reader.ReadElementContentAsBase64(compressedBuffer, 0, compressedBuffer.Length);
                        break;
                }
            }

            robotStates.Add(new KeyValuePair<string, List<FixedQueue<StateDescriptor>>>(robotPath,
                ReadStates(DecompressBuffer(uncompressedLength, compressedBuffer))));
        }

        /// <summary>
        /// Generates game piece state information from the given XmlReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="gamePieceStates"></param>
        private static void ReadGamePieces(XmlReader reader, Dictionary<string, List<FixedQueue<StateDescriptor>>> gamePieceStates)
        {
            string pieceName = string.Empty;

            int uncompressedLength = 0;
            byte[] compressedBuffer = null;

            foreach (string name in AllElements(reader))
            {
                switch (name)
                {
                    case "gamepiece":
                        pieceName = reader["name"];
                        uncompressedLength = int.Parse(reader["ulength"]);
                        compressedBuffer = new byte[int.Parse(reader["clength"])];
                        reader.ReadElementContentAsBase64(compressedBuffer, 0, compressedBuffer.Length);
                        break;
                }
            }

            gamePieceStates[pieceName] = ReadStates(DecompressBuffer(uncompressedLength, compressedBuffer));
        }

        /// <summary>
        /// Generates contact information from the given XmlReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="contacts"></param>
        private static void ReadContacts(XmlReader reader, out List<List<KeyValuePair<ContactDescriptor, int>>> contacts)
        {
            int uncompressedLength = 0;
            byte[] compressedBuffer = null;

            foreach (string name in AllElements(reader))
            {
                switch (name)
                {
                    case "contacts":
                        uncompressedLength = int.Parse(reader["ulength"]);
                        compressedBuffer = new byte[int.Parse(reader["clength"])];
                        reader.ReadElementContentAsBase64(compressedBuffer, 0, compressedBuffer.Length);
                        break;
                }
            }

            contacts = ReadContactDescriptors(DecompressBuffer(uncompressedLength, compressedBuffer));
        }

        /// <summary>
        /// Generates a List of FixedQueues of StateDescriptors from the given byte buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static List<FixedQueue<StateDescriptor>> ReadStates(byte[] buffer)
        {
            IFormatter formatter = new BinaryFormatter();

            List<FixedQueue<StateDescriptor>> states = new List<FixedQueue<StateDescriptor>>();

            int stateSize = System.Runtime.InteropServices.Marshal.SizeOf(new StateDescriptor());

            MemoryStream ms = new MemoryStream(buffer);

            while (ms.Position < ms.Length)
            {
                FixedQueue<StateDescriptor> currentStates = new FixedQueue<StateDescriptor>(Tracker.Length);

                for (int i = 0; i < Tracker.Length; i++)
                    currentStates.Add((StateDescriptor)formatter.Deserialize(ms));

                states.Add(currentStates);
            }

            return states;
        }

        /// <summary>
        /// Generates contact information from the given byte buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static List<List<KeyValuePair<ContactDescriptor, int>>> ReadContactDescriptors(byte[] buffer)
        {
            IFormatter formatter = new BinaryFormatter();

            List<List<KeyValuePair<ContactDescriptor, int>>> contacts = new List<List<KeyValuePair<ContactDescriptor, int>>>();

            BinaryReader reader = new BinaryReader(new MemoryStream(buffer));

            int numContactLists = reader.ReadInt32();

            for (int i = 0; i < numContactLists; i++)
            {
                int contactCount = reader.ReadInt32();

                if (contactCount > 0)
                {
                    List<KeyValuePair<ContactDescriptor, int>> currentContacts = new List<KeyValuePair<ContactDescriptor, int>>();

                    for (int j = 0; j < contactCount; j++)
                    {
                        ContactDescriptor contact = new ContactDescriptor
                        {
                            AppliedImpulse = reader.ReadSingle(),
                            Position = (BulletSharp.Math.Vector3)formatter.Deserialize(reader.BaseStream)
                        };

                        currentContacts.Add(new KeyValuePair<ContactDescriptor, int>(contact, reader.ReadInt32()));
                    }

                    contacts.Add(currentContacts);
                }
                else
                {
                    contacts.Add(null);
                }
            }

            return contacts;
        }

        /// <summary>
        /// Enumerates through all elements of the given XmlReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static IEnumerable<string> AllElements(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement())
                    yield return reader.Name;
            }
        }

        /// <summary>
        /// Returns a decompressed copy of the given compressed buffer.
        /// </summary>
        /// <param name="compressedBuffer"></param>
        /// <returns></returns>
        private static byte[] DecompressBuffer(int uncompressedLength, byte[] compressedBuffer)
        {
            byte[] decompressedBuffer = new byte[uncompressedLength];

            using (MemoryStream compressedStream = new MemoryStream(compressedBuffer))
            using (DeflateStream ds = new DeflateStream(compressedStream, CompressionMode.Decompress))
                ds.Read(decompressedBuffer, 0, decompressedBuffer.Length);

            return decompressedBuffer;
        }
    }
}
