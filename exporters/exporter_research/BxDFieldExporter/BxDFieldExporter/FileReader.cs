using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Inventor;
using System.Collections.Generic;
using System.Drawing;

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
        private AssemblyDocument CurrentDocument;
        private bool writeFilesBreak = false;
        private Inventor.Application CurrentApplication;
        Document doc;
        Dictionary<string, float[,]> transformations = new Dictionary<string, float[,]>();
        #endregion

        //Called by pathInput to initalize state variables 
        private bool SetUpFileReader() {
            try {
                listOfJoints = CurrentDocument.ComponentDefinition.Joints;
                folderPath = "C:\\Users\\" + System.Environment.UserName + "\\AppData\\Roaming\\Autodesk\\Synthesis\\";
                destinationPath = folderPath + CurrentDocument.DisplayName + ".FIELD";
                IDList = new List<string>();
                IDOccurances = new List<int>();
                if (System.IO.File.Exists(destinationPath)) System.IO.File.Delete(destinationPath);


                Bitmap thumbnail = new Bitmap(AxHostConverter.PictureDispToImage(CurrentDocument.Thumbnail), new Size(256, 256));

                thumbnail.Save(folderPath + "thumb.bmp");
                foreach (ComponentOccurrence component in CurrentDocument.ComponentDefinition.Occurrences) {     //Xanders section of the export process
                    Matrix transformationMatrix = component.Transformation;
                    float[,] trans = new float[4, 4];
                    for (int x = 0; x < 4; x++) {
                        for (int y = 0; y < 4; y++) {
                            trans[x, y] = (float)transformationMatrix.Cell[x, y];
                        }
                    }

                    doc = (Document)component.Definition.Document;  //Saves the STLS into the directory
                    string name = component.Name;
                    
                    transformations.Add(name, trans);

                    doc.SaveAs(folderPath + name + ".stl", true);
                }
                if (Directory.Exists(folderPath)) return readSTLFiles(Directory.GetFiles(folderPath));
                else return false;
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        //Handles the reading of the STLs on the 
        private bool readSTLFiles(string[] fileList) {
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
                NameValueMap nameMap = CurrentApplication.TransientObjects.CreateNameValueMap();
                nameMap.Add("DoubleBearing", false);
                RigidBodyResults jointsContainer = CurrentDocument.ComponentDefinition.RigidBodyAnalysis(nameMap);
                RigidBodyJoints jointsList = jointsContainer.RigidBodyJoints;
                writeFiles(jointsList); //Writes joint data
                raiseProgress();
                writeFiles(CurrentDocument.ComponentDefinition.WorkPoints);
                raiseProgress();
                raiseProgress();
                return !writeFilesBreak;
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        #region writeFiles Overloads
        //Only to be used to write header
        private void writeFiles() {
            try {
                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    //Translates to FLDB 1.0.0.0000
                    //FLDB 1.0.0.0000 = Field Binary Ver 1.0.0
                    FileWriter.Write(Encoding.Default.GetBytes(FieldFormatVer));
                    for (int bytes = 45; bytes < 80; bytes++) {
                        FileWriter.Write(BitConverter.GetBytes(20));//Fills rest of 80 byte header with blank space//
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
                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    FileWriter.Write(BitConverter.GetBytes(elements));
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
                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    FileWriter.Write(Encoding.Default.GetBytes(fieldData.Name));
                }
                MarkWithZero();
                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    FileWriter.Write(BitConverter.GetBytes(fieldData.compOcc.Count));
                    //Needs implementation of field elements within field type
                    FileWriter.Write(BitConverter.GetBytes(fieldData.Dynamic));
                    FileWriter.Write(BitConverter.GetBytes(fieldData.Mass));
                    FileWriter.Write(BitConverter.GetBytes(fieldData.Friction));
                    FileWriter.Write(BitConverter.GetBytes(fieldData.colliderType == 0 ? true : false));
                    FileWriter.Write(BitConverter.GetBytes(((int)fieldData.colliderType == 1) ? true : false));
                    FileWriter.Write(BitConverter.GetBytes(((int)fieldData.colliderType == 2) ? true : false));
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                writeFilesBreak = true;
            }
        }
        private void writeFiles(byte STLSection) {
            try {
                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    FileWriter.Write(BitConverter.GetBytes(STLSection));
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }
        private void writeFiles(string Id) {
            try {

                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {

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
                        FileWriter.Write(BitConverter.GetBytes(IDSection));
                    }
                    FileWriter.Write(Encoding.Default.GetBytes(":+6" + IDOccurances[IDIndex].ToString()));
                    float[,] translationSection = new float[4, 4];
                    transformations.TryGetValue(Id, out translationSection);
                    for (int x = 0; x < 4; x++) {
                        for (int y = 0; y < 4; y++) {
                            FileWriter.Write(BitConverter.GetBytes(translationSection[x, y]));
                        }
                    }
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }

        private void writeFiles(ushort STLSection) {
            try {
                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    FileWriter.Write(BitConverter.GetBytes(STLSection));
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }
        private void writeFiles(uint STLSection) {
            try {
                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    FileWriter.Write(BitConverter.GetBytes(STLSection));
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }
        private void writeFiles(RigidBodyJoints jointsList) {

            try {
                object GeometryOne, GeometryTwo;
                Line jointLine;
                Circle jointCircle;
                NameValueMap nameMap;
                foreach (RigidBodyJoint rigidJoint in jointsList) {
                    MessageBox.Show(rigidJoint.JointType.ToString());
                    foreach (AssemblyJoint joint in rigidJoint.Joints) {

                        AssemblyJointDefinition jointData = joint.Definition;
                        if (jointData.JointType.ToString().Equals("kRotationalJointType")) {
                            if (!jointData.HasAngularPositionLimits) {
                                MessageBox.Show("The joint between " + joint.OccurrenceOne.Name + " and " + joint.OccurrenceTwo.Name
                                    + "has no limits on its rotation.", "Unrestricted joint", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                writeFilesBreak = true;
                                break;
                            }

                            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                                FileWriter.Write(BitConverter.GetBytes(true));
                            }
                            MarkWithZero();
                            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                                FileWriter.Write(Encoding.Default.GetBytes(joint.OccurrenceOne.Name));
                            }
                            MarkWithZero();
                            MarkWithZero();
                            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                                FileWriter.Write(Encoding.Default.GetBytes(joint.OccurrenceTwo.Name));
                            }
                            MarkWithZero();
                            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {

                                rigidJoint.GetJointData(out GeometryOne, out GeometryTwo, out nameMap);

                                jointCircle = (Circle)GeometryTwo;
                                FileWriter.Write(BitConverter.GetBytes((jointCircle.Center.X)));
                                FileWriter.Write(BitConverter.GetBytes((jointCircle.Center.Y)));
                                FileWriter.Write(BitConverter.GetBytes((jointCircle.Center.Z)));

                                FileWriter.Write(BitConverter.GetBytes((jointData.OriginTwo.Point.X)));
                                FileWriter.Write(BitConverter.GetBytes((jointData.OriginTwo.Point.Y)));
                                FileWriter.Write(BitConverter.GetBytes((jointData.OriginTwo.Point.Z)));
                                ModelParameter positionData = (ModelParameter)jointData.AngularPosition;
                                double relataivePosition = positionData._Value;
                                positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                                FileWriter.Write(BitConverter.GetBytes(((float)(relataivePosition - positionData._Value))));
                                positionData = (ModelParameter)jointData.AngularPositionEndLimit;
                                FileWriter.Write(BitConverter.GetBytes(((float)(relataivePosition - positionData._Value))));
                                FileWriter.Write(BitConverter.GetBytes((false)));
                            }
                        }
                        else if (jointData.JointType.ToString().Equals("kSlideJointType")) {
                            if (!jointData.HasLinearPositionEndLimit || !jointData.HasLinearPositionStartLimit) {
                                MessageBox.Show("The joint between " + joint.OccurrenceOne.Name + " and " + joint.OccurrenceTwo.Name
                                    + "has no limits on its movement.", "Unrestricted joint",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                writeFilesBreak = true;
                                break;
                            }
                            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                                FileWriter.Write(BitConverter.GetBytes((true)));
                                FileWriter.Write(BitConverter.GetBytes((false)));
                            }
                            MarkWithZero();
                            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                                FileWriter.Write(Encoding.Default.GetBytes(joint.OccurrenceOne.Name));
                            }
                            MarkWithZero();
                            MarkWithZero();
                            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                                FileWriter.Write(Encoding.Default.GetBytes(joint.OccurrenceTwo.Name));
                            }
                            MarkWithZero();
                            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                                rigidJoint.GetJointData(out GeometryOne, out GeometryTwo, out nameMap);

                                jointLine = (Line)GeometryTwo;
                                FileWriter.Write(BitConverter.GetBytes((jointLine.RootPoint.X)));
                                FileWriter.Write(BitConverter.GetBytes((jointLine.RootPoint.Y)));
                                FileWriter.Write(BitConverter.GetBytes((jointLine.RootPoint.Z)));
                                FileWriter.Write(BitConverter.GetBytes((jointData.OriginTwo.Point.X)));
                                FileWriter.Write(BitConverter.GetBytes((jointData.OriginTwo.Point.Y))); //Writes the origins of the joints relative to the parent occurence
                                FileWriter.Write(BitConverter.GetBytes((jointData.OriginTwo.Point.Z)));
                                ModelParameter positionData = (ModelParameter)jointData.LinearPosition;
                                double relataivePosition = positionData._Value;
                                positionData = (ModelParameter)jointData.LinearPositionEndLimit;
                                FileWriter.Write(BitConverter.GetBytes((float)((Math.Abs(relataivePosition) - Math.Abs(positionData._Value)))));
                                positionData = (ModelParameter)jointData.LinearPositionStartLimit;
                                FileWriter.Write(BitConverter.GetBytes((float)((Math.Abs(relataivePosition) - Math.Abs(positionData._Value)))));
                            }
                        }




                        else {
                            MessageBox.Show("Error: Only Rotational and Slide joints allowed.");
                            writeFilesBreak = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                writeFilesBreak = true;
            }
        }

        private void writeFiles(WorkPoints workPointList) {
            using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                foreach (WorkPoint spawn in workPointList) {
                    if (spawn.Name.Equals("Start Location")) {
                        FileWriter.Write(BitConverter.GetBytes((spawn.Point.X)));
                        FileWriter.Write(BitConverter.GetBytes((spawn.Point.Y)));
                        FileWriter.Write(BitConverter.GetBytes((spawn.Point.Z)));
                    }
                }
            }
        }
        #endregion

        #region Text Formaters
        private void MarkWithZero() {
            try {
                using (BinaryWriter FileWriter = new BinaryWriter(System.IO.File.Open(destinationPath, FileMode.Append))) {
                    FileWriter.Write(Encoding.Default.GetBytes("00000000"));
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
                writeFilesBreak = true;
            }
        }

        private String FilterOutIllegalChars(string filteredWord) {
            filteredWord = filteredWord.Replace("*", "");
            filteredWord = filteredWord.Replace(".", "");
            filteredWord = filteredWord.Replace("\"", "");
            filteredWord = filteredWord.Replace("/", "");
            filteredWord = filteredWord.Replace("[", "");
            filteredWord = filteredWord.Replace("]", "");
            filteredWord = filteredWord.Replace(":", "");
            filteredWord = filteredWord.Replace(";", "");
            filteredWord = filteredWord.Replace("|", "");
            filteredWord = filteredWord.Replace("=", "");
            filteredWord = filteredWord.Replace(",", "");
            return filteredWord;
        }
        #endregion
    }
}
