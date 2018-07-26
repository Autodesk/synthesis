using System;
using System.Collections.Generic;
using System.Text;
using Inventor;

public class CustomRigidResults
{
    public List<CustomRigidGroup> groups;
    public List<CustomRigidJoint> joints;

    public Dictionary<string, CustomRigidGroup> groupIDToCustom = new Dictionary<string, CustomRigidGroup>();

    /// <summary>
    /// Grabs the joints and groups from the Inventor results, 
    /// </summary>
    /// <param name="results"></param>
    public CustomRigidResults(RigidBodyResults results)
    {
        Console.WriteLine("Building custom dataset");
        groups = new List<CustomRigidGroup>(results.RigidBodyGroups.Count);
        joints = new List<CustomRigidJoint>(results.RigidBodyJoints.Count);
        foreach (RigidBodyGroup group in results.RigidBodyGroups)
        {
            CustomRigidGroup tmp = new CustomRigidGroup(group);
            if ((!(groupIDToCustom.ContainsKey(tmp.fullQualifier))))
            {
                groups.Add(tmp);
                groupIDToCustom.Add(tmp.fullQualifier, tmp);
            }
            else
            {
                Console.WriteLine("GroupID Collision: " + groupIDToCustom[CustomRigidGroup.GetGroupQualifier(group)].ToString() + " and " + tmp.ToString());
            }
            Console.Write("Group " + groups.Count + "/" + results.RigidBodyGroups.Count + "\tJoint " + joints.Count + "/" + results.RigidBodyJoints.Count);
        }
        foreach (RigidBodyJoint joint in results.RigidBodyJoints)
        {
            joints.Add(new CustomRigidJoint(joint, groupIDToCustom[CustomRigidGroup.GetGroupQualifier(joint.GroupOne)], groupIDToCustom[CustomRigidGroup.GetGroupQualifier(joint.GroupTwo)]));
            Console.Write("Group " + groups.Count + "/" + results.RigidBodyGroups.Count + "\tJoint " + joints.Count + "/" + results.RigidBodyJoints.Count);
        }
        Console.WriteLine();
        Console.WriteLine("Built custom dataset");
        RigidBodyCleaner.CleanMeaningless(this);
    }
}