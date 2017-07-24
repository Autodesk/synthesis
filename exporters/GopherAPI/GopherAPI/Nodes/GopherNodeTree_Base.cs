using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GopherAPI.Nodes.Joint;

namespace GopherAPI.Nodes
{
    /// <summary>
    /// Base class for GopherNodeRobot and GopherNodeField
    /// </summary>
    public class GopherNodeTree_Base
    {
        protected List<GopherNode_Base> rootNodes = new List<GopherNode_Base>();

        private List<GopherJoint_Base> joints = new List<GopherJoint_Base>();

        public List<GopherJoint_Base> Joints => joints;

        public bool IsLeaf;
    }
}
