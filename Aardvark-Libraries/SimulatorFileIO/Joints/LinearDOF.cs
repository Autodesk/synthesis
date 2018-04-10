/// <summary>
/// Contains information about a linear degree of freedom.
/// </summary>
public interface LinearDOF
{
    float currentPosition
    {
        get;
    }
    float upperLimit
    {
        get;
        set;
    }
    float lowerLimit
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
        return !float.IsPositiveInfinity(dof.upperLimit);
    }

    public static bool hasLowerLinearLimit(this LinearDOF dof)
    {
        return !float.IsNegativeInfinity(dof.lowerLimit);
    }
}