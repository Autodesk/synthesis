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
        #endregion
        public JointResolver(Inventor.Application passedApplication, Dictionary<string, STLData> STLDictionaryIn) {
            try {
                currentApplication = passedApplication;
                STLDictionary = STLDictionaryIn;
                currentDocument = (AssemblyDocument)currentApplication.ActiveDocument;
                
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        public byte[] ReadJoints() {
            try {
                List<byte> jointBytes = new List<byte>();
                NameValueMap nameMap = currentApplication.TransientObjects.CreateNameValueMap();
                nameMap.Add("DoubleBearing", false);
                RigidBodyResults jointsContainer = currentDocument.ComponentDefinition.RigidBodyAnalysis(nameMap);
                RigidBodyJoints jointList = jointsContainer.RigidBodyJoints;
            
                foreach (RigidBodyJoint joint in jointList) {
                        foreach (byte byteID in BitConverter.GetBytes(0003)) {
                            jointBytes.Add(byteID);
                        
                        }
                        if (joint.JointType.Equals("kSlideJointType")) {
                            foreach (byte byteJID in BitConverter.GetBytes((ushort)0000)) {
                                jointBytes.Add(byteJID);
                            }
                            foreach (byte jointSection in ProcessLinearJoint(joint)) {
                                jointBytes.Add(jointSection);
                            }
                        }
                        if (joint.JointType.Equals("kRotationalJointType")) {
                            foreach (byte byteJID in BitConverter.GetBytes((ushort)0001)) {
                                jointBytes.Add(byteJID);
                            }
                            foreach (byte jointSection in ProcessRotationalJoint(joint)) {
                                jointBytes.Add(jointSection);  
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
        private byte[] ProcessRotationalJoint(RigidBodyJoint linearJoint) {
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
                    linearJoint.GetJointData(out object GeometryOne, out object GeometryTwo, out NameValueMap nameMap);
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
                    rotationalJointBytes.Add(ProcessJointAttributes());
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
        private byte[] ProcessLinearJoint(RigidBodyJoint linearJoint) {
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
                    linearJoint.GetJointData(out object GeometryOne, out object GeometryTwo, out NameValueMap nameMap);
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
                    linearJointBytes.Add(ProcessJointAttributes());
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
        private byte[] ProcessJointAttributes() {
            List<byte> jointAttributeBytes = new List<byte>();
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
