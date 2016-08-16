using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Inventor;
using System.Collections.Generic;

namespace BxDFieldExporter {
    public partial class pathInput {
        #region State Variables
        private readonly string FieldFormatVer = "303730303736303638303636303332303439303436303438303436303438303436303438303438303438303438";
        private string folderPath, destinationPath;
        private bool usedID;
        private int IDIndex;
        private List<FieldDataType> fieldTypes;
        private List<string> IDList;
        private List<int> IDOccurances;
        private AssemblyJoints listOfJoints;
        private AssemblyDocument CurrentProject;
        private bool writeFilesBreak = false;

        #endregion

        private bool SetUpFileReader(List<FieldDataType> fieldTypes) {
            try {
                listOfJoints = CurrentProject.ComponentDefinition.Joints;
                folderPath = "C:\\Users\\" + System.Environment.UserName + "\\AppData\\Roaming\\Autodesk\\Synthesis";

                this.fieldTypes = fieldTypes;
                destinationPath = folderPath + "\\Test.FIELD";
                IDList = new List<string>();
                IDOccurances = new List<int>();
                if (System.IO.File.Exists(destinationPath)) System.IO.File.Delete(destinationPath);
                if (Directory.Exists(folderPath)) return readFiles(Directory.GetFiles(folderPath));
                else return (false);
            }
            catch (Exception e) {
                MessageBox.Show("does it go all the way up?");
                MessageBox.Show(e.Message);
                return false;
            }
        }
        #region .
        /*
         * |||||||||||||||
         * |||||||||||||||  The man that
         * |||||| O ||||||  was freed 
         * ||||||\|/||||||  from access    
         * |||||| | ||||||  bugs
         * ||||||/ \||||||
         */
        #endregion
        private bool readFiles(string[] fileList) {

            try {
                writeFiles();//Writes header 
                writeFiles(fileList.Length); //and amount of field elements
                raiseProgress();
                foreach (FieldDataType fieldType in fieldTypes) writeFiles(fieldType);
                raiseProgress();
                foreach (string file in fileList) {

                    int length = System.IO.File.ReadAllBytes(file).Length;
                    writeFiles(file.Substring(file.LastIndexOf("\\"), (file.LastIndexOf(".") - file.LastIndexOf("\\"))));
                    using (BinaryReader fileRead = new BinaryReader(System.IO.File.Open(file, FileMode.Open))) {
                        for (int linesRead = 0; linesRead < length;) {
                            if (linesRead < 80) {
                                writeFiles(fileRead.ReadByte());
                                linesRead++;
                            }
                            else if (linesRead == 80) {
                                writeFiles(fileRead.ReadUInt32());
                                linesRead += 4;
                            }
                            else if ((linesRead - 81) % 13 == 0) {
                                writeFiles(fileRead.ReadUInt16());
                                linesRead += 2;
                            }
                            else {
                                writeFiles(fileRead.ReadInt32());
                                linesRead += 4;
                            }
                        }
                    }
                }
                raiseProgress();
                writeFiles(listOfJoints); //Writes joint data
                raiseProgress();
                //  writeFiles(CurrentProject.ComponentDefinition.Occurrences);
                raiseProgress();
                raiseProgress();
                return !writeFilesBreak;
            }
            catch (Exception e) {
                MessageBox.Show("Why");
                MessageBox.Show(e.Message);
                return false;
            }
        }

