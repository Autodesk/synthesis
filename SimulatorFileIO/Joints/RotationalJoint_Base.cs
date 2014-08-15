
using System.Collections.Generic;
public class RotationalJoint_Base : SkeletalJoint_Base
{
    #region AngularDOF_Impl
    private class AngularDOF_Impl : AngularDOF
    {
        private readonly RotationalJoint_Base rjb;
        public AngularDOF_Impl(RotationalJoint_Base rjb)
        {
            this.rjb = rjb;
        }
        public float currentPosition
        {
            get
            {
                return rjb.currentAngularPosition;
            }
        }

        public float upperLimit
        {
            get
            {
                rjb.enforceOrder();
                return rjb.hasAngularLimit ? rjb.angularLimitHigh : float.PositiveInfinity;
            }
            set
            {
                rjb.angularLimitHigh = value;
            }
        }

        public float lowerLimit
        {
            get
            {
                rjb.enforceOrder();
                return rjb.hasAngularLimit ? rjb.angularLimitLow : float.NegativeInfinity;
            }
            set
            {
                rjb.angularLimitLow = value;
            }
        }

        public BXDVector3 rotationAxis
        {
            get
            {
                return rjb.axis;
            }
            set
            {
                rjb.axis = value;
            }
        }

        public BXDVector3 basePoint
        {
            get
            {
                return rjb.basePoint;
            }
            set
            {
                rjb.basePoint = value;
            }
        }
    }
    #endregion

    public BXDVector3 axis;
    public BXDVector3 basePoint;

    public float currentAngularPosition;
    public bool hasAngularLimit;
    public float angularLimitLow;
    public float angularLimitHigh;

    private readonly AngularDOF[] angularDOF;

    public RotationalJoint_Base()
    {
        angularDOF = new AngularDOF[] { new AngularDOF_Impl(this) };
    }

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.ROTATIONAL;
    }

    protected override void WriteJointInternal(System.IO.BinaryWriter writer)
    {
        enforceOrder();
        writer.Write(basePoint);
        writer.Write(axis);

        writer.Write((byte)((hasAngularLimit ? 1 : 0)));
        if (hasAngularLimit)
        {
            writer.Write(angularLimitLow);
            writer.Write(angularLimitHigh);
        }

        writer.Write(currentAngularPosition);
    }

    protected override void ReadJointInternal(System.IO.BinaryReader reader)
    {
        basePoint = reader.ReadRWObject<BXDVector3>();
        axis = reader.ReadRWObject<BXDVector3>();

        hasAngularLimit = (reader.ReadByte() & 1) == 1;
        if (hasAngularLimit)
        {
            angularLimitLow = reader.ReadSingle();
            angularLimitHigh = reader.ReadSingle();
        }

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
    }

    public override IEnumerable<AngularDOF> GetAngularDOF()
    {
        return angularDOF;
    }

    public override IEnumerable<LinearDOF> GetLinearDOF()
    {
        return new LinearDOF[0];
    }
}