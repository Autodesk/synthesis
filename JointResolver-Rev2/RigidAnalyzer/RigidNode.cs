using System.Collections.Generic;
using System.Text;
using Inventor;

public class RigidNode : RigidNode_Base
{
    public delegate void DeferredCalculation(RigidNode node);

    public CustomRigidGroup group;

    public RigidNode()
    {
        group = null;
    }

    //public RigidNode(OGLViewer.OGL_RigidNode oglNode)
    //{
    //    modelFullID = oglNode.modelFullID;
    //    modelFileName = oglNode.modelFileName;

    //    foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> child in baseData.children)
    //    {
    //        AddChild(child.Key, new OGL_RigidNode(child.Value));
    //    }
    //}

    public RigidNode(CustomRigidGroup grp)
    {
        this.group = grp;
    }

    public override object GetModel()
    {
        return group;
    }

    public override string GetModelID()
    {
        // Compile a model ID
        List<string> components = new List<string>();
        if (group == null)
        {
            components.Add(GetHashCode().ToString());
        }
        else
        {
            foreach (ComponentOccurrence oc in group.occurrences)
            {
                components.Add(oc.Name.ToLower());
            }
        }
        components.Sort();
        StringBuilder id = new StringBuilder();
        foreach (string comp in components)
        {
            id.Append(comp);
            id.Append("-_-");
        }
        return id.ToString();
    }
}