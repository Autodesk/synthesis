
using System.Collections.Generic;
public class LinearJoint_Base : SkeletalJoint_Base
{
    #region LinearDOF_Impl
    private class LinearDOF_Impl : LinearDOF
    {
        private readonly LinearJoint_Base ljb;
        public LinearDOF_Impl(LinearJoint_Base ljb)
        {
            this.ljb = ljb;
        }

        public float currentPosition
        {
            get
            {
                return ljb.currentLinearPosition;
            }
        }

        public float upperLimit
        {
            get
            {
                ljb.EnforceOrder();
                return ljb.hasUpperLimit ? ljb.linearLimitHigh : float.PositiveInfinity;
            }
            set
            {
                ljb.linearLimitHigh = value;
                ljb.hasUpperLimit = !float.IsInfinity(ljb.linearLimitHigh);
            }
        }

        public float lowerLimit
        {
            get
            {
                ljb.EnforceOrder();
                return ljb.hasLowerLimit ? ljb.linearLimitLow : float.NegativeInfinity;
            }
            set
            {
                ljb.linearLimitLow = value;
                ljb.hasLowerLimit = !float.IsInfinity(ljb.linearLimitLow);
            }
        }

        public BXDVector3 translationalAxis
        {
            get
            {
                return ljb.axis;
            }
            set
            {
                ljb.axis = value;
            }
        }

        public BXDVector3 basePoint
        {
            get
            {
                return ljb.basePoint;
            }
            set
            {
                ljb.basePoint = value;
            }
        }
    }
    #endregion

    public BXDVector3 axis;
    public BXDVector3 basePoint;

    public float currentLinearPosition;
    public bool hasUpperLimit, hasLowerLimit;
    public float linearLimitLow, linearLimitHigh;

    private readonly LinearDOF[] linearDOF;

    public LinearJoint_Base() {
        linearDOF = new LinearDOF[]{ new LinearDOF_Impl(this) };
    }

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.LINEAR;
    }

    protected override void WriteBinaryJointInternal(System.IO.BinaryWriter writer)
    {
        EnforceOrder();

        writer.Write(basePoint);
        writer.Write(axis);

        writer.Write((byte) ((hasLowerLimit ? 1 : 0) | (hasUpperLimit ? 2 : 0)));
        if (hasLowerLimit)
        {
            writer.Write(linearLimitLow);
        }
        if (hasUpperLimit)
        {
            writer.Write(linearLimitHigh);
        }

        writer.Write(currentLinearPosition);
    }

    protected override void ReadBinaryJointInternal(System.IO.BinaryReader reader)
    {
        basePoint = reader.ReadRWObject<BXDVector3>();
        axis = reader.ReadRWObject<BXDVector3>();

        byte limitFlags = reader.ReadByte();
        hasLowerLimit = (limitFlags & 1) == 1;
        hasUpperLimit = (limitFlags & 2) == 2;
        if (hasLowerLimit)
        {
            linearLimitLow = reader.ReadSingle();
        }
        if (hasUpperLimit)
        {
            linearLimitHigh = reader.ReadSingle();
        }
        currentLinearPosition = reader.ReadSingle();

        EnforceOrder();
    }

    public void EnforceOrder()
    {
        if (hasLowerLimit && hasUpperLimit && linearLimitLow > linearLimitHigh)
        {
            float temp = linearLimitHigh;
            linearLimitHigh = linearLimitLow;
            linearLimitLow = temp;
        }
    }

    public override IEnumerable<AngularDOF> GetAngularDOF()
    {
        return new AngularDOF[0];
    }

    public override IEnumerable<LinearDOF> GetLinearDOF()
    {
        return linearDOF;
    }
}