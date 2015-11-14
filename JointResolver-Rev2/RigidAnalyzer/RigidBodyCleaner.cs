using System;
using System.Collections.Generic;
using Inventor;

static class RigidBodyCleaner
{
    /// <summary>
    /// Removes all the meaningless items from the given group of rigid body results, simplifying the model.
    /// </summary>
    /// <remarks>
    /// Meaningless items include...
    /// <list type="bullet">
    /// <item><description>Component occurrences that are suppressed</description></item>
    /// <item><description>Rigid joints between the same object</description></item>
    /// <item><description>Rigid joints with meaningless groups</description></item>
    /// <item><description>Rigid groups with no objects</description></item>
    /// </list>
    /// </remarks>
    /// <param name="results">Rigid results to clean</param>
    public static void CleanMeaningless(CustomRigidResults results)
    {
        foreach (CustomRigidGroup group in results.groups)
        {
            group.occurrences.RemoveAll(item => item.Suppressed);
        }
        //Removes groups with no components.
        results.groups.RemoveAll(item => item.occurrences.Count <= 0);
        //Removes joints that attach a group to itself or an empty group.
        results.joints.RemoveAll(item => item.groupOne.Equals(item.groupTwo) || item.groupOne.occurrences.Count <= 0 || item.groupTwo.occurrences.Count <= 0);
    }

    /// <summary>
    /// Merges all the grounded bodies within the given rigid results into a single group. 
    /// </summary>
    /// <remarks>
    /// This also updates the references of all the rigid joints to the new groups.
    /// </remarks>
    /// <exception cref="InvalidOperationException">No grounded bodies exist to merge.</exception>
    /// <param name="results">Rigid results to clean</param>
    public static void CleanGroundedBodies(CustomRigidResults results)
    {
        CustomRigidGroup firstRoot = null;
        //Bundles all the grounded components together into one CustomRigidGroup.
        foreach (CustomRigidGroup group in results.groups)
        {
            if (group.grounded)
            {
                if ((firstRoot == null))
                {
                    firstRoot = group;
                }
                else
                {
                    firstRoot.occurrences.AddRange(group.occurrences);
                    group.occurrences.Clear();
                }
            }
        }
        if (firstRoot != null)
        {
            foreach (CustomRigidJoint joint in results.joints)
            {
                //If a group has no occurrences, its because they were transferred to the firstRoot group.  Updates joints to attach to that joint.
                if (joint.groupOne.occurrences.Count == 0)
                {
                    joint.groupOne = firstRoot;
                }
                if (joint.groupTwo.occurrences.Count == 0)
                {
                    joint.groupTwo = firstRoot;
                }
            }
        }
        else
        {
            throw new InvalidOperationException("No ground!");
        }
        CleanMeaningless(results);
    }

    /// <summary>
    /// Generates a mapping between each rigid group and all the other groups connected to it.
    /// </summary>
    /// <param name="results">The rigid results to generate joint maps from.</param>
    /// <param name="joints">A mapping between each rigid group and a set of rigid groups connected by a joint.</param>
    /// <param name="constraints">A mapping between each rigid group and a set of rigid groups connected by constraints.</param>
    private static void GenerateJointMaps(CustomRigidResults results, Dictionary<CustomRigidGroup, HashSet<CustomRigidGroup>> joints, Dictionary<CustomRigidGroup, HashSet<CustomRigidGroup>> constraints)
    {
        //Creates a spot in the dictionary for each component.
        foreach (CustomRigidGroup group in results.groups)
        {
            joints.Add(group, new HashSet<CustomRigidGroup>());
            constraints.Add(group, new HashSet<CustomRigidGroup>());
        }
        //Adds connections between components going both directions.
        foreach (CustomRigidJoint j in results.joints)
        {
            if (j.jointBased && j.joints.Count > 0 && j.joints[0].Definition.JointType != AssemblyJointTypeEnum.kRigidJointType)
            {
                joints[j.groupOne].Add(j.groupTwo);
                joints[j.groupTwo].Add(j.groupOne);
            }
            else
            {
                constraints[j.groupOne].Add(j.groupTwo);
                constraints[j.groupTwo].Add(j.groupOne);
            }
        }
    }

    /// <summary>
    /// Represents a joint that will be created once preprocessing is complete.
    /// </summary>
    private class PlannedJoint
    {
        public RigidNode node;
        public RigidNode parentNode;
        public CustomRigidJoint joint;
    }

