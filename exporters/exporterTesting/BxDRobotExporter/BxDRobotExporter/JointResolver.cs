using BxDRobotExporter;
using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ExportProcess {
    public class JointResolver {
        #region State Variables
        private Inventor.Application currentApplication;
        private AssemblyDocument currentDocument;
        private Dictionary<string, STLData> STLDictionary;
        private List<JointData> jointDataList;
        #endregion
        public JointResolver(Inventor.Application passedApplication, Dictionary<string, STLData> STLDictionaryIn, List<JointData> jDataList) {
            try {
                currentApplication = passedApplication;
                STLDictionary = STLDictionaryIn;
                currentDocument = (AssemblyDocument)currentApplication.ActiveDocument;
                jointDataList = jDataList;
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        public byte[] readJoints() {
            try {
                List<byte> jointBytes = new List<byte>();
                NameValueMap nameMap = currentApplication.TransientObjects.CreateNameValueMap();
                nameMap.Add("DoubleBearing", false);
                RigidBodyResults jointsContainer = currentDocument.ComponentDefinition.RigidBodyAnalysis(nameMap);
                RigidBodyJoints jointList = jointsContainer.RigidBodyJoints;
                JointData jointDataObject = null;
                foreach (RigidBodyJoint joint in jointList) {
                    foreach (AssemblyJoint assemblyJoint in joint.Joints) {
                        foreach (byte byteID in BitConverter.GetBytes(0003)) {
                            jointBytes.Add(byteID);
                        }
                        foreach (JointData jointData in jointDataList) {
                            if (jointData.jointOfType.OccurrenceOne.Name.Equals(assemblyJoint.OccurrenceOne.Name) && jointData.jointOfType.OccurrenceTwo.Name.Equals(assemblyJoint.OccurrenceTwo.Name)) {
                                jointDataObject = jointData;
                            }
                        }
                        if (joint.JointType.Equals("kSlideJointType")) {
                            foreach (byte byteJID in BitConverter.GetBytes((ushort)0000)) {
                                jointBytes.Add(byteJID);
                            }
                            foreach (byte jointSection in ProcessLinearJoint(joint, jointDataObject)) {
                                jointBytes.Add(jointSection);
                            }
                        }
                        if (joint.JointType.Equals("kRotationalJointType")) {
                            foreach (byte byteJID in BitConverter.GetBytes((ushort)0001)) {
                                jointBytes.Add(byteJID);
                            }
                            foreach (byte jointSection in ProcessRotationalJoint(joint, jointDataObject)) {
                                jointBytes.Add(jointSection);
                            }
                        }
                    }
                }
                return jointBytes.ToArray();
            }
            catch (Exception e) {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;

            }
        }
        private byte[] ProcessRotationalJoint(RigidBodyJoint linearJoint, JointData jointDataObject) {
            try {
                ArrayList rotationalJointBytes = new ArrayList();

                //Adds ID to signify that it's a rotational joint
                ushort ID = 0;
                byte[] rotationalJointID = BitConverter.GetBytes(ID);
                rotationalJointBytes.Add(rotationalJointID);

                foreach (AssemblyJoint joint in linearJoint.Joints) {
                    //Adds the ID of the parent STL to the output array
                    STLData parentSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceOne.Name), out parentSTL);
                    byte[] parentIDBytes = BitConverter.GetBytes((uint)(parentSTL.getID()));
                    rotationalJointBytes.Add(parentIDBytes);
                    //Adds the ID of the child STL to the output array
                    STLData childSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceTwo.Name), out childSTL);
                    byte[] childIDBytes = BitConverter.GetBytes((uint)(childSTL.getID()));
                    rotationalJointBytes.Add(childIDBytes);
                    //Adds the vector parallel to the movement onto the array of bytes
                    object GeometryOne, GeometryTwo;
                    NameValueMap nameMap;
                    linearJoint.GetJointData(out GeometryOne, out GeometryTwo, out nameMap);
                    Circle vectorCircle = (Circle)GeometryTwo;
                    byte[] vectorJointData = new byte[12];
                    BitConverter.GetBytes(vectorCircle.Center.X).CopyTo(vectorJointData, 0);
                    BitConverter.GetBytes(vectorCircle.Center.Y).CopyTo(vectorJointData, 4);
                    BitConverter.GetBytes(vectorCircle.Center.Z).CopyTo(vectorJointData, 8);
                    rotationalJointBytes.Add(vectorJointData);
                    //Adds the point of connection relative to the parent part
                    byte[] transJointData = new byte[12];
                    AssemblyJointDefinition jointData = joint.Definition;
                    BitConverter.GetBytes(jointData.OriginTwo.Point.X).CopyTo(transJointData, 0);
                    BitConverter.GetBytes(jointData.OriginTwo.Point.Y).CopyTo(transJointData, 4);
                    BitConverter.GetBytes(jointData.OriginTwo.Point.Z).CopyTo(transJointData, 8);
                    rotationalJointBytes.Add(transJointData);
                    //Adds the degrees of freedom 
                    byte[] freedomData = new byte[8];
                    ModelParameter positionData = (ModelParameter)jointData.AngularPosition;
                    double relataivePosition = positionData._Value;
                    positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                    BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))).CopyTo(freedomData, 0);
                    positionData = (ModelParameter)jointData.AngularPositionStartLimit;
                    BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))).CopyTo(freedomData, 4);
                    rotationalJointBytes.Add(freedomData);
                    rotationalJointBytes.Add(processJointAttributes(jointDataObject));
                }
                object[] tempArray = rotationalJointBytes.ToArray();
                byte[] returnedArray = new byte[tempArray.Length];
                tempArray.CopyTo(returnedArray, 0);
                return returnedArray;
            }
            catch (Exception e) {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;

            }
        }
        private byte[] ProcessLinearJoint(RigidBodyJoint linearJoint, JointData jointDataObject) {
            try {
                ArrayList linearJointBytes = new ArrayList();

                //Adds ID to signify that it's a linear joint
                ushort ID = 1;
                byte[] linearJointID = BitConverter.GetBytes(ID);
                linearJointBytes.Add(linearJointID);

                foreach (AssemblyJoint joint in linearJoint.Joints) {
                    //Adds the ID of the parent STL to the output array
                    STLData parentSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceOne.Name), out parentSTL);
                    byte[] parentIDBytes = BitConverter.GetBytes((uint)(parentSTL.getID()));
                    linearJointBytes.Add(parentIDBytes);
                    //Adds the ID of the child STL to the output array
                    STLData childSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceTwo.Name), out childSTL);
                    byte[] childIDBytes = BitConverter.GetBytes((uint)(childSTL.getID()));
                    linearJointBytes.Add(childIDBytes);
                    //Adds the vector parallel to the movement onto the array of bytes
                    object GeometryOne, GeometryTwo;
                    NameValueMap nameMap;
                    linearJoint.GetJointData(out GeometryOne, out GeometryTwo, out nameMap);
                    Line vectorLine = (Line)GeometryTwo;
                    byte[] vectorJointData = new byte[12];
                    BitConverter.GetBytes(vectorLine.RootPoint.X).CopyTo(vectorJointData, 0);
                    BitConverter.GetBytes(vectorLine.RootPoint.Y).CopyTo(vectorJointData, 4);
                    BitConverter.GetBytes(vectorLine.RootPoint.Z).CopyTo(vectorJointData, 8);
                    linearJointBytes.Add(vectorJointData);
                    //Adds the point of connection relative to the parent part
                    byte[] transJointData = new byte[12];
                    AssemblyJointDefinition jointData = joint.Definition;
                    BitConverter.GetBytes(jointData.OriginTwo.Point.X).CopyTo(transJointData, 0);
                    BitConverter.GetBytes(jointData.OriginTwo.Point.Y).CopyTo(transJointData, 4);
                    BitConverter.GetBytes(jointData.OriginTwo.Point.Z).CopyTo(transJointData, 8);
                    linearJointBytes.Add(transJointData);
                    //Adds the degrees of freedom 
                    byte[] freedomData = new byte[8];
                    ModelParameter positionData = (ModelParameter)jointData.LinearPosition;
                    double relataivePosition = positionData._Value;
                    positionData = (ModelParameter)jointData.LinearPositionEndLimit;
                    BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))).CopyTo(freedomData, 0);
                    positionData = (ModelParameter)jointData.LinearPositionStartLimit;
                    BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))).CopyTo(freedomData, 4);
                    linearJointBytes.Add(freedomData);
                    linearJointBytes.Add(processJointAttributes(jointDataObject));
                }
                object[] tempArray = linearJointBytes.ToArray();
                byte[] returnedArray = new byte[tempArray.Length];
                tempArray.CopyTo(returnedArray, 0);
                return returnedArray;
            }
            catch (Exception e) {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;

            }
        }
        private byte[] processJointAttributes(JointData jointData) {
            List<byte> jointAttributeBytes = new List<byte>();
            foreach (byte byteID in BitConverter.GetBytes((uint)0004)) {
                jointAttributeBytes.Add(byteID);
            }
            switch ((int)jointData.Driver) {
                case 0:
                    foreach (byte IDByte in BitConverter.GetBytes((UInt16)0000)) {
                        jointAttributeBytes.Add(IDByte);
                    }

                    break;
                case 1:
                    foreach (byte IDByte in BitConverter.GetBytes((UInt16)0001)) {
                        jointAttributeBytes.Add(IDByte);
                    }
                    foreach (byte PWMByte in BitConverter.GetBytes(jointData.PWM)) {
                        jointAttributeBytes.Add(PWMByte);
                    }
                    if (jointData.PWM) {
                        foreach (byte Port1Byte in BitConverter.GetBytes(jointData.PWMport)) {
                            jointAttributeBytes.Add(Port1Byte);
                        }
                    }
                    else {
                        foreach (byte Port1Byte in BitConverter.GetBytes(jointData.CANport)) {
                            jointAttributeBytes.Add(Port1Byte);
                        }
                    }
                    foreach (byte fricBoolByte in BitConverter.GetBytes(jointData.HasJointFriction)) {
                        jointAttributeBytes.Add(fricBoolByte);
                    }
                    if (jointData.HasJointFriction) {
                        foreach (byte fricByte in BitConverter.GetBytes((UInt16)jointData.Friction)) {
                            jointAttributeBytes.Add(fricByte);
                        }
                    }
                    foreach (byte driveBoolByte in BitConverter.GetBytes(jointData.DriveWheel)) {
                        jointAttributeBytes.Add(driveBoolByte);
                    }

                    foreach (byte driveByte in BitConverter.GetBytes((UInt16)jointData.Driver)) {
                        jointAttributeBytes.Add(driveByte);
                    }
                    foreach (byte inGearByte in BitConverter.GetBytes((float)jointData.InputGear)) {
                        jointAttributeBytes.Add(inGearByte);
                    }
                    foreach (byte outGearByte in BitConverter.GetBytes((float)jointData.OutputGear)) {
                        jointAttributeBytes.Add(outGearByte);
                    }

                    break;
                case 2:
                    foreach (byte IDByte in BitConverter.GetBytes((UInt16)0002)) {
                        jointAttributeBytes.Add(IDByte);
                    }
                    foreach (byte Port1Byte in BitConverter.GetBytes(jointData.CANport)) {
                        jointAttributeBytes.Add(Port1Byte);
                    }
                    foreach (byte fricBoolByte in BitConverter.GetBytes(jointData.HasJointFriction)) {
                        jointAttributeBytes.Add(fricBoolByte);
                    }
                    if (jointData.HasJointFriction) {
                        foreach (byte fricByte in BitConverter.GetBytes((UInt16)jointData.Friction)) {
                            jointAttributeBytes.Add(fricByte);
                        }
                    }
                    break;
                case 3:
                    foreach(byte Port1 in BitConverter.GetBytes((float)jointData.SolenoidPortA)) {
                        jointAttributeBytes.Add(Port1);
                    }
                    foreach (byte Port2 in BitConverter.GetBytes((float)jointData.SolenoidPortB)) {
                        jointAttributeBytes.Add(Port2);
                    }
                    foreach (byte IDByte in BitConverter.GetBytes((UInt16)0003)) {
                        jointAttributeBytes.Add(IDByte);
                    }
                    if (jointData.HasJointFriction) {
                        foreach (byte fricByte in BitConverter.GetBytes((UInt16)jointData.Friction)) {
                            jointAttributeBytes.Add(fricByte);
                        }
                    }
                    break;
                case 4:
                    foreach (byte IDByte in BitConverter.GetBytes((UInt16)0004)) {
                        jointAttributeBytes.Add(IDByte);
                    }
                    if (jointData.HasJointFriction) {
                        foreach (byte fricByte in BitConverter.GetBytes((UInt16)jointData.Friction)) {
                            jointAttributeBytes.Add(fricByte);
                        }
                    }
                    break;
                case 5:
                    foreach (byte IDByte in BitConverter.GetBytes((UInt16)0005)) {
                        jointAttributeBytes.Add(IDByte);
                    }
                    foreach (byte PWMByte in BitConverter.GetBytes(jointData.PWM)) {
                        jointAttributeBytes.Add(PWMByte);
                    }
                    if (jointData.PWM) {
                        foreach (byte Port1Byte in BitConverter.GetBytes(jointData.PWMport)) {
                            jointAttributeBytes.Add(Port1Byte);
                        }
                    }
                    else {
                        foreach (byte Port1Byte in BitConverter.GetBytes(jointData.CANport)) {
                            jointAttributeBytes.Add(Port1Byte);
                        }
                    }
                    if (jointData.HasJointFriction) {
                        foreach (byte fricByte in BitConverter.GetBytes((UInt16)jointData.Friction)) {
                            jointAttributeBytes.Add(fricByte);
                        }
                    }
                    break;
                case 6:
                    foreach (byte IDByte in BitConverter.GetBytes((UInt16)0006)) {
                        jointAttributeBytes.Add(IDByte);
                    }
                    foreach (byte PWMByte in BitConverter.GetBytes(jointData.PWM)) {
                        jointAttributeBytes.Add(PWMByte);
                    }
                    if (jointData.PWM) {
                        foreach (byte Port1Byte in BitConverter.GetBytes(jointData.PWMport)) {
                            jointAttributeBytes.Add(Port1Byte);
                        }
                        foreach (byte Port2Byte in BitConverter.GetBytes(jointData.PWMport2)) {
                            jointAttributeBytes.Add(Port2Byte);
                        }
                    }
                    else {
                        foreach (byte Port1Byte in BitConverter.GetBytes(jointData.CANport)) {
                            jointAttributeBytes.Add(Port1Byte);
                        }
                        foreach (byte Port2Byte in BitConverter.GetBytes(jointData.CANport2)) {
                            jointAttributeBytes.Add(Port2Byte);
                        }
                    }
                    if (jointData.HasJointFriction) {
                        foreach (byte fricByte in BitConverter.GetBytes((UInt16)jointData.Friction)) {
                            jointAttributeBytes.Add(fricByte);
                        }
                    }
                    foreach (byte driveBoolByte in BitConverter.GetBytes(jointData.DriveWheel)) {
                        jointAttributeBytes.Add(driveBoolByte);
                    }

                    foreach (byte driveByte in BitConverter.GetBytes((UInt16)jointData.Driver)) {
                        jointAttributeBytes.Add(driveByte);
                    }

                    foreach (byte inGearByte in BitConverter.GetBytes((float)jointData.InputGear)) {
                        jointAttributeBytes.Add(inGearByte);
                    }
                    foreach (byte outGearByte in BitConverter.GetBytes((float)jointData.OutputGear)) {
                        jointAttributeBytes.Add(outGearByte);
                    }
                    break;
                case 7:
                    foreach (byte IDByte in BitConverter.GetBytes((UInt16)0007)) {
                        jointAttributeBytes.Add(IDByte);
                    }
                    foreach (byte PWMByte in BitConverter.GetBytes(jointData.PWM)) {
                        jointAttributeBytes.Add(PWMByte);
                    }
                    if (jointData.PWM) {
                        foreach (byte Port1Byte in BitConverter.GetBytes(jointData.PWMport)) {
                            jointAttributeBytes.Add(Port1Byte);
                        }
                    }
                    else {
                        foreach (byte Port1Byte in BitConverter.GetBytes(jointData.CANport)) {
                            jointAttributeBytes.Add(Port1Byte);
                        }
                    }
                    if (jointData.HasJointFriction) {
                        foreach (byte fricByte in BitConverter.GetBytes((UInt16)jointData.Friction)) {
                            jointAttributeBytes.Add(fricByte);
                        }
                    }
                    foreach(byte brakeBoolByte in BitConverter.GetBytes(jointData.HasBrake)) {
                        jointAttributeBytes.Add(brakeBoolByte);
                    }
                    if (jointData.HasBrake) {
                        foreach (byte brake1Byte in BitConverter.GetBytes((float)jointData.BrakePortA)) {
                            jointAttributeBytes.Add(brake1Byte);
                        }
                        foreach (byte brake2Byte in BitConverter.GetBytes((float)jointData.BrakePortB)) {
                            jointAttributeBytes.Add(brake2Byte);
                        }
                    }
                    foreach (byte inGearByte in BitConverter.GetBytes((float)jointData.InputGear)) {
                        jointAttributeBytes.Add(inGearByte);
                    }
                    foreach (byte outGearByte in BitConverter.GetBytes((float)jointData.OutputGear)) {
                        jointAttributeBytes.Add(outGearByte);
                    }
                    break;
            }

            return jointAttributeBytes.ToArray();
        }
        private string NameFilter(string name) {
            //each line removes an invalid character from the file name 
            name = name.Replace("\\", "");
            name = name.Replace("/", "");
            name = name.Replace("*", "");
            name = name.Replace("?", "");
            name = name.Replace("\"", "");
            name = name.Replace("<", "");
            name = name.Replace(">", "");
            name = name.Replace("|", "");
            return name;
        }
    }
}