        #region writeFiles Overloads
        //Only to be used to write header
        private void writeFiles() {
            try {
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    //Translates to FLDB 1.0.0.0000
                    //FLDB 1.0.0.0000 = Field Binary Ver 1.0.0
                    fileWrote.Write(Encoding.Default.GetBytes(FieldFormatVer));
                    for (int bytes = 45; bytes < 80; bytes++) {
                        fileWrote.Write(20);//Fills rest of 80 byte header with blank space//
                    }
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                writeFilesBreak = true;
            }
        }
        //Used for both number of elements, and Int32 calls
        private void writeFiles(int elements) {
            try {
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    fileWrote.Write(elements);
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                writeFilesBreak = true;
            }
        }
        private void writeFiles(FieldDataType fieldData) {
            try {
                MarkWithZero();
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    fileWrote.Write(Encoding.Default.GetBytes(fieldData.Name));
                }
                MarkWithZero();
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    fileWrote.Write(fieldData.compOcc.Count);
                    //Needs implementation of field elements within field type
                    fileWrote.Write(fieldData.Dynamic);
                    fileWrote.Write(fieldData.Mass);
                    fileWrote.Write(fieldData.Friction);
                    fileWrote.Write(((int)fieldData.colliderType == 0) ? true : false);
                    fileWrote.Write(((int)fieldData.colliderType == 1) ? true : false);
                    fileWrote.Write(((int)fieldData.colliderType == 2) ? true : false);
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                writeFilesBreak = true;
            }
        }
        private void writeFiles(byte STLSection) {
            try {
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    fileWrote.Write(STLSection);
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }
        private void writeFiles(string Id) {
            try {
                MarkWithZero();
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {

                    usedID = false;
                    foreach (string SavedID in IDList) {
                        if (SavedID.Equals(Id)) {
                            usedID = true;
                            IDIndex = IDList.IndexOf(SavedID);
                            break;
                        }
                    }
                    if (!usedID) {
                        IDList.Add(Id);
                        IDOccurances.Add(0);
                    }
                    else {
                        int tempValue = IDOccurances[IDIndex];
                        IDOccurances.Remove(IDIndex);
                        IDOccurances.Insert(IDIndex, tempValue + 1);
                    }
                    byte[] IDBytes = Encoding.Default.GetBytes(Id);
                    foreach (byte IDSection in IDBytes) {
                        fileWrote.Write(IDSection);
                    }
                    fileWrote.Write(Encoding.Default.GetBytes(":+6" + IDOccurances[IDIndex].ToString()));
                }
                MarkWithZero();
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }

        private void writeFiles(ushort STLSection) {
            try {
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    fileWrote.Write(STLSection);
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }
        private void writeFiles(uint STLSection) {
            try {
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    fileWrote.Write(STLSection);
                }
            }
            catch (Exception e) {
                MessageBox.Show("Wait what");
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }
        private void writeFiles(AssemblyJoints jointList) {

            try {
                foreach (AssemblyJoint joint in jointList) {                    
                    AssemblyJointDefinition jointData = joint.Definition;                    
                    if (jointData.JointType.ToString().Equals("kRotationalJointType")) {
                        if (!jointData.HasAngularPositionLimits) {
                            MessageBox.Show("The joint between " + joint.OccurrenceOne.Name + " and " + joint.OccurrenceTwo.Name
                                + "has no limits on its rotation.", "Unrestricted joint", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            writeFilesBreak = true;
                            break;
                        }
                        using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                            fileWrote.Write(true);
                        }
                        MarkWithZero();
                        using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                            fileWrote.Write(Encoding.Default.GetBytes(joint.OccurrenceOne.Name));
                        }
                        MarkWithZero();
                        MarkWithZero();
                        using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                            fileWrote.Write(Encoding.Default.GetBytes(joint.OccurrenceTwo.Name));
                        }
                        MarkWithZero();
                        using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                            fileWrote.Write((float)(joint.OccurrenceOne.Transformation.Translation.X + jointData.OriginOne.Point.X));
                            fileWrote.Write((float)(joint.OccurrenceOne.Transformation.Translation.Y + jointData.OriginOne.Point.Y));
                            fileWrote.Write((float)(joint.OccurrenceOne.Transformation.Translation.Z + jointData.OriginOne.Point.Z));
                            ModelParameter positionData = (ModelParameter)jointData.AngularPosition;
                            double relataivePosition = positionData._Value;
                            positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                            fileWrote.Write((float)(relataivePosition - positionData._Value));
                            positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                            fileWrote.Write((float)(relataivePosition - positionData._Value));
                            fileWrote.Write(false);
                        }
                    }
                    else if (jointData.JointType.ToString().Equals("kSlideJointType")) {
                        MessageBox.Show("Seciton 1");
                        if (!jointData.HasLinearPositionEndLimit || !jointData.HasLinearPositionStartLimit) {
                            MessageBox.Show("The joint between " + joint.OccurrenceOne.Name + " and " + joint.OccurrenceTwo.Name
                                + "has no limits on its movement.", "Unrestricted joint",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            writeFilesBreak = true;
                            break;
                        }
                        MessageBox.Show("Seciton 2");
                        using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                            fileWrote.Write(true);
                            fileWrote.Write(false);
                        }
                        MarkWithZero();
                        MessageBox.Show("Seciton 3");
                        using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                            fileWrote.Write(Encoding.Default.GetBytes(joint.OccurrenceOne.Name));
                        }
                        MarkWithZero();
                        MarkWithZero();
                        MessageBox.Show("Seciton 4");
                        using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                            fileWrote.Write(Encoding.Default.GetBytes(joint.OccurrenceTwo.Name));
                        }
                        MarkWithZero();
                        MessageBox.Show("Seciton 5");
                        using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                            fileWrote.Write((float)(joint.OccurrenceOne.Transformation.Translation.X + jointData.OriginOne.Point.X));
                            fileWrote.Write((float)(joint.OccurrenceOne.Transformation.Translation.Y + jointData.OriginOne.Point.Y));
                            fileWrote.Write((float)(joint.OccurrenceOne.Transformation.Translation.Z + jointData.OriginOne.Point.Z));
                            ModelParameter positionData = (ModelParameter)jointData.LinearPosition;
                            double relataivePosition = positionData._Value;
                            positionData = (ModelParameter)jointData.LinearPositionEndLimit;
                            fileWrote.Write((float)((Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                            positionData = (ModelParameter)jointData.LinearPositionStartLimit;
                            fileWrote.Write((float)((Math.Abs(relataivePosition) - Math.Abs(positionData._Value))));
                            Point OccurenceOneOrigin;
                            Point OccurenceTwoOrigin;
                            MessageBox.Show("Seciton 6");
                            foreach (WorkPoint OccurrenceOneJoint in joint.OccurrenceOne.Parent.WorkPoints) {
                                if (OccurrenceOneJoint.Name.Equals("Center Point")) OccurenceOneOrigin = OccurrenceOneJoint.Point;
                                MessageBox.Show(OccurrenceOneJoint.Name);
                            }
                            foreach (WorkPoint OccurrenceTwoJoint in joint.OccurrenceTwo.Parent.WorkPoints) { 
                                if (OccurrenceTwoJoint.Name.Equals("Center Point")) OccurenceTwoOrigin = OccurrenceTwoJoint.Point;
                                MessageBox.Show(OccurrenceTwoJoint.Name);
                            }
                           
                         //   MessageBox.Show(joint.OccurrenceOne.Transformation.Translation.X.ToString() + "   " + jointData.OriginOne.Point.X.ToString());
                          //  MessageBox.Show(joint.OccurrenceTwo.Transformation.Translation.X.ToString() + "   " + jointData.OriginTwo.Point.X.ToString());
                            /*   MessageBox.Show("Part : " + joint.OccurrenceOne.Name);
                               MessageBox.Show("Origin 1 X: " + (jointData.OriginOne.Point.X).ToString());
                               MessageBox.Show("Origin 1 Y: " + (jointData.OriginOne.Point.Y).ToString());
                               MessageBox.Show("Origin 1 Z: " + (jointData.OriginOne.Point.Z).ToString());

                               MessageBox.Show("Part : " + joint.OccurrenceTwo.Name);
                               MessageBox.Show("Origin 1 X: " + (jointData.OriginTwo.Point.X).ToString());
                               MessageBox.Show("Origin 1 Y: " + (jointData.OriginTwo.Point.Y).ToString());
                               MessageBox.Show("Origin 1 Z: " + (jointData.OriginTwo.Point.Z).ToString());


                                     MessageBox.Show("Joint: " + joint.OccurrenceOne.Name + " " + joint.OccurrenceTwo.Name);
                               MessageBox.Show("Origin 1 X: " + (jointData. + jointData.OriginOne.Point.X).ToString());
                               MessageBox.Show("Origin 2 X: " + (OccurenceOneOrigin.X + jointData.OriginTwo.Point.X).ToString());
                               MessageBox.Show("Origin 1 Y: " + (OccurenceOneOrigin.Y + jointData.OriginOne.Point.Y).ToString());
                               MessageBox.Show("Origin 2 Y: " + (OccurenceOneOrigin.Y + jointData.OriginTwo.Point.Y).ToString());
                               MessageBox.Show("Origin 1 Z: " + (OccurenceOneOrigin.Z + jointData.OriginOne.Point.Z).ToString());
                               MessageBox.Show("Origin 2 Z: " + (OccurenceOneOrigin.Z + jointData.OriginTwo.Point.Z).ToString());
                              */ // MessageBox.Show((OccurenceOneOrigin.X + jointData.OriginOne.Point.X).Equals(OccurenceTwoOrigin.X + jointData.OriginTwo.Point.X).ToString());*/
                        }
                    }
                    else {
                        MessageBox.Show("Error: Only Rotational and Slide joints allowed.");
                        writeFilesBreak = true;
                        break;
                    }
                }
            }
            catch (Exception e){
                MessageBox.Show(e.Message);
                writeFilesBreak = true;
            }
        }
        #endregion

        private void MarkWithZero() {
            try {
                using (BinaryWriter fileWrote = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    fileWrote.Write(Encoding.Default.GetBytes("00000000"));
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }
    }
}
