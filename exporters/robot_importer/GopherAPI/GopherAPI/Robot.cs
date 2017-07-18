using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GopherAPI.STL;
using GopherAPI.Properties;

namespace GopherAPI
{
    public enum JointSearchMode { PARENTS, CHILDREN, BOTH }
    public class Robot
    {
        public Bitmap Thumbnail;

        protected List<STLMesh> meshes = new List<STLMesh>();
        protected List<STLAttribute> attributes = new List<STLAttribute>();
        protected List<Joint> joints = new List<Joint>();
        protected List<IJointAttribute> jointAttributes = new List<IJointAttribute>();
        //private Other.Vec3 spawn;

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
        /// <summary>
        /// Pretty much useless to access it here. Use GetJointAttribute and reference a joint to get the attributes attached to it
        /// </summary>
        public List<IJointAttribute> JointAttributes { get => jointAttributes; }

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
        /// Returns an array of all the Joints attached to a given STLMesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        private Joint[] GetJoints(STLMesh mesh, JointSearchMode mode)
        {
            List<Joint> ret = new List<Joint>();
            switch (mode)
            {
                case JointSearchMode.PARENTS:
                    foreach (var j in joints)
                    {
                        if (j.ParentID == mesh.MeshID)
                            ret.Add(j);
                    }
                    break;
                case JointSearchMode.CHILDREN:
                    foreach (var j in joints)
                    {
                        if (j.ChildID == mesh.MeshID)
                            ret.Add(j);
                    }
                    break;
                case JointSearchMode.BOTH:
                    foreach (var j in joints)
                    {
                        if (j.ChildID == mesh.MeshID || j.ParentID == mesh.MeshID)
                            ret.Add(j);
                    }
                    break;
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Gets the IJointAttribute attached to the joint parameter. If none exists, it will return a 'NoDriver' struct with a matching jointID
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        public IJointAttribute GetJointAttribute(Joint joint)
        {
            foreach (var ja in jointAttributes)
            {
                if (ja.GetJointID() == joint.JointID)
                    return ja;
            }
            return new NoDriver(joint.JointID);
        }

        protected Robot(Robot robot)
        {
            meshes = robot.Meshes;
            attributes = robot.Attributes;
            joints = robot.Joints;
            jointAttributes = robot.JointAttributes;
        }
        public Robot() { }

        public SortedRobot GetSortedRobot()
        {
            return new SortedRobot(this);
        }
    }

    public class SortedRobot : Robot
    {
        private List<STLMesh> staticMeshes = new List<STLMesh>();
        private List<STLMesh> dynamicMeshes = new List<STLMesh>();
        private DriveTrain driveTrain;

        public List<STLMesh> StaticMeshes => staticMeshes;
        public  List<STLMesh> DynamicMeshes => dynamicMeshes;
        public DriveTrain DriveTrain => driveTrain;

        public SortedRobot(Robot robot) : base(robot)
        {
            //Sort joints into wheel joints and non-wheel joints, then filters out duplicates
            List<Joint> NonWheelJoints = new List<Joint>();
            foreach (var ja in jointAttributes)
            {
                if(!ja.GetIsDriveWheel())
                    NonWheelJoints.Add(GetJoint(ja.GetJointID()));
            }
            NonWheelJoints = (List<Joint>)NonWheelJoints.Distinct(); //filter duplicates

            List<Joint> WheelJoints = new List<Joint>();
            foreach (var ja in jointAttributes)
            {
                if (ja.GetIsDriveWheel())
                    WheelJoints.Add(GetJoint(ja.GetJointID()));
            }
            WheelJoints = (List<Joint>)WheelJoints.Distinct(); //filter duplicates

            //Gets the IDs of all the meshes with joints attached, then adds all the meshes to dynamicMeshes
            List<uint> dynamicMeshIDs = new List<uint>();
            foreach(var j in NonWheelJoints)
            {
                dynamicMeshIDs.Add(j.ChildID);
                dynamicMeshIDs.Add(j.ParentID);
            }
            dynamicMeshIDs = (List<uint>)dynamicMeshIDs.Distinct(); //filter duplicates
            foreach (var id in dynamicMeshIDs)
            {
                dynamicMeshes.Add(GetMesh(id));
            }

            //Creates the DriveTrain object
            List<Wheel> wheels = new List<Wheel>();
            List<STLMesh> parents = new List<STLMesh>();
            foreach(var j in WheelJoints)
            {
                wheels.Add(new Wheel(GetMesh(j.ChildID), j));
                parents.Add(GetMesh(j.ParentID));
            }
            driveTrain = new DriveTrain(wheels.ToArray(), parents.ToArray());

            //Adds to the staticMeshes List
            foreach(var mesh in meshes)
            {
                if (!dynamicMeshes.Contains(mesh) && !DriveTrain.Contains(mesh))
                    staticMeshes.Add(mesh);
            }
        }
    }
}
