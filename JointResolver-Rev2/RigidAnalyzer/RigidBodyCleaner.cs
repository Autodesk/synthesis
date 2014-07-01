using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

static class RigidBodyCleaner
{
    public static void CleanMeaningless(CustomRigidResults results)
    {
        foreach (CustomRigidGroup group in results.groups)
        {
            group.occurrences.RemoveAll(item => item.Suppressed);
        }
        foreach (CustomRigidJoint joint in results.joints)
        {
            joint.joints.RemoveAll(item => item.OccurrenceOne.Suppressed || item.OccurrenceTwo.Suppressed || item.Suppressed);
            joint.constraints.RemoveAll(item => item.OccurrenceOne.Suppressed || item.OccurrenceTwo.Suppressed || item.Suppressed);
        }
        results.joints.RemoveAll(item => item.groupOne.Equals(item.groupTwo) || item.groupOne.occurrences.Count <= 0 || item.groupTwo.occurrences.Count <= 0 || (item.joints.Count + item.constraints.Count) <= 0);
        results.groups.RemoveAll(item => item.occurrences.Count <= 0);
    }

    public static void CleanGroundedBodies(CustomRigidResults results)
    {
        CustomRigidGroup firstRoot = null;
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
        if (!((firstRoot == null)))
        {
            foreach (CustomRigidJoint joint in results.joints)
            {
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
            throw new Exception("No ground!");
        }
        CleanMeaningless(results);
    }

    public static void CleanConstraintOnly(CustomRigidResults results)
    {
        // Determine what groups move
        Dictionary<string, int> jointCount = new Dictionary<string, int>();
        Dictionary<string, int> constraintCount = new Dictionary<string, int>();
        foreach (CustomRigidJoint joint in results.joints)
        {
            if (joint.joints.Count > 0)
            {
                // Moving joint
                int val = 0;
                if (!(joint.groupOne.grounded))
                {
                    jointCount.TryGetValue(joint.groupOne.fullQualifier, out val);
                    jointCount[joint.groupOne.fullQualifier] = val + 1;
                }
                if (!(joint.groupTwo.grounded))
                {
                    jointCount.TryGetValue(joint.groupTwo.fullQualifier, out val);
                    jointCount[joint.groupTwo.fullQualifier] = val + 1;
                }
            }
            if (joint.constraints.Count > 0)
            {
                // Moving joint
                int val = 0;
                constraintCount.TryGetValue(joint.groupOne.fullQualifier, out val);
                constraintCount[joint.groupOne.fullQualifier] = val + 1;
                constraintCount.TryGetValue(joint.groupTwo.fullQualifier, out val);
                constraintCount[joint.groupTwo.fullQualifier] = val + 1;
            }
        }

        // Determine first level merge intention
        Dictionary<string, string> mergeIntents = new Dictionary<string, string>();
        //Merge key into value
        foreach (CustomRigidJoint joint in results.joints)
        {
            if (joint.joints.Count > 0)
                continue;
            bool oneJointed = false;
            bool twoJointed = false;
            int val = 0;
            jointCount.TryGetValue(joint.groupOne.fullQualifier, out val);
            oneJointed = val > 0;
            jointCount.TryGetValue(joint.groupTwo.fullQualifier, out val);
            twoJointed = val > 0;
            if (oneJointed && twoJointed)
                continue;
            if (!(oneJointed) && !(twoJointed))
            {
                // Determine best thing to fit to
                double groupOneVolume = 0;
                double groupTwoVolume = 0;
                foreach (ComponentOccurrence component in joint.groupOne.occurrences)
                {
                    groupOneVolume += component.MassProperties.Volume;
                }
                foreach (ComponentOccurrence component in joint.groupTwo.occurrences)
                {
                    groupTwoVolume += component.MassProperties.Volume;
                }
                oneJointed = !(joint.groupTwo.grounded) && (groupOneVolume > groupTwoVolume || mergeIntents.ContainsKey(joint.groupOne.fullQualifier) || joint.groupOne.grounded) && !(mergeIntents.ContainsKey(joint.groupTwo.fullQualifier));
                twoJointed = !(oneJointed) && !(joint.groupOne.grounded);
            }
            if (oneJointed)
            {
                if ((mergeIntents.ContainsKey(joint.groupTwo.fullQualifier)))
                {
                    if (!(mergeIntents[joint.groupTwo.fullQualifier].Equals(joint.groupOne.fullQualifier)))
                        Console.WriteLine("[WARN] Double jointed on " + joint.groupTwo.ToString() + System.Environment.NewLine + "\tCURRENT: " + joint.groupOne.ToString() + System.Environment.NewLine + "\tREPLACE: " + results.groupIDToCustom[mergeIntents[joint.groupTwo.fullQualifier]].ToString());
                    continue;
                }
                string cOVal = null;
                if (mergeIntents.TryGetValue(joint.groupOne.fullQualifier, out cOVal) && cOVal.Equals(joint.groupTwo.fullQualifier))
                    continue;
                mergeIntents[joint.groupTwo.fullQualifier] = joint.groupOne.fullQualifier;

                Console.WriteLine("PreMerge " + joint.groupTwo.ToString() + " into " + joint.groupOne.ToString() + " because " + joint.ToString());
            }
            else if (twoJointed)
            {
                if ((mergeIntents.ContainsKey(joint.groupOne.fullQualifier)))
                {
                    if (!(mergeIntents[joint.groupOne.fullQualifier].Equals(joint.groupTwo.fullQualifier)))
                        Console.WriteLine("[WARN] Double jointed on " + joint.groupOne.ToString() + System.Environment.NewLine + "\tCURRENT: " + joint.groupTwo.ToString() + System.Environment.NewLine + "\tREPLACE: " + results.groupIDToCustom[mergeIntents[joint.groupOne.fullQualifier]].ToString());
                    continue;
                }
                string cOVal = null;
                if (mergeIntents.TryGetValue(joint.groupTwo.fullQualifier, out cOVal) && cOVal.Equals(joint.groupOne.fullQualifier))
                    continue;
                mergeIntents[joint.groupOne.fullQualifier] = joint.groupTwo.fullQualifier;

                Console.WriteLine("PreMerge " + joint.groupOne.ToString() + " into " + joint.groupTwo.ToString() + " because " + joint.ToString());
            }
        }

        // Resolve merges and porm merge
        string[] currentKeys = new string[mergeIntents.Keys.Count];
        mergeIntents.Keys.CopyTo(currentKeys, 0);
        foreach (string key in currentKeys)
        {
            CustomRigidGroup myGroup = results.groupIDToCustom[key];

            string currentMerge = mergeIntents[key];
            while (mergeIntents.ContainsKey(currentMerge))
            {
                currentMerge = mergeIntents[currentMerge];
            }
            mergeIntents[key] = currentMerge;

            CustomRigidGroup mergeInto = results.groupIDToCustom[mergeIntents[key]];
            Console.WriteLine("FinalMerge " + myGroup.ToString() + " into " + mergeInto.ToString());
            Console.WriteLine();

            mergeInto.occurrences.AddRange(myGroup.occurrences);
            mergeInto.grounded = mergeInto.grounded || myGroup.grounded;
            myGroup.occurrences.Clear();
        }

        // Clean up joints to use new groups
        foreach (CustomRigidJoint joint in results.joints)
        {
            if (joint.groupOne.occurrences.Count > 0 && joint.groupTwo.occurrences.Count > 0)
                continue;

            if (joint.groupTwo.occurrences.Count == 0)
            {
                // Merged joint?
                string newOccurrence = null;
                if ((mergeIntents.TryGetValue(joint.groupTwo.fullQualifier, out newOccurrence)))
                {
                    results.groupIDToCustom.TryGetValue(newOccurrence, out joint.groupTwo);
                }
            }
            if (joint.groupOne.occurrences.Count == 0)
            {
                // Merged joint?
                string newOccurrence = null;
                if ((mergeIntents.TryGetValue(joint.groupOne.fullQualifier, out newOccurrence)))
                {
                    results.groupIDToCustom.TryGetValue(newOccurrence, out joint.groupOne);
                }
            }
            //Otherwise dispose the joint as needed
        }
        CleanMeaningless(results);
    }
}