using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface AngularDOF
{
    float currentAngularPosition
    {
        get;
    }
    float upperAngularLimit
    {
        get;
        set;
    }
    float lowerAngularLimit
    {
        get;
        set;
    }

    BXDVector3 rotationAxis
    {
        get;
        set;
    }
    BXDVector3 basePoint
    {
        get;
        set;
    }
}

public static class AngularDOFExtensions
{
    public static bool hasAngularLimits(this AngularDOF dof)
    {
        return !float.IsPositiveInfinity(dof.upperAngularLimit) || !float.IsNegativeInfinity(dof.lowerAngularLimit);
    }
}