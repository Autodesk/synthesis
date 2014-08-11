using System.Collections.Generic;
using System.Text;
using Inventor;

public class RigidNode : RigidNode_Base
{
    public delegate void DeferredCalculation(RigidNode node);

    public CustomRigidGroup group;
    private Dictionary<string, DeferredCalculation> deferredCalculations = new Dictionary<string, DeferredCalculation>();

    public RigidNode()
        : this(null)
    {
    }
    public RigidNode(CustomRigidGroup grp)
    {
        this.group = grp;
    }

    public override object GetModel()
    {
        return group;
    }

    public bool RegisterDeferredCalculation(string id, DeferredCalculation calc)
    {
        try
        {
            deferredCalculations.Add(id, calc);
            return false;
        }
        catch
        {
            deferredCalculations[id] = calc;
            return true;
        }
    }

    public bool UnregisterDeferredCalculation(string id)
    {
        return deferredCalculations.Remove(id);
    }

    public void DoDeferredCalculations()
    {
        try
        {
            foreach (DeferredCalculation calc in deferredCalculations.Values)
            {
                calc(this);
            }
        }

        catch
        {
        }

        deferredCalculations.Clear();
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