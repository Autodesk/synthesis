using System.Collections.Generic;

/// <summary>
/// Contains DOF information for a cylindrical joint.  (1 angular and 1 linear)
/// </summary>
public class CylindricalJoint_Base : SkeletalJoint_Base
{
    #region LinearDOF_Impl
    private class LinearDOF_Impl : LinearDOF
    {
        private readonly CylindricalJoint_Base cjb;
        public LinearDOF_Impl(CylindricalJoint_Base cjb)
        {
            this.cjb = cjb;
        }

        public float currentPosition
        {
            get
            {
                return cjb.currentLinearPosition;
            }
        }

        public float upperLimit
        {
            get
            {
                cjb.enforceOrder();
                return cjb.hasLinearEndLimit ? cjb.linearLimitEnd : float.PositiveInfinity;
            }
            set
            {
                cjb.linearLimitEnd = value;
                cjb.hasLinearEndLimit = !float.IsInfinity(cjb.linearLimitEnd);
            }
        }

        public float lowerLimit
        {
            get
            {
                cjb.enforceOrder();
                return cjb.hasLinearStartLimit ? cjb.linearLimitStart : float.NegativeInfinity;
            }
            set
            {
                cjb.linearLimitStart = value;
                cjb.hasLinearStartLimit = !float.IsInfinity(cjb.linearLimitStart);
            }
        }

        public BXDVector3 translationalAxis
        {
            get
            {
                return cjb.axis;
            }
            set
            {
                cjb.axis = value;
            }
        }

        public BXDVector3 basePoint
        {
            get
            {
                return cjb.basePoint;
            }
            set
            {
                cjb.basePoint = value;
            }
        }
    }
    #endregion
    #region AngularDOF_Impl
    private class AngularDOF_Impl : AngularDOF
    {
        private readonly CylindricalJoint_Base cjb;
        public AngularDOF_Impl(CylindricalJoint_Base cjb)
        {
            this.cjb = cjb;
        }
        public float currentPosition
        {
            get
            {
                return cjb.currentAngularPosition;
            }
        }

        public float upperLimit
        {
            get
            {
                cjb.enforceOrder();
                return cjb.hasAngularLimit ? cjb.angularLimitHigh : float.PositiveInfinity;
            }
            set
            {
                cjb.angularLimitHigh = value;
                cjb.hasAngularLimit = !float.IsInfinity(cjb.angularLimitHigh) && !float.IsInfinity(cjb.angularLimitLow);
            }
        }

        public float lowerLimit
        {
            get
            {
                cjb.enforceOrder();
                return cjb.hasAngularLimit ? cjb.angularLimitLow : float.NegativeInfinity;
            }
            set
            {
                cjb.angularLimitLow = value;
                cjb.hasAngularLimit = !float.IsInfinity(cjb.angularLimitHigh) && !float.IsInfinity(cjb.angularLimitLow);
            }
        }

        public BXDVector3 rotationAxis
        {
            get
            {
                return cjb.axis;
            }
            set
            {
                cjb.axis = value;
            }
        }

        public BXDVector3 basePoint
        {
            get
            {
                return cjb.basePoint;
            }
            set
            {
                cjb.basePoint = value;
            }
        }
    }
    #endregion

    public BXDVector3 axis; //The axis of both rotation and movement;
    public BXDVector3 basePoint;

    public float currentLinearPosition, currentAngularPosition;

    public bool hasAngularLimit;
    public float angularLimitLow;
    public float angularLimitHigh;
    public bool hasLinearStartLimit;
    public bool hasLinearEndLimit;
    public float linearLimitStart;
    public float linearLimitEnd;

    private readonly LinearDOF[] linearDOF;
    private readonly AngularDOF[] angularDOF;

    public CylindricalJoint_Base()
    {
        linearDOF = new LinearDOF[] { new LinearDOF_Impl(this) };
        angularDOF = new AngularDOF[] { new AngularDOF_Impl(this) };
    }

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.CYLINDRICAL;
    }

    protected override void WriteJointInternal(System.IO.BinaryWriter writer)
    {
        enforceOrder();

        writer.Write(basePoint);
        writer.Write(axis);

        //1 indicates a linear limit.
        writer.Write((byte) ((hasAngularLimit ? 1 : 0) | (hasLinearStartLimit ? 2 : 0) | (hasLinearEndLimit ? 4 : 0)));
        if (hasAngularLimit)
        {
            writer.Write(angularLimitLow);
            writer.Write(angularLimitHigh);
        }

        if (hasLinearStartLimit)
        {
            writer.Write(linearLimitStart);
        }
        if (hasLinearEndLimit)
        {
            writer.Write(linearLimitEnd);
        }

        writer.Write(currentLinearPosition);
        writer.Write(currentAngularPosition);
    }

    protected override void ReadJointInternal(System.IO.BinaryReader reader)
    {
        basePoint = reader.ReadRWObject<BXDVector3>();
        axis = reader.ReadRWObject<BXDVector3>();

        byte limits = reader.ReadByte();
        hasAngularLimit = (limits & 1) == 1;
        hasLinearStartLimit = (limits & 2) == 2;
        hasLinearEndLimit = (limits & 4) == 4;

        if (hasLinearStartLimit)
        {
            linearLimitStart = reader.ReadSingle();
        }
        if (hasLinearEndLimit)
        {
            linearLimitEnd = reader.ReadSingle();
        }

        currentLinearPosition = reader.ReadSingle();
        currentAngularPosition = reader.ReadSingle();

        enforceOrder();
    }

    private void enforceOrder()
    {
        if (hasAngularLimit && angularLimitLow > angularLimitHigh)
        {
            float temp = angularLimitHigh;
            angularLimitHigh = angularLimitLow;
            angularLimitLow = temp;
        }
        if (hasLinearStartLimit && hasLinearEndLimit && linearLimitStart > linearLimitEnd)
        {
            float temp = linearLimitEnd;
            linearLimitEnd = linearLimitStart;
            linearLimitStart = temp;
        }
    }

    public override IEnumerable<AngularDOF> GetAngularDOF()
    {
        return angularDOF;
    }

    public override IEnumerable<LinearDOF> GetLinearDOF()
    {
        return linearDOF;
    }
}
