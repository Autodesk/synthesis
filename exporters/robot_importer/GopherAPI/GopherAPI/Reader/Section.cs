using System;
using GopherAPI.Other;

namespace GopherAPI.Reader
{
    public enum SectionType { IMAGE, STL, STL_ATTRIBUTE, JOINT, JOINT_ATTRIBUTE }
    public struct Section
    {
        public SectionType ID;
        public UInt32 Length;
        public byte[] Data;
        public bool IsEmpty;
    }

    public struct RawMesh
    {
        public UInt32 MeshID;
        public UInt32 FacetCount;
        public byte[] Facets;
    }
}
