using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GopherAPI.STL;
using GopherAPI.Other;

namespace GopherAPI.Reader
{
    public class STLReader
    {
        public STLFile LoadedMesh;
        public STLReader(string path, uint MeshID)
        {
            using (var Reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                Reader.ReadBytes(80);
                System.Drawing.Color tempColor = new System.Drawing.Color(); bool isFirst = true;
                bool IsDefault = false;
                uint FacetCount = Reader.ReadUInt32();
                List<Facet> facets = new List<Facet>();
                for(uint i = 0; i < FacetCount; i++)
                {
                    facets.Add(new Facet(new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle()),
                        new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle()),
                        new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle()),
                        new Vec3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle())));

                    if (isFirst)
                        tempColor = RobotReader.ParseColor(Reader.ReadBytes(2), out IsDefault);
                    else
                        Reader.ReadBytes(2);
                }
                LoadedMesh = new STLFile(facets.ToArray(), tempColor, IsDefault);
            }
        }
        private STLReader() { }
    }
}
