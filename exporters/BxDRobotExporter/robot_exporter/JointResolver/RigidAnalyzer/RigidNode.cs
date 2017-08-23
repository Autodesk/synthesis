using System.Collections.Generic;
using System.Text;
using Inventor;
using System;
using OGLViewer;

public class RigidNode : OGL_RigidNode 
{
    public delegate void DeferredCalculation(RigidNode node);

    public CustomRigidGroup group;

    public RigidNode(Guid guid)
        : base(guid)
    {
        group = null;
    }

    public RigidNode(Guid guid, CustomRigidGroup grp)
        : base(guid)
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
                components.Add(oc.Name);
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