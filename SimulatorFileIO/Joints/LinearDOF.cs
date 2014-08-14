using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Contains information about a linear degree of freedom.
/// </summary>
public interface LinearDOF
{
    float currentLinearPosition
    {
        get;
    }
    float upperLinearLimit
    {
        get;
        set;
    }
    float lowerLinearLimit
    {
        get;
        set;
    }

    BXDVector3 translationalAxis
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

public static class LinearDOFExtensions
{
    public static bool hasUpperLinearLimit(this LinearDOF dof)
    {
        return !float.IsPositiveInfinity(dof.upperLinearLimit);
    }

    public static bool hasLowerLinearLimit(this LinearDOF dof)
    {
        return !float.IsNegativeInfinity(dof.lowerLinearLimit);
    }
}