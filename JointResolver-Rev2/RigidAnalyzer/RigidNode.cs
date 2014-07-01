using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class RigidNode
{
    private int level;
    public RigidNode parent;
    public CustomRigidJoint parentConnection;
    private SkeletalJoint skeletalJoint;
    public Dictionary<CustomRigidJoint, RigidNode> children = new Dictionary<CustomRigidJoint, RigidNode>();

    public CustomRigidGroup group;
    private static RigidNode createRigidNode(Dictionary<string, List<CustomRigidJoint>> jointDictionary, Dictionary<string, RigidNode> nodeDictionary, CustomRigidGroup groupz, RigidNode parentz = null)
    {
        RigidNode node = null;
        if ((nodeDictionary.TryGetValue(groupz.fullQualifier, out node)))
        {
            return node;
        }
        else
        {
            node = new RigidNode();
        }
        node.parent = parentz;
        node.parentConnection = null;
        if ((node.parent == null))
        {
            node.level = 0;
        }
        else
        {
            node.level = node.parent.level + 1;
        }
        Console.WriteLine("Creating node for " + groupz.ToString() + " at level " + (node.level));
        node.group = groupz;
        nodeDictionary[node.group.fullQualifier] = node;

        List<CustomRigidJoint> joints = null;
        if ((jointDictionary.TryGetValue(node.group.fullQualifier, out joints)))
        {
            foreach (CustomRigidJoint joint in joints)
            {
                CustomRigidGroup childGroup = joint.groupOne.Equals(node.group) ? joint.groupTwo : joint.groupOne;
                if (nodeDictionary.ContainsKey(childGroup.fullQualifier))
                    continue;
                node.children[joint] = createRigidNode(jointDictionary, nodeDictionary, childGroup, node);
                node.children[joint].parentConnection = joint;
            }
        }
        return node;
    }

    public static RigidNode generateNodeTree(CustomRigidResults results)
    {
        Dictionary<string, List<CustomRigidJoint>> jointDictionary = new Dictionary<string, List<CustomRigidJoint>>();
        CustomRigidGroup baseGroup = null;
        foreach (CustomRigidGroup group in results.groups)
        {
            jointDictionary[group.fullQualifier] = new List<CustomRigidJoint>();
            if ((group.grounded))
                baseGroup = group;
        }
        if ((baseGroup == null))
            return null;
        foreach (CustomRigidJoint joint in results.joints)
        {
            jointDictionary[joint.groupOne.fullQualifier].Add(joint);
            jointDictionary[joint.groupTwo.fullQualifier].Add(joint);
        }

        Dictionary<string, RigidNode> nodeDictionary = new Dictionary<string, RigidNode>();
        return createRigidNode(jointDictionary, nodeDictionary, baseGroup);
    }

    public SkeletalJoint getSkeletalJoint()
    {
        if (skeletalJoint == null && parentConnection != null && parent != null && parent.group != null)
        {
            skeletalJoint = SkeletalJoint.create(parentConnection, parent.group);
        }
        return skeletalJoint;
    }

    public override string ToString()
    {
        string result = new string(' ', 3 * level) + "Rigid Node" + System.Environment.NewLine + new string(' ', 3 * level) + "Name: " + group.ToString() + System.Environment.NewLine;
        if (children.Count > 0)
        {
            result += new string(' ', 3 * level) + "Children: ";
            foreach (KeyValuePair<CustomRigidJoint, RigidNode> pair in children)
            {
                result += System.Environment.NewLine + new string(' ', 3 * level + 1) + "- " + pair.Key.ToString();
                result += System.Environment.NewLine + pair.Value.ToString();
            }
        }
        return result;
    }

    public void listAllNodes(List<RigidNode> list)
    {
        list.Add(this);
        foreach (KeyValuePair<CustomRigidJoint, RigidNode> pair in children)
        {
            pair.Value.listAllNodes(list);
        }
    }
}