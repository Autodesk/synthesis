using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ExportProcess
{
    public class JointResolver
    {
        #region State Variables
        private Inventor.Application currentApplication;
        private AssemblyDocument currentDocument;
        private Dictionary<string, STLData> STLDictionary;
        #endregion
        public JointResolver(Inventor.Application passedApplication, Dictionary<string, STLData> STLDictionaryIn)
        {
            try
            {
                currentApplication = passedApplication;
                STLDictionary = STLDictionaryIn;
                currentDocument = (AssemblyDocument)currentApplication.ActiveDocument;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public byte[] ReadJoints()
        {
            try
            {
                List<byte> jointBytes = new List<byte>();
                NameValueMap nameMap = currentApplication.TransientObjects.CreateNameValueMap();
                nameMap.Add("DoubleBearing", false);
                RigidBodyResults jointsContainer = currentDocument.ComponentDefinition.RigidBodyAnalysis(nameMap);
                RigidBodyJoints jointList = jointsContainer.RigidBodyJoints;

                foreach (RigidBodyJoint rigidJoint in jointList)
                {
                    foreach (AssemblyJoint assemblyJoint in rigidJoint.Joints)
                    {
                        if (assemblyJoint.Definition.JointType.ToString().Equals("kSlideJoint")) jointBytes.AddRange(ProcessLinearJoint(rigidJoint));
                        if (assemblyJoint.Definition.JointType.ToString().Equals("kRotationalJointType")) jointBytes.AddRange(ProcessRotationalJoint(rigidJoint));
                        if (assemblyJoint.Definition.JointType.ToString().Equals("kCylindricalJointType")) jointBytes.AddRange(ProcessCylindricalJoint(rigidJoint));
                        if (assemblyJoint.Definition.JointType.ToString().Equals("kPlanarJointType")) jointBytes.AddRange(ProcessPlanarJoint(rigidJoint));
                        if (assemblyJoint.Definition.JointType.ToString().Equals("kBallJointType")) jointBytes.AddRange(ProcessBallJoint(rigidJoint));
                    }
                }
                //Adds ID of joint section and the size of the section if there are joints in the model, to avoid excess data in the file.
                if (jointBytes.Count > 0)
                {
                    jointBytes.AddRange(BitConverter.GetBytes(0003));
                    jointBytes.InsertRange(4, BitConverter.GetBytes(jointBytes.Count - 4));
                }
                return jointBytes.ToArray();
            }
            catch (Exception e)
            {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;

            }
        }
        private byte[] ProcessRotationalJoint(RigidBodyJoint rotationalJoint)
        {
            try
            {
                List<byte> rotationalJointBytes = new List<byte>();

                //Adds ID to signify that it's a rotational joint
                ushort ID = 0;
                byte[] rotationalJointID = BitConverter.GetBytes(ID);
                rotationalJointBytes.AddRange(rotationalJointID);

                foreach (AssemblyJoint joint in rotationalJoint.Joints)
                {
                    //Adds the ID of the parent STL to the output array
                    STLData parentSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceOne.Name), out parentSTL);
                    byte[] parentIDBytes = BitConverter.GetBytes((uint)(parentSTL.getID()));
                    rotationalJointBytes.AddRange(parentIDBytes);
                    //Adds the ID of the child STL to the output array
                    STLData childSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceTwo.Name), out childSTL);
                    byte[] childIDBytes = BitConverter.GetBytes((uint)(childSTL.getID()));
                    rotationalJointBytes.AddRange(childIDBytes);
                    //Adds the vector normal to the plane of rotation onto the array of bytes
                    rotationalJoint.GetJointData(out object GeometryOne, out object GeometryTwo, out NameValueMap nameMap);
                    Circle vectorCircle = (Circle)GeometryTwo;
                    byte[] vectorJointData = new byte[12];
                    BitConverter.GetBytes((float)vectorCircle.Center.X).CopyTo(vectorJointData, 0);
                    BitConverter.GetBytes((float)vectorCircle.Center.Y).CopyTo(vectorJointData, 4);
                    BitConverter.GetBytes((float)vectorCircle.Center.Z).CopyTo(vectorJointData, 8);
                    rotationalJointBytes.AddRange(vectorJointData);
                    //Adds the point of axis relative to the parent part
                    byte[] transJointData = new byte[12];
                    AssemblyJointDefinition jointData = joint.Definition;
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.X).CopyTo(transJointData, 0);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Y).CopyTo(transJointData, 4);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Z).CopyTo(transJointData, 8);
                    rotationalJointBytes.AddRange(transJointData);
                    //Adds the degrees of freedom 
                    rotationalJointBytes.AddRange(BitConverter.GetBytes(jointData.HasAngularPositionLimits));
                    List<byte> freedomData = new List<byte>();
                    ModelParameter positionData = (ModelParameter)jointData.AngularPosition;
                    double relataivePosition = positionData._Value;
                    if (jointData.HasAngularPositionLimits)
                    {
                        positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                        freedomData.AddRange(BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                        positionData = (ModelParameter)jointData.AngularPositionStartLimit;
                        freedomData.AddRange(BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                        rotationalJointBytes.AddRange(freedomData);
                    }
                }
                return rotationalJointBytes.ToArray();
            }
            catch (Exception e)
            {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;

            }
        }
        private byte[] ProcessLinearJoint(RigidBodyJoint linearJoint)
        {
            try
            {
                List<byte> linearJointBytes = new List<byte>();

                //Adds ID to signify that it's a linear joint
                ushort ID = 1;
                byte[] linearJointID = BitConverter.GetBytes(ID);
                linearJointBytes.AddRange(linearJointID);

                foreach (AssemblyJoint joint in linearJoint.Joints)
                {
                    //Adds the ID of the parent STL to the output array
                    STLData parentSTL = new STLData();
                    string name = joint.OccurrenceOne.Name;
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceOne.Name), out parentSTL);
                    byte[] parentIDBytes = BitConverter.GetBytes((uint)(parentSTL.getID()));
                    linearJointBytes.AddRange(parentIDBytes);
                    //Adds the ID of the child STL to the output array
                    STLData childSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceTwo.Name), out childSTL);
                    byte[] childIDBytes = BitConverter.GetBytes((uint)(childSTL.getID()));
                    linearJointBytes.AddRange(childIDBytes);
                    //Adds the vector parallel to the movement onto the array of bytes
                    linearJoint.GetJointData(out object GeometryOne, out object GeometryTwo, out NameValueMap nameMap);
                    Line vectorLine = (Line)GeometryTwo;
                    byte[] vectorJointData = new byte[12];
                    BitConverter.GetBytes((float)vectorLine.RootPoint.X).CopyTo(vectorJointData, 0);
                    BitConverter.GetBytes((float)vectorLine.RootPoint.Y).CopyTo(vectorJointData, 4);
                    BitConverter.GetBytes((float)vectorLine.RootPoint.Z).CopyTo(vectorJointData, 8);
                    linearJointBytes.AddRange(vectorJointData);
                    //Adds the point of connection relative to the parent part
                    byte[] transJointData = new byte[12];
                    AssemblyJointDefinition jointData = joint.Definition;
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.X).CopyTo(transJointData, 0);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Y).CopyTo(transJointData, 4);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Z).CopyTo(transJointData, 8);
                    linearJointBytes.AddRange(transJointData);
                    //Adds the degrees of freedom 
                    List<byte> freedomData = new List<byte>();
                    ModelParameter positionData = (ModelParameter)jointData.LinearPosition;
                    double relataivePosition = positionData._Value;
                    if (jointData.HasLinearPositionEndLimit)
                    {
                        positionData = (ModelParameter)jointData.LinearPositionEndLimit;
                        freedomData.AddRange(BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                    }
                    if (jointData.HasLinearPositionStartLimit)
                    {
                        positionData = (ModelParameter)jointData.LinearPositionStartLimit;
                        freedomData.AddRange(BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                    }
                    if (jointData.HasLinearPositionStartLimit || jointData.HasLinearPositionEndLimit) linearJointBytes.AddRange(freedomData);
                }
                return linearJointBytes.ToArray();
            }
            catch (Exception e)
            {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;

            }
        }
        private byte[] ProcessCylindricalJoint(RigidBodyJoint cylindricalJoint)
        {

            try
            {
                List<byte> cylindricalJointBytes = new List<byte>();

                //Adds ID to signify that it's a cylindrical joint
                cylindricalJointBytes.AddRange(BitConverter.GetBytes((ushort)2));

                foreach (AssemblyJoint joint in cylindricalJoint.Joints)
                {
                    //Adds the ID of the parent STL to the output array
                    STLData parentSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceOne.Name), out parentSTL);
                    byte[] parentIDBytes = BitConverter.GetBytes((uint)(parentSTL.getID()));
                    cylindricalJointBytes.AddRange(parentIDBytes);
                    //Adds the ID of the child STL to the output array
                    STLData childSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceTwo.Name), out childSTL);
                    byte[] childIDBytes = BitConverter.GetBytes((uint)(childSTL.getID()));
                    cylindricalJointBytes.AddRange(childIDBytes);
                    //Adds the vector normal to the plane of rotation
                    cylindricalJoint.GetJointData(out object GeometryOne, out object GeometryTwo, out NameValueMap nameMap);
                    /* Circle vectorCircle = (Circle)GeometryTwo;
                     byte[] vectorJointData = new byte[12];
                     BitConverter.GetBytes((float)vectorCircle.Center.X).CopyTo(vectorJointData, 0);
                     BitConverter.GetBytes((float)vectorCircle.Center.Y).CopyTo(vectorJointData, 4);
                     BitConverter.GetBytes((float)vectorCircle.Center.Z).CopyTo(vectorJointData, 8);
                     cylindricalJointBytes.AddRange(vectorJointData);
                     *///Adds the vector parallel to the movement
                    MessageBox.Show(GeometryTwo.GetType().ToString());
                    //Adds the vector parallel to movement
                    Line vectorLine = (Line)GeometryTwo;
                    byte[] vectorJointData = new byte[12];
                    BitConverter.GetBytes((float)vectorLine.RootPoint.X).CopyTo(vectorJointData, 0);
                    BitConverter.GetBytes((float)vectorLine.RootPoint.Y).CopyTo(vectorJointData, 4);
                    BitConverter.GetBytes((float)vectorLine.RootPoint.Z).CopyTo(vectorJointData, 8);
                    cylindricalJointBytes.AddRange(vectorJointData);
                    //Adds the point of connection relative to the parent part
                    byte[] transJointData = new byte[12];
                    AssemblyJointDefinition jointData = joint.Definition;
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.X).CopyTo(transJointData, 0);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Y).CopyTo(transJointData, 4);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Z).CopyTo(transJointData, 8);
                    cylindricalJointBytes.AddRange(transJointData);
                    //Adds the degrees of freedom 
                    List<byte> freedomData = new List<byte>();
                    ModelParameter positionData = (ModelParameter)jointData.AngularPosition;
                    double relataivePosition = positionData._Value;
                    if (jointData.HasLinearPositionEndLimit)
                    {
                        positionData = (ModelParameter)jointData.LinearPositionEndLimit;
                        freedomData.AddRange(BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                    }
                    if (jointData.HasLinearPositionStartLimit)
                    {
                        positionData = (ModelParameter)jointData.LinearPositionStartLimit;
                        freedomData.AddRange(BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                    }
                    if (jointData.HasAngularPositionLimits)
                    {
                        positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                        freedomData.AddRange(BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                        positionData = (ModelParameter)jointData.AngularPositionStartLimit;
                        freedomData.AddRange(BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                    }
                    if (freedomData != null) cylindricalJointBytes.AddRange(freedomData);
                }
                return cylindricalJointBytes.ToArray();
            }
            catch (Exception e)
            {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;
                
            }
        }
        private byte[] ProcessPlanarJoint(RigidBodyJoint planarJoint)
        {
            try
            {
                List<byte> planarJointBytes = new List<byte>();

                //Adds ID to signify that it's a rotational joint
                ushort ID = 3;
                byte[] rotationalJointID = BitConverter.GetBytes(ID);
                planarJointBytes.AddRange(rotationalJointID);

                foreach (AssemblyJoint joint in planarJoint.Joints)
                {
                    //Adds the ID of the parent STL to the output array
                    STLData parentSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceOne.Name), out parentSTL);
                    byte[] parentIDBytes = BitConverter.GetBytes((uint)(parentSTL.getID()));
                    planarJointBytes.AddRange(parentIDBytes);
                    //Adds the ID of the child STL to the output array
                    STLData childSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceTwo.Name), out childSTL);
                    byte[] childIDBytes = BitConverter.GetBytes((uint)(childSTL.getID()));
                    planarJointBytes.AddRange(childIDBytes);
                    //Adds the vector parallel to the movement onto the array of bytes
                    planarJoint.GetJointData(out object GeometryOne, out object GeometryTwo, out NameValueMap nameMap);
                    Circle vectorCircle = (Circle)GeometryTwo;
                    byte[] vectorJointData = new byte[12];
                    BitConverter.GetBytes((float)vectorCircle.Center.X).CopyTo(vectorJointData, 0);
                    BitConverter.GetBytes((float)vectorCircle.Center.Y).CopyTo(vectorJointData, 4);
                    BitConverter.GetBytes((float)vectorCircle.Center.Z).CopyTo(vectorJointData, 8);
                    planarJointBytes.AddRange(vectorJointData);
                    //Adds the point of connection relative to the parent part
                    byte[] transJointData = new byte[12];
                    AssemblyJointDefinition jointData = joint.Definition;
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.X).CopyTo(transJointData, 0);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Y).CopyTo(transJointData, 4);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Z).CopyTo(transJointData, 8);
                    planarJointBytes.AddRange(transJointData);
                    //Adds the degrees of freedom 
                    byte[] freedomData = new byte[8];
                    ModelParameter positionData = (ModelParameter)jointData.AngularPosition;
                    double relataivePosition = positionData._Value;
                    if (jointData.HasLinearPositionEndLimit)
                    {
                        positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                        BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))).CopyTo(freedomData, 0);
                    }
                    positionData = (ModelParameter)jointData.AngularPositionStartLimit;
                    BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))).CopyTo(freedomData, 4);
                    planarJointBytes.AddRange(freedomData);
                }
                return planarJointBytes.ToArray();
            }
            catch (Exception e)
            {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;

            }
        }
        private byte[] ProcessBallJoint(RigidBodyJoint ballJoint)
        {
            try
            {
                List<byte> ballJointBytes = new List<byte>();

                //Adds ID to signify that it's a rotational joint
                ushort ID = 4;
                byte[] ballJointID = BitConverter.GetBytes(ID);
                ballJointBytes.AddRange(ballJointID);

                foreach (AssemblyJoint joint in ballJoint.Joints)
                {
                    //Adds the ID of the parent STL to the output array
                    STLData parentSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceOne.Name), out parentSTL);
                    byte[] parentIDBytes = BitConverter.GetBytes((uint)(parentSTL.getID()));
                    ballJointBytes.AddRange(parentIDBytes);
                    //Adds the ID of the child STL to the output array
                    STLData childSTL = new STLData();
                    STLDictionary.TryGetValue(NameFilter(joint.OccurrenceTwo.Name), out childSTL);
                    byte[] childIDBytes = BitConverter.GetBytes((uint)(childSTL.getID()));
                    ballJointBytes.AddRange(childIDBytes);
                    //Adds the vector parallel to the movement onto the array of bytes
                    ballJoint.GetJointData(out object GeometryOne, out object GeometryTwo, out NameValueMap nameMap);
                    Circle vectorCircle = (Circle)GeometryTwo;
                    byte[] vectorJointData = new byte[12];
                    BitConverter.GetBytes((float)vectorCircle.Center.X).CopyTo(vectorJointData, 0);
                    BitConverter.GetBytes((float)vectorCircle.Center.Y).CopyTo(vectorJointData, 4);
                    BitConverter.GetBytes((float)vectorCircle.Center.Z).CopyTo(vectorJointData, 8);
                    ballJointBytes.AddRange(vectorJointData);
                    //Adds the point of connection relative to the parent part
                    byte[] transJointData = new byte[12];
                    AssemblyJointDefinition jointData = joint.Definition;
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.X).CopyTo(transJointData, 0);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Y).CopyTo(transJointData, 4);
                    BitConverter.GetBytes((float)jointData.OriginTwo.Point.Z).CopyTo(transJointData, 8);
                    ballJointBytes.AddRange(transJointData);
                    //Adds the degrees of freedom 
                    byte[] freedomData = new byte[8];
                    ModelParameter positionData = (ModelParameter)jointData.AngularPosition;
                    double relataivePosition = positionData._Value;
                    if (jointData.HasLinearPositionEndLimit)
                    {
                        positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                        BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))).CopyTo(freedomData, 0);
                    }
                    positionData = (ModelParameter)jointData.AngularPositionStartLimit;
                    BitConverter.GetBytes((float)(Math.Abs(relataivePosition) - Math.Abs(positionData._Value))).CopyTo(freedomData, 4);
                    ballJointBytes.AddRange(freedomData);

                }
                return ballJointBytes.ToArray();
            }
            catch (Exception e)
            {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
                return null;

            }
        }
        private string NameFilter(string name)
        {
            //each line removes an invalid character from the file name 
            name = name.Replace("\\", "");
            name = name.Replace("/", "");
            name = name.Replace("*", "");
            name = name.Replace("?", "");
            name = name.Replace("\"", "");
            name = name.Replace("<", "");
            name = name.Replace(">", "");
            name = name.Replace("|", "");
            name = name.Replace(":", "");
            return name;
        }
    }
}
