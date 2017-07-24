using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GopherAPI.Other;

namespace GopherAPI.Nodes.Joint
{
    /// <summary>
    /// Metadata for joints containing the IDs that are used in creating the node tree
    /// </summary>
    public class JointMeta
    {
        public readonly UInt32 JointID;

        public readonly UInt32 ParentID;
        public readonly UInt32 ChildID;
    }
}
