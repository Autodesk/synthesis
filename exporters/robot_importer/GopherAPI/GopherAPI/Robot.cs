using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GopherAPI.STL;
using GopherAPI.Properties;

namespace GopherAPI
{
    public class Robot
    {
        public Bitmap Thumbnail;

        private List<STLMesh> meshes = new List<STLMesh>();
        private List<STLAttribute> attributes = new List<STLAttribute>();
        private List<Joint> joints = new List<Joint>();
        private List<IJointAttribute> jointAttribs = new List<IJointAttribute>();
        private Other.Vec3 spawn;

        /// <summary>
        /// IMPORTANT: MeshID might not be the same as the index in the list. Use the GetMesh method.
        /// </summary>
        public List<STLMesh> Meshes { get => meshes; }
        /// <summary>
        /// IMPORTANT: MeshID might not be the same as the index in the list. Use the GetAttribute method.
        /// </summary>
        public List<STLAttribute> Attributes { get => attributes; }
        /// <summary>
        /// IMPORTANT: JointID might not be the same as the index in the list. Use the GetJoint method.
        /// </summary>
        public List<Joint> Joints { get => joints; }

        public List<IJointAttribute> JointAttributes { get => jointAttribs; }

        /// <summary>
        /// Searches meshes for a mesh with the matching ID
        /// </summary>
        /// <param name="MeshID"></param>
        /// <returns></returns>
        public STLMesh GetMesh(UInt32 MeshID)
        {
            foreach (var mesh in meshes)
            {
                if (mesh.MeshID == MeshID)
                    return mesh;
            }
            throw new Exception("ERROR: STLMesh not found");
        }

        /// <summary>
        /// Searches attributes for an attribute with the matching ID
        /// </summary>
        /// <param name="AttributeID"></param>
        /// <returns></returns>
        public STLAttribute GetAttribute(UInt32 AttributeID)
        {
            foreach (var attrib in attributes)
            {
                if (attrib.AttributeID == AttributeID)
                    return attrib;
            }
            throw new Exception("ERROR: STLAttribute not found");
        }

        /// <summary>
        /// Searches attributes for the STLAttributes matching the AttributeID of the stlMesh parameter
        /// </summary>
        /// <param name="stlMesh"></param>
        /// <returns></returns>
        public STLAttribute GetAttribute(STLMesh stlMesh)
        {
            return GetAttribute(stlMesh.AttributeID);
        }

        /// <summary>
        /// Searches joints for a joint with the matching ID
        /// </summary>
        /// <param name="JointID"></param>
        /// <returns></returns>
        public Joint GetJoint(UInt32 JointID)
        {
            foreach (var joint in joints)
            {
                if (joint.JointID == JointID)
                    return joint;
            }
            throw new Exception("ERROR: STLAttribute not found");
        }

        /// <summary>
        /// Not yet implemented in the exporter
        /// </summary>
        public Other.Vec3 Spawn { get => spawn; set => spawn = value; }
    }
}
