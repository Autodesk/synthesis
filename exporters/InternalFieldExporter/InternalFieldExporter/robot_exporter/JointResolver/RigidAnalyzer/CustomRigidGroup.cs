using System;
using System.Collections.Generic;
using System.Text;
using Inventor;

public class CustomRigidGroup
{
    
    public List<ComponentOccurrence> occurrences = new List<ComponentOccurrence>();

    //Checks to see if the RigidBody is grounded
    public bool grounded;

    public string fullQualifier;
    public static string GetGroupQualifier(RigidBodyGroup group)
    {
        StringBuilder builder = new StringBuilder();
        foreach (ComponentOccurrence occ in group.Occurrences)
        {
            builder.Append(occ.Name);
        }
        return group.GroupID + "_" + group.Parent.Parent.Parent.Parent.InternalName + "_" + builder.ToString();
    }

    /// <summary>
    /// Adds a occurrence for each RigidBodyGroup
    /// </summary>
    /// <param name="group"></param>
    public CustomRigidGroup(RigidBodyGroup group)
    {
        foreach (ComponentOccurrence comp in group.Occurrences)
        {
            occurrences.Add(comp);
        }
        
        grounded = group.Grounded;
        fullQualifier = GetGroupQualifier(group);
    }

    /// <summary>
    /// returns a string that is tagged onto the end of the GroupID
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (occurrences.Count == 1)
        {
            return occurrences[0].Name;
        }
        string res = "[";
        foreach (ComponentOccurrence occ in occurrences)
        {
            if (res.Length > 100)
            {
                res += "...";
                break; // TODO: might not be correct. Was : Exit For
            }
            res += occ.Name + ";";
        }
        res += "]";
        return res;
    }

    /// <summary>
    /// Checks to see if object is CustomRigidGroup or a RigidBodyGroup
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if ((obj is CustomRigidGroup))
        {
            return fullQualifier.Equals(((CustomRigidGroup)obj).fullQualifier);
        }
        else if ((obj is RigidBodyGroup))
        {
            return fullQualifier.Equals(GetGroupQualifier((RigidBodyGroup)obj));
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return fullQualifier.GetHashCode();
    }

    public bool Contains(ComponentOccurrence c)
    {
        return occurrences.Contains(c);
    }
}
