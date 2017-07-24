using System;
using GopherAPI.Other;

namespace GopherAPI.Reader
{
    internal enum SectionType { IMAGE, STL, STL_ATTRIBUTE, JOINT, JOINT_ATTRIBUTE }
    internal struct Section
    {
        public SectionType ID;
        public byte[] Data;
        public bool IsEmpty;
    }

    internal struct RawMesh
    {
        public UInt32 MeshID;
        public UInt32 FacetCount;
        public TransformationMatrix TransMat;
        public byte[] Facets;
    }
}
