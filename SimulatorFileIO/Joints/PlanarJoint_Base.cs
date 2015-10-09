using System.Collections.Generic;
using System;

/// <summary>
/// Contains a joint with two degrees of linear freedom and one degree of rotational freedom.
/// </summary>
public class PlanarJoint_Base : SkeletalJoint_Base
{
    private static readonly BXDVector3 PLANAR_JOINT_BASIS = new BXDVector3(1.12824E-7, 0.418275, 0.90832);

    #region AngularDOF_Impl
    private class AngularDOF_Impl : AngularDOF
    {
        private readonly PlanarJoint_Base pjb;
        public AngularDOF_Impl(PlanarJoint_Base pjb)
        {
            this.pjb = pjb;
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
                return pjb.normal;
            }
            set
            {
                pjb.normal = value;
            }
        }

        public BXDVector3 basePoint
        {
            get
            {
                return pjb.basePoint;
            }
            set
            {
                pjb.basePoint = value;
            }
        }
    }
    #endregion
    #region LinearDOF_Impl
    private class LinearDOF_Impl : LinearDOF
    {
        private readonly PlanarJoint_Base pjb;
        private readonly int axis;
        public LinearDOF_Impl(PlanarJoint_Base pjb, int axis)
        {
            this.pjb = pjb;
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public BXDVector3 translationalAxis
        {
            get
            {
                BXDVector3 rot = BXDVector3.CrossProduct(PlanarJoint_Base.PLANAR_JOINT_BASIS, pjb.normal);
                if (axis == 1)
                {
                    rot = BXDVector3.CrossProduct(rot, pjb.normal);
                }
                return rot;
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
                return pjb.basePoint;
            }
            set
            {
                pjb.basePoint = value;
            }
        }
    }
    #endregion

    public BXDVector3 normal;
    public BXDVector3 basePoint;

    private readonly AngularDOF[] angularDOF;
    private readonly LinearDOF[] linearDOF;

    public PlanarJoint_Base()
    {
        angularDOF = new AngularDOF[] { new AngularDOF_Impl(this) };
        linearDOF = new LinearDOF[] { new LinearDOF_Impl(this, 0), new LinearDOF_Impl(this, 1) };
    }

    public override SkeletalJointType GetJointType()
    {
        return SkeletalJointType.PLANAR;
    }

    protected override void WriteBinaryJointInternal(System.IO.BinaryWriter writer)
    {
        writer.Write(normal);
        writer.Write(basePoint);
    }

    protected override void ReadBinaryJointInternal(System.IO.BinaryReader reader)
    {
        normal = reader.ReadRWObject<BXDVector3>();
        basePoint = reader.ReadRWObject<BXDVector3>();
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
