using System;

namespace BxDRobotExporter.Utilities.Synthesis
{
    public static class JointToStringUtils
    {
        public static string SensorCountString(SkeletalJoint_Base joint)
        {
            return joint.attachedSensors.Count.ToString();
        }

        public static string WheelTypeString(SkeletalJoint_Base joint)
        {
            WheelDriverMeta wheelData = null;
            if (joint.cDriver != null)
            {
                wheelData = joint.cDriver.GetInfo<WheelDriverMeta>();
            }

            return wheelData != null ? wheelData.GetTypeString() + ", " + WheelFrictionString(wheelData) + " Friction" : "No Wheel";
        }

        public static string WheelFrictionString(WheelDriverMeta wheel)
        {
            switch (wheel.GetFrictionLevel())
            {
                case FrictionLevel.LOW:
                    return "Low";
                case FrictionLevel.MEDIUM:
                    return "Medium";
                case FrictionLevel.HIGH:
                    return "High";
                default:
                    return "None";
            }
        }

        public static string DriverString(SkeletalJoint_Base joint)
        {
            return joint.cDriver != null ? joint.cDriver.ToString() + (joint.cDriver.port1 > 2 ? "" : ", " + DriveTrainSideString(joint) + " Drivetrain") : "No Driver";
        }

        public static string DriveTrainSideString(SkeletalJoint_Base joint)
        {
            switch (joint.cDriver.port1)
            {
                case 0:
                    return "Right";
                case 1:
                    return "Left";
                case 2:
                    return "Other";
                default:
                    return "None";
            }
        }

        public static string NodeNameString(RigidNode_Base node)
        {
            return InventorDocumentIoUtils.CapitalizeFirstLetter(node.ModelFileName.Replace('_', ' ').Replace(".bxda", ""));
        }

        public static string ParentNameString(RigidNode_Base node)
        {
            return InventorDocumentIoUtils.CapitalizeFirstLetter(node.GetParent().ModelFileName.Replace('_', ' ').Replace(".bxda", ""));
        }

        public static string JointTypeString(SkeletalJoint_Base joint, RobotData robotData = null)
        {
            return InventorDocumentIoUtils.CapitalizeFirstLetter(Enum.GetName(typeof(SkeletalJointType), joint.GetJointType()), true)
                   + (joint.weight <= 0 ? ", Calculated Weight" : ", " + Math.Round(Math.Max(joint.weight, 0) * (robotData != null && robotData.RMeta.PreferMetric ? 1 : 2.20462f), 2)+(robotData != null && robotData.RMeta.PreferMetric ? " Kilograms" : " Pounds"));
        }
    }
}