using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GopherAPI;
using GopherAPI.STL;
using GopherAPI.Nodes;
using GopherAPI.Nodes.Joint;
using GopherAPI.Nodes.Colliders;
using GopherAPI.Nodes.Joint.Driver;

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
                while (true)
                {
                    reader.ReadBytes(80);
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
        /// Step 2: Processes all of the non facet information into RawMesh structs
        /// </summary>
        /// <param name="raw">Section to be passed</param>
        internal void PreProcessSTL(Section raw)
        {
            if (raw.ID != SectionType.STL)
                throw new ArgumentException("Non-STL section passed to PreProcessSTL", "raw");
            using (var reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                
            }
        }


    }
}