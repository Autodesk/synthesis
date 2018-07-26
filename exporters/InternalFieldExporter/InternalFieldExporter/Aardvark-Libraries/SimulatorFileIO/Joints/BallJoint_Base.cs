using System;
using System.Collections.Generic;

public class BallJoint_Base : SkeletalJoint_Base
{
    #region AngularDOF_Impl
    private class AngularDOF_Impl : AngularDOF
    {
        private readonly BallJoint_Base bjb;
        private readonly int axis;
        public AngularDOF_Impl(BallJoint_Base bjb, int axis)
        {
            this.bjb = bjb;
            this.axis = axis;
        }

        public float currentPosition
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float upperLimit
        {
            get
            {
                return float.PositiveInfinity;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public float lowerLimit
        {
            get
            {
                return float.NegativeInfinity;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public BXDVector3 rotationAxis
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public BXDVector3 basePoint
        {
            get
            {
                return bjb.basePoint;
            }
            set
            {
                bjb.basePoint = value;
            }
        }
    }
    #endregion

    public BXDVector3 basePoint;

    private readonly AngularDOF[] angularDOF;

    public BallJoint_Base()
    {
        angularDOF = new AngularDOF[] { new AngularDOF_Impl(this, 0), new AngularDOF_Impl(this, 1), new AngularDOF_Impl(this, 2) };
    }

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.BALL;
    }

    protected override void WriteBinaryJointInternal(System.IO.BinaryWriter writer)
    {
        writer.Write(basePoint);
    }

    protected override void ReadBinaryJointInternal(System.IO.BinaryReader reader)
    {
        basePoint = reader.ReadRWObject<BXDVector3>();
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