using System.Collections.Generic;
using System.Text;
using Inventor;

namespace BxDRobotExporter.RigidAnalyzer
{
    public class CustomRigidGroup
    {
        public List<ComponentOccurrence> Occurrences = new List<ComponentOccurrence>();

        public bool Grounded;

        public string FullQualifier;
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
                Occurrences.Add(comp);
            }
        
            Grounded = group.Grounded;
            FullQualifier = GetGroupQualifier(group);
        }

        public override string ToString()
        {
            if (Occurrences.Count == 1)
            {
                return Occurrences[0].Name;
            }
            string res = "[";
            foreach (ComponentOccurrence occ in Occurrences)
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
                return FullQualifier.Equals(((CustomRigidGroup)obj).FullQualifier);
            }
            else if ((obj is RigidBodyGroup))
            {
                return FullQualifier.Equals(GetGroupQualifier((RigidBodyGroup)obj));
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return FullQualifier.GetHashCode();
        }

        public bool Contains(ComponentOccurrence c)
        {
            return Occurrences.Contains(c);
        }
    }
}
