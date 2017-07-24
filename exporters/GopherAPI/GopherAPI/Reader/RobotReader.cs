using System;
using System.Collections.Generic;
using System.IO;
using GopherAPI.Nodes.Joint.Driver;
using GopherAPI.Nodes;

namespace GopherAPI.Reader
{
    internal class RobotReader : GopherReader_Base
    {
        internal RawRobot Robot = new RawRobot();

        /// <summary>
        /// Step 3: Processes the facets of all the RawMeshes and adds the PreProcessed data into an STLMesh object
        /// </summary>
        new internal void ProcessSTL()
        {
            Robot.Meshes = base.ProcessSTL();
        }

        /// <summary>
        /// Step 4: Processes the joint section 
        /// </summary>
        new internal void ProcessJoints()
        {
            Robot.Joints = base.ProcessJoints();
        }

        /// <summary>
        /// Step 5: Processes the Driver/Joint Attribute Section
        /// </summary>
        /// <param name="raw"></param>
        internal void ProcessDrivers(Section raw)
        {
            if (raw.ID != SectionType.JOINT_ATTRIBUTE)
                throw new ArgumentException("ERROR: Non-Driver section passed to ReadDrivers", "raw");
            List<GopherDriver_Base> drivers = new List<GopherDriver_Base>();
            using (var reader = new BinaryReader(new MemoryStream(raw.Data)))
            {
                uint driverCount = reader.ReadUInt32();
                for(uint i = 0; i < driverCount; i++)
                {
                    uint tempJointID = reader.ReadUInt32();
                    Driver dt = (Driver)reader.ReadUInt16();

                    switch (dt)
                    {
                        case Driver.MOTOR:
                            var motor = new Motor
                            {
                                Meta = new DriverMeta(tempJointID),
                                IsCAN = reader.ReadBoolean(),
                                MotorPort = reader.ReadSingle(),
                                HasLimits = reader.ReadBoolean()
                            };
                            if (motor.HasLimits)
                                motor.Friction = (Friction)reader.ReadUInt16();
                            motor.IsDriveWheel = reader.ReadBoolean();
                            motor.WheelType = (Wheel)reader.ReadUInt16();
                            motor.InputGear = reader.ReadUInt16();
                            motor.OutputGear = reader.ReadUInt16();
                            drivers.Add(motor);
                            break;
                        case Driver.SERVO:
                            var servo = new Servo
                            {
                                Meta = new DriverMeta(tempJointID),
                                MotorPort = (float)reader.ReadDecimal(),
                                HasLimits = reader.ReadBoolean()
                            };
                            if(servo.HasLimits)
                                servo.Friction = (Friction)reader.ReadUInt16();
                            drivers.Add(servo);
                            break;
                        case Driver.BUMPER_PNUEMATIC:
                            var bumperP = new BumperPnuematic
                            {
                                Meta = new DriverMeta(tempJointID),
                                SolenoidPortOne = (float)reader.ReadDecimal(),
                                SolenoidPortTwo = (float)reader.ReadDecimal(),
                                HasLimits = reader.ReadBoolean()
                            };
                            if (bumperP.HasLimits)
                                bumperP.Friction = (Friction)reader.ReadUInt16();
                            bumperP.InternalDiameter = (InternalDiameter)reader.ReadUInt16();
                            bumperP.Pressure = (Pressure)reader.ReadUInt16();
                            drivers.Add(bumperP);
                            break;
                        case Driver.RELAY_PNUEMATIC:
                            var relayP = new RelayPnuematic
                            {
                                Meta = new DriverMeta(tempJointID),
                                RelayPort = (float)reader.ReadDecimal(),
                                HasLimits = reader.ReadBoolean()
                            };
                            if (relayP.HasLimits)
                                relayP.Friction = (Friction)reader.ReadUInt16();
                            relayP.InternalDiameter = (InternalDiameter)reader.ReadUInt16();
                            relayP.Pressure = (Pressure)reader.ReadUInt16();
                            drivers.Add(relayP);
                            break;
                        case Driver.WORM_SCREW:
                            var wormScrew = new WormScrew
                            {
                                Meta = new DriverMeta(tempJointID),
                                IsCAN = reader.ReadBoolean(),
                                MotorPort = reader.ReadSingle(),
                                HasLimits = reader.ReadBoolean()
                            };
                            if (wormScrew.HasLimits)
                                wormScrew.Friction = (Friction)reader.ReadUInt16();
                            break;
                        case Driver.DUAL_MOTOR:
                            var dualMotor = new DualMotor
                            {
                                Meta = new DriverMeta(tempJointID),
                                IsCAN = reader.ReadBoolean(),
                                PortOne = reader.ReadSingle(),
                                PortTwo = reader.ReadSingle(),
                                HasLimits = reader.ReadBoolean()
                            };
                            if (dualMotor.HasLimits)
                                dualMotor.Friction = (Friction)reader.ReadUInt16();
                            dualMotor.IsDriveWheel = reader.ReadBoolean();
                            dualMotor.WheelType = (Wheel)reader.ReadUInt16();
                            dualMotor.InputGear = reader.ReadUInt16();
                            dualMotor.OutputGear = reader.ReadUInt16();
                            drivers.Add(dualMotor);
                            break;
                        case Driver.ELEVATOR:
                            var elevator = new Elevator
                            {
                                Meta = new DriverMeta(tempJointID),
                                IsCAN = reader.ReadBoolean(),
                                MotorPort = reader.ReadSingle(),
                                HasLimits = reader.ReadBoolean()
                            };
                            if (elevator.HasLimits)
                                elevator.Friction = (Friction)reader.ReadUInt16();
                            elevator.HasBrake = reader.ReadBoolean();
                            if (elevator.HasBrake)
                            {
                                elevator.BrakePortOne = reader.ReadSingle();
                                elevator.BrakePortTwo = reader.ReadSingle();
                            }
                            elevator.Stages = (Stages)reader.ReadUInt16();
                            elevator.InputGear = reader.ReadUInt32();
                            elevator.OutputGear = reader.ReadUInt32();
                            drivers.Add(elevator);
                            break;
                        default:
                            throw new Exception("ERROR: Bad robot file (thrown in ReadDrivers)");
                    }
                }
            }
            Robot.Drivers = drivers;
        }

        /// <summary>
        /// Step 5: Processes the Driver/Joint Attribute Section
        /// </summary>
        internal void ProcessDrivers()
        {
            foreach(var section in sections)
            {
                if (section.ID == SectionType.JOINT_ATTRIBUTE)
                    ProcessDrivers(section);
            }
        }

        /// <summary>
        /// Initializes the RobotReader class and loads the given file into memory
        /// </summary>
        /// <param name="path"></param>
        internal RobotReader(string path) : base(path)
        {
            if (Path.GetExtension(path).ToLower() != ".robot")
                throw new ArgumentException("ERROR: non robot file passed to RobotReader", "path");
        }
    }
}