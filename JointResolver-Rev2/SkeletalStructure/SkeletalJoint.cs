using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class SkeletalJoint
{
    public CustomRigidGroup childGroup;
    public CustomRigidGroup parentGroup;
    public CustomRigidJoint rigidJoint;
    public AssemblyJointDefinition asmJoint;

    public bool childIsTheOne;

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
        ComponentHighlighter.prepareHighlight();
        ComponentHighlighter.clearHighlight();
        foreach (ComponentOccurrence child in childGroup.occurrences)
        {
            ComponentHighlighter.childHS.AddItem(child);
        }
        foreach (ComponentOccurrence parent in parentGroup.occurrences)
        {
            ComponentHighlighter.parentHS.AddItem(parent);
        }
    }

    public static SkeletalJoint_Base create(CustomRigidJoint rigidJoint, CustomRigidGroup parent)
    {
        if (RotationalJoint.isRotationalJoint(rigidJoint))
            return new RotationalJoint(parent, rigidJoint);
        if (LinearJoint.isLinearJoint(rigidJoint))
            return new LinearJoint(parent, rigidJoint);
        return null;
    }
}