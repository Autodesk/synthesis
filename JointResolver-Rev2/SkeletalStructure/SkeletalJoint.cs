using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public abstract class SkeletalJoint
{
    private static HighlightSet parentHS = null;
    private static HighlightSet childHS = null;

    protected CustomRigidGroup childGroup;
    protected CustomRigidGroup parentGroup;
    protected CustomRigidJoint rigidJoint;
    protected AssemblyJointDefinition asmJoint;

    protected bool childIsTheOne;
    public abstract string ExportData();
    public override abstract string ToString();

    public SkeletalJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (rigidJoint.joints.Count != 1)
            throw new Exception("Not a proper joint");

        asmJoint = rigidJoint.joints[0].Definition;
        childGroup = null;
        parentGroup = parent;
        this.rigidJoint = rigidJoint;
        if ((rigidJoint.groupOne.Equals(parent)))
        {
            childGroup = rigidJoint.groupTwo;
            childIsTheOne = false;
        }
        else if (rigidJoint.groupTwo.Equals(parent))
        {
            childGroup = rigidJoint.groupOne;
            childIsTheOne = true;
        }
        if ((childGroup == null))
            throw new Exception("Not a proper joint");
    }

    public CustomRigidGroup GetChild()
    {
        return childGroup;
    }

    public CustomRigidGroup GetParent()
    {
        return parentGroup;
    }

    public void DoHighlight()
    {
        if ((childHS == null))
        {
            childHS = Program.invApplication.ActiveDocument.CreateHighlightSet();
            childHS.Color.SetColor(0, 0, Convert.ToByte(255));
        }
        if ((parentHS == null))
        {
            parentHS = Program.invApplication.ActiveDocument.CreateHighlightSet();
            parentHS.Color.SetColor(Convert.ToByte(255), 0, 0);
        }
        childHS.Clear();
        parentHS.Clear();
        foreach (ComponentOccurrence child in childGroup.occurrences)
        {
            childHS.AddItem(child);
        }
        foreach (ComponentOccurrence parent in parentGroup.occurrences)
        {
            parentHS.AddItem(parent);
        }
    }

    public static SkeletalJoint create(CustomRigidJoint rigidJoint, CustomRigidGroup parent)
    {
        if (RotationalJoint.isRotationalJoint(rigidJoint))
            return new RotationalJoint(parent, rigidJoint);
        return null;
    }

    public static void cleanupHS()
    {
        if (!((childHS == null)))
            childHS.Delete();
        if (!((parentHS == null)))
            parentHS.Delete();
    }

}