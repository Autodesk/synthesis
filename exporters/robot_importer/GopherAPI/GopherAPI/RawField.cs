using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GopherAPI.STL;
using GopherAPI.Nodes.Joint;
using GopherAPI.Nodes.Colliders;

namespace GopherAPI
{
    /// <summary>
    /// The class that serves as the middleman between the file and the GopherField
    /// </summary>
    internal class RawField
    {
        private List<STLMesh> meshes = new List<STLMesh>();
        private List<GopherCollider_Base> colliders = new List<GopherCollider_Base>();
        private List<GopherJoint_Base> joints = new List<GopherJoint_Base>();

        /// <summary>
        /// IMPORTANT: MeshID might not be the same as the index in the list. Use the GetMesh method.
        /// </summary>
        internal List<STLMesh> Meshes { get => meshes; set => meshes = value; }
        /// <summary>
        /// IMPORTANT: MeshID might not be the same as the index in the list. Use the GetAttribute method.
        /// </summary>
        internal List<GopherCollider_Base> Colliders { get => colliders; set => colliders = value; }
        /// <summary>
        /// IMPORTANT: JointID might not be the same as the index in the list. Use the GetJoint method.
        /// </summary>
        internal List<GopherJoint_Base> Joints { get => joints; set => joints = value; }

        /// <summary>
        /// Searches meshes for a mesh with the matching ID
        /// </summary>
        /// <param name="MeshID"></param>
        /// <returns></returns>
        internal STLMesh GetMesh(UInt32 MeshID)
        {
            foreach (var mesh in meshes)
            {
                if (mesh.MeshID == MeshID)
                    return mesh;
            }
            throw new Exception("ERROR: STLMesh not found");
        }

        /// <summary>
        /// Searches attributes for an colliders with the matching ID
        /// </summary>
        /// <param name="ColliderID"></param>
        /// <returns></returns>
        internal GopherCollider_Base GetCollider(UInt32 ColliderID)
        {
            foreach (var collider in colliders)
            {
                if (collider.Meta.ColliderID == ColliderID)
                    return collider;
            }
            throw new Exception("ERROR: Collider not found");
        }

        /// <summary>
        /// Searches joints for a joint with the matching ID
        /// </summary>
        /// <param name="JointID"></param>
        /// <returns></returns>
        internal GopherJoint_Base GetJoint(UInt32 JointID)
        {
            foreach (var joint in joints)
            {
                if (joint.Meta.JointID == JointID)
                    return joint;
            }
            throw new Exception("ERROR: Joint not found");
        }

        /// <summary>
        /// Gets all the joints which the given mesh is a parent of
        /// </summary>
        /// <param name="MeshID"></param>
        /// <returns>A list of all joints the given mesh is a parent of</returns>
        internal List<GopherJoint_Base> GetChildJoints(UInt32 MeshID)
        {
            List<GopherJoint_Base> ret = new List<GopherJoint_Base>();

            foreach(var joint in joints)
            {
                if (joint.Meta.ParentID == MeshID)
                    ret.Add(joint);
            }
            return ret;
        }

        /// <summary>
        /// Gets whether or not a given mesh is a root node
        /// </summary>
        /// <param name="MeshID"></param>
        /// <returns></returns>
        internal bool GetIsRoot(UInt32 MeshID)
        {
            foreach(var joint in joints)
            {
                if (joint.Meta.ChildID == MeshID)
                    return false;
            }
            return true;
        }
    }
}