    /// <summary>
    /// Merges any groups that are connected only with constraints and generate a node tree.
    /// </summary>
    /// <remarks>
    /// This starts at whichever rigid group is grounded, then branches out along rigid joints from there.
    /// If the rigid joint is movable (made of assembly joint(s)) then another node is created, if the joint
    /// is constraint-only then the leaf node is merged into the current branch.
    /// </remarks>
    /// <param name="results">Rigid results to clean</param>
    public static RigidNode BuildAndCleanDijkstra(CustomRigidResults results)
    {
        Dictionary<CustomRigidGroup, HashSet<CustomRigidGroup>> constraints = new Dictionary<CustomRigidGroup, HashSet<CustomRigidGroup>>();
        Dictionary<CustomRigidGroup, HashSet<CustomRigidGroup>> joints = new Dictionary<CustomRigidGroup, HashSet<CustomRigidGroup>>();
        GenerateJointMaps(results, joints, constraints);

        // Mapping rigid group to merge-into group
        Dictionary<CustomRigidGroup, CustomRigidGroup> mergePattern = new Dictionary<CustomRigidGroup, CustomRigidGroup>();
        // Mapping rigid group to skeletal node
        Dictionary<CustomRigidGroup, RigidNode> baseNodes = new Dictionary<CustomRigidGroup, RigidNode>();
        // Deffered joint creation.  Required so merge can take place.
        List<PlannedJoint> plannedJoints = new List<PlannedJoint>();
        // The base of the skeletal tree
        RigidNode baseRoot = null;

        // All the currently open groups as an array {currentGroup, mergeIntoGroup}
        List<CustomRigidGroup[]> openNodes = new List<CustomRigidGroup[]>();
        // All the groups that have been processed.  (Closed nodes)
        HashSet<CustomRigidGroup> closedNodes = new HashSet<CustomRigidGroup>();

        // Find the first grounded group, the start point for dijkstra's algorithm.
        foreach (CustomRigidGroup grp in results.groups)
        {
            if (grp.grounded)
            {
                openNodes.Add(new CustomRigidGroup[] { grp, grp });
                closedNodes.Add(grp);
                baseNodes.Add(grp, baseRoot = new RigidNode(Guid.NewGuid(), grp));
                break; //Should only contain one grounded group, as they have all been merged together.
            }
        }
        Console.WriteLine("Determining merge commands");
        while (openNodes.Count > 0)
        {
            List<CustomRigidGroup[]> newOpen = new List<CustomRigidGroup[]>();
            foreach (CustomRigidGroup[] node in openNodes)
            {
                // Get all connections
                HashSet<CustomRigidGroup> cons = constraints[node[0]];
                HashSet<CustomRigidGroup> jons = joints[node[0]];
                foreach (CustomRigidGroup jonConn in jons)
                {
                    if (!closedNodes.Add(jonConn)) //Moves on to next if the connected component is already in closedNodes.
                        continue;
                    RigidNode rnode = new RigidNode(Guid.NewGuid(), jonConn); //Makes a new rigid node for the connected component.
                    baseNodes.Add(jonConn, rnode);

                    //Find the actual joint between the two components.
                    foreach (CustomRigidJoint jnt in results.joints)
                    {
                        if (jnt.joints.Count > 0 && ((jnt.groupOne.Equals(jonConn) && jnt.groupTwo.Equals(node[0]))
                            || (jnt.groupOne.Equals(node[0]) && jnt.groupTwo.Equals(jonConn))))
                        {
                            PlannedJoint pJoint = new PlannedJoint();
                            pJoint.joint = jnt;
                            pJoint.parentNode = baseNodes[node[1]];
                            pJoint.node = rnode;
                            plannedJoints.Add(pJoint);
                            newOpen.Add(new CustomRigidGroup[] { jonConn, jonConn });
                            break;
                        }
                    }
                }
                foreach (CustomRigidGroup consConn in cons)
                {
                    if (!closedNodes.Add(consConn))
                        continue;
                    mergePattern.Add(consConn, node[1]); 
                    newOpen.Add(new CustomRigidGroup[] { consConn, node[1] }); //Uses node[1] to ensure all constrained groups are merged into the same group.
                }
            }
            openNodes = newOpen;
        }

        Console.WriteLine("Do " + mergePattern.Count + " merge commands");
        //Transfers components between constrained groups.
        foreach (KeyValuePair<CustomRigidGroup, CustomRigidGroup> pair in mergePattern)
        {
            pair.Value.occurrences.AddRange(pair.Key.occurrences); //Transfers key components to related value.
            pair.Key.occurrences.Clear();
            pair.Value.grounded = pair.Value.grounded || pair.Key.grounded; //Is it possible for the key to be grounded?  Would there have to be a loop of groups?
        }
        Console.WriteLine("Resolve broken joints");
        //Goes through each joint and sees if it was merged.  If it was, it attaches the group left behind to the group that was merged into.
        foreach (CustomRigidJoint joint in results.joints)
        {
            CustomRigidGroup updatedGroup = null; //Stores the group that the previous groupOne/Two was merged into.
            if (mergePattern.TryGetValue(joint.groupOne, out updatedGroup))
            {
                joint.groupOne = updatedGroup;
            }
            if (mergePattern.TryGetValue(joint.groupTwo, out updatedGroup))
            {
                joint.groupTwo = updatedGroup;
            }
        }
        Console.WriteLine("Creating planned skeletal joints");
        foreach (PlannedJoint pJoint in plannedJoints)
        {
            SkeletalJoint_Base sJ = SkeletalJoint.Create(pJoint.joint, pJoint.parentNode.group);
            pJoint.parentNode.AddChild(sJ, pJoint.node);
        }
        Console.WriteLine("Cleanup remainders");
        CleanMeaningless(results);
        return baseRoot;
    }
}