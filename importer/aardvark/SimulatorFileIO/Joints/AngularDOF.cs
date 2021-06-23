/// <summary>
/// Contains information about a rotational degree of freedom.
/// </summary>
public interface AngularDOF
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
        return !float.IsPositiveInfinity(dof.upperLimit) || !float.IsNegativeInfinity(dof.lowerLimit);
    }
}