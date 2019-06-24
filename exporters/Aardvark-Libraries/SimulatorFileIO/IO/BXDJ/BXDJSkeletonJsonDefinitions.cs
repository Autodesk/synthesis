using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BXDSim.IO.BXDJ
{
    public class JsonSkeleton
    {
        public string Version;
        public RigidNode_Base.DriveTrainType DriveTrainType;
        public string SoftwareExportedWith;
        public List<JsonSkeletonNode> Nodes = new List<JsonSkeletonNode>();
    }


    public class JsonSkeletonNode
    {
        public string GUID;
        public string ParentID;
        public string ModelFileName;
        public string ModelID;
        
        public SkeletalJoint_Base joint;

    }


    // JSON Redefintions (Not Nessary as most internal classes are "json-friendly")

    /*
    public class JsonSkeletonJoint_Base
    {
        public string type = "BaseJoint";
        public BXDVector3 basePoint;
    }
    public class JsonSkeletonJoint_Ball : JsonSkeletonJoint_Base
    {
        // Same as parent
    }

    public class JsonSkeletonJoint_Cylindrical : JsonSkeletonJoint_Base
    {
        public BXDVector3 Axis;
        public string AngularLowLimit, AngularHighLimit;
        public string LinearStartLimit;
        public string LinearEndLimit;
        public string CurrentLinearPosition;
        public string CurrentAngularPosition;

    }

    public class JsonSkeletonJoint_Linear : JsonSkeletonJoint_Base
    {
        public BXDVector3 Axis;
        public string LinearLowLimit;
        public string LinearUpperLimit;
        public string CurrentLinearPosition;
    }

    public class JsonSkeletonJoint_Planar : JsonSkeletonJoint_Base
    {
        public BXDVector3 normal;
    }
    public class JsonSkeletonJoint_Rotational : JsonSkeletonJoint_Base
    {
        public BXDVector3 Axis;
        public string AngularLowLimit, AngularHighLimit;
        public string CurrentAngularPosition;
    }

    public class JsonSkeletonJointDriver
    {
        public string DriverType;
        public string MotorType;
        public string Port1;
        public string Port2;
        public string InputGear;
        public string OutputGear;
        public string LowerLimit;
        public string UpperLimit;
        public string SignalType;
        public string HasBreak;
        public List<JsonSkeletonDriverMeta_Base> Meta = new List<JsonSkeletonDriverMeta_Base>();
    }

    public class JsonSkeletonDriverMeta_Base
    {
        public string DriverMetaID;
    }

    public class JsonSkeletonDriverMeta_Elevator : JsonSkeletonDriverMeta_Base
    {
        public string ElevatorType;
    }
    public class JsonSkeletonDriverMeta_Pnuematic : JsonSkeletonDriverMeta_Base
    {
        public double WidthMM;
        public double PressurePSI;

    }
    public class JsonSkeletonDriverMeta_Wheel : JsonSkeletonDriverMeta_Base
    {
        public string WheelType;
        public string WheelRadius;
        public BXDVector3 WheelCenter;
        public string ForwardAsympSlip;
        public string ForwardAsympValue;
        public string ForwardExtremeSlip;
        public string ForwardExtremeValue;
        public string SideAsympSlip;
        public string SideAsympValue;
        public bool IsDriveWheel;
    }

    public class JsonSkeletonSensor
    {
        public string SensorType;
        public int SensorPortNumberA;
        public string SignalTypeA;
        public int SensorPortNumberB;
        public string SignalTypeB;
        public double SensorConversionFactor;
    }
    */
}
