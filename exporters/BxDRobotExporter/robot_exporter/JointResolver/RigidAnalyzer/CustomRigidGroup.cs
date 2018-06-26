using System;
using System.Collections.Generic;
using System.Text;
using Inventor;

public class CustomRigidGroup
{
    public List<ComponentOccurrence> occurrences = new List<ComponentOccurrence>();

    public bool grounded;

    public ExporterHint hint;

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

    public CustomRigidGroup(RigidBodyGroup group)
    {
        foreach (ComponentOccurrence comp in group.Occurrences)
        {
            occurrences.Add(comp);
        }

        hint = new ExporterHint();
        hint.Convex = true;
        hint.HighResolution = false;
        hint.MultiColor = SynthesisGUI.PluginSettings.GeneralUseFancyColors;
        grounded = group.Grounded;
        fullQualifier = GetGroupQualifier(group);
    }

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
