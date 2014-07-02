using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class RigidNode : RigidNode_Base
{
    public CustomRigidGroup group;

    public RigidNode()
        : this(null)
    {
    }
    public RigidNode(CustomRigidGroup grp)
    {
        this.group = grp;
    }

    public override object getModel()
    {
        return group;
    }


    private static RigidNode createRigidNode(Dictionary<CustomRigidGroup, List<CustomRigidJoint>> jointDictionary, Dictionary<CustomRigidGroup, RigidNode> nodeDictionary, CustomRigidGroup groupz, RigidNode parentz = null)
    {
        RigidNode node = null;
        if ((nodeDictionary.TryGetValue(groupz, out node)))
        {
            return node;
        }
        else
        {
            node = new RigidNode();
        }
        Console.WriteLine("Creating node for " + groupz.ToString() + " at level " + (node.level));
        node.group = groupz;
        nodeDictionary[node.group] = node;

        List<CustomRigidJoint> joints = null;
        if ((jointDictionary.TryGetValue(node.group, out joints)))
        {
            foreach (CustomRigidJoint joint in joints)
            {
                CustomRigidGroup childGroup = joint.groupOne.Equals(node.group) ? joint.groupTwo : joint.groupOne;
                if (nodeDictionary.ContainsKey(childGroup))
                    continue;
                node.addChild(SkeletalJoint.create(joint, groupz), createRigidNode(jointDictionary, nodeDictionary, childGroup, node));
            }
        }
        return node;
    }

    public static RigidNode generateNodeTree(CustomRigidResults results)
    {
        Dictionary<CustomRigidGroup, List<CustomRigidJoint>> jointDictionary = new Dictionary<CustomRigidGroup, List<CustomRigidJoint>>();
        CustomRigidGroup baseGroup = null;
        foreach (CustomRigidGroup group in results.groups)
        {
            jointDictionary[group] = new List<CustomRigidJoint>();
            if ((group.grounded))
                baseGroup = group;
        }
        if ((baseGroup == null))
            return null;
        foreach (CustomRigidJoint joint in results.joints)
        {
            jointDictionary[joint.groupOne].Add(joint);
            jointDictionary[joint.groupTwo].Add(joint);
        }

        Dictionary<CustomRigidGroup, RigidNode> nodeDictionary = new Dictionary<CustomRigidGroup, RigidNode>();
        return createRigidNode(jointDictionary, nodeDictionary, baseGroup);
    }
}