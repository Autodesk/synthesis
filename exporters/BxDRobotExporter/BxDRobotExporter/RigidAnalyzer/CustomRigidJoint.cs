using System;
using System.Collections.Generic;
using Inventor;

namespace BxDRobotExporter.RigidAnalyzer
{
    public class CustomRigidJoint
    {
        public List<AssemblyJoint> Joints = new List<AssemblyJoint>();
        public List<AssemblyConstraint> Constraints = new List<AssemblyConstraint>();
        public CustomRigidGroup GroupOne;
        public CustomRigidGroup GroupTwo;

        public dynamic GeomOne, GeomTwo;
        public NameValueMap Options;

        public bool JointBased;

        public RigidBodyJointTypeEnum Type;

        public CustomRigidJoint(RigidBodyJoint joint, CustomRigidGroup groupOnez, CustomRigidGroup groupTwoz)
        {
            foreach (AssemblyJoint aj in joint.Joints)
            {
                Joints.Add(aj);
            }
            foreach (AssemblyConstraint cj in joint.Constraints)
            {
                Constraints.Add(cj);
            }
            GroupOne = groupOnez;
            GroupTwo = groupTwoz;
            Type = joint.JointType;
            joint.GetJointData(out GeomOne, out GeomTwo, out Options);
            try
            {
                JointBased = Options.get_Value("FromJoint");
            }
            catch
            {
                JointBased = false;
            }
        }

        public override string ToString()
        {
            return "RigidJoint (" + Enum.GetName(typeof(RigidBodyJointTypeEnum), Type) + "): " + Constraints.Count + "C, " + Joints.Count + "J";
        }
    }
}
