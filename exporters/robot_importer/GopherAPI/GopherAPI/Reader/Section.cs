using System;
using GopherAPI.Other;

namespace GopherAPI.Reader
{
    internal enum SectionType { IMAGE, STL, STL_ATTRIBUTE, JOINT, JOINT_ATTRIBUTE }
    internal struct Section
    {
        internal SectionType ID;
        internal byte[] Data;
        internal bool IsEmpty;
    }

    internal struct RawMesh
    {
        internal UInt32 MeshID;
        internal UInt32 FacetCount;
        internal TransformationMatrix TransMat;
        internal byte[] Facets;
        internal UInt32 AttribID;
    }
}
