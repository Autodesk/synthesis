using System;
using System.Collections.Generic;
using Inventor;

namespace BxDRobotExporter.RigidAnalyzer
{
    public class CustomRigidResults
    {
        public List<CustomRigidGroup> Groups;
        public List<CustomRigidJoint> Joints;

        public Dictionary<string, CustomRigidGroup> GroupIdToCustom = new Dictionary<string, CustomRigidGroup>();

        /// <summary>
        /// Grabs the joints and groups from the Inventor results, 
        /// </summary>
        /// <param name="results"></param>
        public CustomRigidResults(RigidBodyResults results)
        {
            Console.WriteLine("Building custom dataset");
            Groups = new List<CustomRigidGroup>(results.RigidBodyGroups.Count);
            Joints = new List<CustomRigidJoint>(results.RigidBodyJoints.Count);
            foreach (RigidBodyGroup group in results.RigidBodyGroups)
            {
                CustomRigidGroup tmp = new CustomRigidGroup(group);
                if ((!(GroupIdToCustom.ContainsKey(tmp.FullQualifier))))
                {
                    Groups.Add(tmp);
                    GroupIdToCustom.Add(tmp.FullQualifier, tmp);
                }
                else
                {
                    Console.WriteLine("GroupID Collision: " + GroupIdToCustom[CustomRigidGroup.GetGroupQualifier(group)].ToString() + " and " + tmp.ToString());
                }
                Console.Write("Group " + Groups.Count + "/" + results.RigidBodyGroups.Count + "\tJoint " + Joints.Count + "/" + results.RigidBodyJoints.Count);
            }
            foreach (RigidBodyJoint joint in results.RigidBodyJoints)
            {
                Joints.Add(new CustomRigidJoint(joint, GroupIdToCustom[CustomRigidGroup.GetGroupQualifier(joint.GroupOne)], GroupIdToCustom[CustomRigidGroup.GetGroupQualifier(joint.GroupTwo)]));
                Console.Write("Group " + Groups.Count + "/" + results.RigidBodyGroups.Count + "\tJoint " + Joints.Count + "/" + results.RigidBodyJoints.Count);
            }
            Console.WriteLine();
            Console.WriteLine("Built custom dataset");
            RigidBodyCleaner.CleanMeaningless(this);
        }
    }
}