using System;
using System.Collections.Generic;
using GopherAPI.STL;
using GopherAPI.Nodes.Joint.Driver;
using GopherAPI.Nodes.Joint;

namespace GopherAPI
{
    /// <summary>
    /// The class that serves as the middleman between the file and the GopherRobot
    /// </summary>
    internal class RawRobot
    {
        private List<STLMesh> meshes = new List<STLMesh>();
        private List<GopherJoint_Base> joints = new List<GopherJoint_Base>();
        private List<GopherDriver_Base> drivers = new List<GopherDriver_Base>();

        /// <summary>
        /// IMPORTANT: MeshID might not be the same as the index in the list. Use the GetMesh method.
        /// </summary>
        internal List<STLMesh> Meshes => meshes;
        /// <summary>
        /// IMPORTANT: JointID might not be the same as the index in the list. Use the GetJoint method.
        /// </summary>
        internal List<GopherJoint_Base> Joints => joints;
        ///// <summary>
        ///// IMPORTANT: JointID might not be the same as the index in the list. Use the GetDriver method.
        ///// </summary>
        //internal List<GopherDriver_Base> Drivers => drivers;

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
        /// Searches drivers for a driver with the matching JointID
        /// </summary>
        /// <param name="JointID"></param>
        /// <returns></returns>
        internal GopherDriver_Base GetDriver(UInt32 JointID)
        {
            foreach(var driver in drivers)
            {
                if (driver.Meta.JointID == JointID)
                    return driver;
            }
            return new NoDriver();
        }

        /// <summary>
        /// Gets all the joints which the given mesh is a parent of
        /// </summary>
        /// <param name="MeshID"></param>
        /// <returns>A list of all joints the given mesh is a parent of</returns>
        internal List<GopherJoint_Base> GetChildJoints(UInt32 MeshID)
        {
            List<GopherJoint_Base> ret = new List<GopherJoint_Base>();

            foreach (var joint in joints)
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
            foreach (var joint in joints)
            {
                if (joint.Meta.ChildID == MeshID)
                    return false;
            }
            return true;
        }
    }
}
