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
        //TODO: Figure out how to implement the embedded image here
        public Bitmap Thumbnail;

        private List<Mesh> meshes = new List<Mesh>();
        private List<Properties.Attribute> attributes = new List<Properties.Attribute>();
        private List<Joint> joints = new List<Joint>();
        private Other.Vec3 spawn;

        /// <summary>
        /// IMPORTANT: MeshID might not be the same as the index in the list. Use the GetMesh method.
        /// </summary>
        public List<Mesh> Meshes { get => meshes; }
        /// <summary>
        /// IMPORTANT: MeshID might not be the same as the index in the list. Use the GetAttribute method.
        /// </summary>
        public List<Properties.Attribute> Attributes { get => attributes; }
        /// <summary>
        /// IMPORTANT: JointID might not be the same as the index in the list. Use the GetJoint method.
        /// </summary>
        public List<Joint> Joints { get => joints; }

        /// <summary>
        /// Searches meshes for a mesh with the matching ID
        /// </summary>
        /// <param name="MeshID"></param>
        /// <returns></returns>
        public Mesh GetMesh(UInt32 MeshID)
        {
            foreach (var mesh in meshes)
            {
                if (mesh.MeshID == MeshID)
                    return mesh;
            }
            throw new Exception("ERROR: Mesh not found");
        }

        /// <summary>
        /// Searches attributes for an attribute with the matching ID
        /// </summary>
        /// <param name="AttributeID"></param>
        /// <returns></returns>
        public Properties.Attribute GetAttribute(UInt32 AttributeID)
        {
            foreach (var attrib in attributes)
            {
                if (attrib.AttributeID == AttributeID)
                    return attrib;
            }
            throw new Exception("ERROR: Attribute not found");
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
            throw new Exception("ERROR: Attribute not found");
        }

        public Other.Vec3 Spawn { get => spawn; set => spawn = value; }
    }
}
