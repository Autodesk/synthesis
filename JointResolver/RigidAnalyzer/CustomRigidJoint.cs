using System;
using System.Collections.Generic;
using System.Text;
using Inventor;

public class CustomRigidJoint
{
    public List<AssemblyJoint> joints = new List<AssemblyJoint>();
    public List<AssemblyConstraint> constraints = new List<AssemblyConstraint>();
    public CustomRigidGroup groupOne;
    public CustomRigidGroup groupTwo;

    public dynamic geomOne, geomTwo;
    public NameValueMap options;

    public bool jointBased;

    public RigidBodyJointTypeEnum type;

    public CustomRigidJoint(RigidBodyJoint joint, CustomRigidGroup groupOnez, CustomRigidGroup groupTwoz)
    {
        foreach (AssemblyJoint aj in joint.Joints)
        {
            joints.Add(aj);
        }
        foreach (AssemblyConstraint cj in joint.Constraints)
        {
            constraints.Add(cj);
        }
        groupOne = groupOnez;
        groupTwo = groupTwoz;
        type = joint.JointType;
        joint.GetJointData(out geomOne, out geomTwo, out options);
        try
        {
            jointBased = options.get_Value("FromJoint");
        }
        catch
        {
            jointBased = false;
        }
    }

    public override string ToString()
    {
        return "RigidJoint (" + Enum.GetName(typeof(RigidBodyJointTypeEnum), type) + "): " + constraints.Count + "C, " + joints.Count + "J";
    }
}
