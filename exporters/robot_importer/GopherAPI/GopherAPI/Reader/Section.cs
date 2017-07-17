using System;
using GopherAPI.Other;

namespace GopherAPI.Reader
{
    public struct Section
    {
        public UInt32 ID;
        public UInt32 Length;
        public byte[] Data;
    }

    public struct RawMesh
    {
        public UInt32 MeshID;
        public TransformationMatrix TransMat;
        public UInt32 FacetCount;
        public byte[] Facets;
    }
}
