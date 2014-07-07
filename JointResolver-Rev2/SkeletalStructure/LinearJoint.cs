using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class LinearJoint : LinearJoint_Base
{
    public SkeletalJoint wrapped;

    public static bool isLinearJoint(CustomRigidJoint jointI)
    {
        if (jointI.joints.Count == 1)
        {
            AssemblyJointDefinition joint = jointI.joints[0].Definition;
            //Cylindrical joints with no rotaion are effectively sliding joints.
            return joint.JointType == AssemblyJointTypeEnum.kSlideJointType 
                || (joint.JointType == AssemblyJointTypeEnum.kCylindricalJointType 
                && joint.HasAngularPositionLimits && joint.AngularPositionStartLimit._Value == joint.AngularPositionEndLimit._Value);
        }
        return false;
    }

    private static void getLinearInfo(dynamic geom, ComponentOccurrence component, out UnitVector groupANormal, out Point groupABase)
    {
        int translationDegrees;
        ObjectsEnumerator translationAxes;
        int rotationDegrees;
        ObjectsEnumerator rotationAxes;
        Point center;
        IEnumerator axesEnumerator;

        component.GetDegreesOfFreedom(out translationDegrees, out translationAxes,
            out rotationDegrees, out rotationAxes, out center);

        if (translationDegrees == 1)
        {
            axesEnumerator = translationAxes.GetEnumerator();
            axesEnumerator.MoveNext();
            groupANormal = ((Vector)axesEnumerator.Current).AsUnitVector();
        }
        else if (translationDegrees == 0)
        {
            groupANormal = null;
        }
        else
        {
            throw new Exception("More than one linear axis of freedom found on linear joint.");
        }

        //Into usual base fiding stuff, will look for a way to change later.
        if (geom is EdgeProxy)
        {
            EdgeProxy edge = (EdgeProxy)geom;
            if (edge.GeometryType == CurveTypeEnum.kCircularArcCurve || edge.GeometryType == CurveTypeEnum.kCircleCurve)
            {
                groupABase = geom.Geometry.Center;
            }
            else if (edge.GeometryType == CurveTypeEnum.kLineSegmentCurve || edge.GeometryType == CurveTypeEnum.kLineCurve)
            {
                groupABase = edge.Geometry.MidPoint;
            }
            else
            {
                throw new Exception("Unimplemented " + Enum.GetName(typeof(CurveTypeEnum), edge.GeometryType));
            }
        }
        else if (geom is FaceProxy)
        {
            FaceProxy face = (FaceProxy)geom;
            Console.WriteLine("FaceType: " + Enum.GetName(typeof(SurfaceTypeEnum), face.SurfaceType));
            if (face.SurfaceType == SurfaceTypeEnum.kPlaneSurface)
            {
                groupABase = face.Geometry.RootPoint;
            }
            else if (face.SurfaceType == SurfaceTypeEnum.kCylinderSurface)
            {
                groupABase = face.Geometry.BasePoint;
            }
            else
            {
                throw new Exception("Unimplemented surface type " + Enum.GetName(typeof(SurfaceTypeEnum), face.SurfaceType));
            }
        }
        else
        {
            throw new Exception("Unimplemented proxy object " + Enum.GetName(typeof(ObjectTypeEnum), geom.Type));
        }
    }

    public LinearJoint(CustomRigidGroup parent, CustomRigidJoint rigidJoint)
    {
        if (!(isLinearJoint(rigidJoint)))
            throw new Exception("Not a linear joint");
        wrapped = new SkeletalJoint(parent, rigidJoint);

        UnitVector groupANormal;
        UnitVector groupBNormal;
        Point groupABase;
        Point groupBBase;

        getLinearInfo(wrapped.asmJoint.OriginOne.Geometry, wrapped.asmJointOccurrence.AffectedOccurrenceOne, out groupANormal, out groupABase);
        getLinearInfo(wrapped.asmJoint.OriginTwo.Geometry, wrapped.asmJointOccurrence.AffectedOccurrenceTwo, out groupBNormal, out groupBBase);

        if (groupABase == null && groupBBase != null)
        {
            groupANormal = groupBNormal;
        }
        else if (groupBBase == null && groupABase != null)
        {
            groupBNormal = groupANormal;
        }
        else if(groupABase == null && groupBBase == null)
        {
            throw new Exception("Both axes for linear movement are null.");
        }

        if (wrapped.childIsTheOne)
        {
            childNormal = Utilities.toBXDVector(groupANormal);
            childBase = Utilities.toBXDVector(groupABase);
            parentNormal = Utilities.toBXDVector(groupBNormal);
            parentBase = Utilities.toBXDVector(groupBBase);
        }
        else
        {
            childNormal = Utilities.toBXDVector(groupBNormal);
            childBase = Utilities.toBXDVector(groupBBase);
            parentNormal = Utilities.toBXDVector(groupANormal);
            parentBase = Utilities.toBXDVector(groupABase);
        }

        currentLinearPosition = !((wrapped.asmJoint.LinearPosition == null)) ? ((float)wrapped.asmJoint.LinearPosition.Value) : 0;
        if (hasUpperLimit = wrapped.asmJoint.HasLinearPositionEndLimit)
        {
            linearLimitHigh = (float) wrapped.asmJoint.LinearPositionEndLimit.Value;
        }
        if (hasLowerLimit = wrapped.asmJoint.HasLinearPositionStartLimit)
        {
            linearLimitLow = (float) wrapped.asmJoint.LinearPositionStartLimit.Value;
        }

        Console.WriteLine("Axis is " + groupANormal.X + ", " + groupANormal.Y + ", " + groupANormal.Z);
    }

    protected override string ToString_Internal()
    {
        return wrapped.childGroup + " translates along " + wrapped.parentGroup;
    }
}