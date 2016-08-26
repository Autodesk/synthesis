using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Inventor;
using BxDFieldExporter;
using System.Collections;

namespace ExportProcess {
    public class TempReader {
        #region State Variables
        private int ID;
        private AssemblyDocument currentDocument;
        private List<FieldDataComponent> fieldComponents = new List<FieldDataComponent>();
        //path to file
        private string directoryPath = "C:\\Users\\" + System.Environment.UserName + "\\AppData\\Roaming\\Autodesk\\Synthesis\\";
        #endregion
        public TempReader(AssemblyDocument currentDocument, ArrayList fieldComponents) {
            foreach (FieldDataComponent fieldComponent in fieldComponents) this.fieldComponents.Add(fieldComponent);
            ID = -1;
            this.currentDocument = currentDocument;

        }

        private byte[] readBMP(string fileName) {
            try {
                //path to file
                string file = directoryPath + fileName;
                //byte array that will be returned
                byte[] fileBytes;

                //reads the file into the array
                fileBytes = System.IO.File.ReadAllBytes(file);

                //returns
                return fileBytes;
            }
            catch (Exception e) {
                //catches problems
                MessageBox.Show(e.Message);
            }
            //returns in case of problem
            return null;
        }
        private STLData readSTL(string fileName) {
            try {
                string path = directoryPath + fileName;
                //byte array that will be returned
                byte[] fileBytes;
                //reads the file into the array
                fileBytes = System.IO.File.ReadAllBytes(path);
                //returns
                ID++;
                float[,] trans = new float[4, 4];
                foreach (ComponentOccurrence component in currentDocument.ComponentDefinition.Occurrences) {
                    if (fileName.Equals(NameFilter(component.Name))) {
                        Matrix m = component.Transformation;
                        for (int x = 0; x < 4; x++) {
                            for (int y = 0; y < 4; y++) {
                                trans[x, y] = (float)m.Cell[x, y];
                            }
                        }
                    }
                }
                return new STLData(ID, fileBytes, trans);
            }
            catch (Exception e) {
                //catches problems
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
            }
            //returns in case of problem
            return new STLData();
        }
        private byte[] processFieldData(FieldDataComponent fieldComponent){
            List<byte> fieldDataBytes = new List<byte>();
            switch ((int)fieldComponent.colliderType) {
                case 0:
                    foreach(byte idByte in BitConverter.GetBytes((UInt16)0000)){
                        fieldDataBytes.Add(idByte);
                    }
                    foreach(byte xByte in BitConverter.GetBytes((float)fieldComponent.X)) {
                        fieldDataBytes.Add(xByte);
                    }
                    foreach (byte yByte in BitConverter.GetBytes((float)fieldComponent.Y)) {
                        fieldDataBytes.Add(yByte);
                    }
                    foreach (byte zByte in BitConverter.GetBytes((float)fieldComponent.Z)) {
                        fieldDataBytes.Add(zByte);
                    }
                    foreach (byte fricByte in BitConverter.GetBytes((float)fieldComponent.Friction)) {
                        fieldDataBytes.Add(fricByte);
                    }
                    foreach(byte dynamicByte in BitConverter.GetBytes(fieldComponent.Dynamic)) {
                        fieldDataBytes.Add(dynamicByte);
                    }
                    if (fieldComponent.Dynamic) foreach (byte massByte in BitConverter.GetBytes(fieldComponent.Mass)) fieldDataBytes.Add(massByte);
                    break;
                case 1:
                    foreach (byte idByte in BitConverter.GetBytes((UInt16)0001)){
                        fieldDataBytes.Add(idByte);
                    }
                    foreach (byte scaleByte in BitConverter.GetBytes((float)fieldComponent.Scale)) {
                        fieldDataBytes.Add(scaleByte);
                    }
                    foreach (byte fricByte in BitConverter.GetBytes((float)fieldComponent.Friction)) {
                        fieldDataBytes.Add(fricByte);
                    }
                    foreach (byte dynamicByte in BitConverter.GetBytes(fieldComponent.Dynamic)) {
                        fieldDataBytes.Add(dynamicByte);
                    }
                    if (fieldComponent.Dynamic) foreach (byte massByte in BitConverter.GetBytes(fieldComponent.Mass)) fieldDataBytes.Add(massByte);
                    break;
                case 2:
                    foreach (byte idByte in BitConverter.GetBytes((UInt16)0002)){
                        fieldDataBytes.Add(idByte);
                    }
                    foreach (byte fricByte in BitConverter.GetBytes((float)fieldComponent.Friction)) {
                        fieldDataBytes.Add(fricByte);
                    }
                    foreach (byte dynamicByte in BitConverter.GetBytes(fieldComponent.Dynamic)) {
                        fieldDataBytes.Add(dynamicByte);
                    }
                    if (fieldComponent.Dynamic) foreach (byte massByte in BitConverter.GetBytes(fieldComponent.Mass)) fieldDataBytes.Add(massByte);
                    break;
            }
            return fieldDataBytes.ToArray();
        }
        public byte[] readFiles() {

            List<byte> bytesOfFiles = new List<byte>(), bmpBytes = new List<byte>();
            string updatedFile;
            uint numOfStls = 0;
            foreach (string file in Directory.GetFiles(directoryPath)) {
                updatedFile = file.Substring(file.LastIndexOf("\\"), file.Length-file.Substring(0, file.LastIndexOf("\\")).Length-1);
                if (updatedFile.Contains(".bmp")) {
                    foreach(byte byteID in BitConverter.GetBytes(0000)) {
                        bmpBytes.Add(byteID);
                    }
                    byte[] BMPBytes = readBMP(updatedFile);
                    foreach(byte byteLength in BitConverter.GetBytes(BMPBytes.Length)) {
                        bmpBytes.Add(byteLength);
                    }
                    foreach (byte bmpSec in BMPBytes) {
                        bmpBytes.Add(bmpSec);
                    }
                }
                else if (updatedFile.Contains(".stl")) {
                    numOfStls++;
                    foreach (byte byteID in BitConverter.GetBytes(0001)) {
                        bytesOfFiles.Add(byteID);
                    }    
                    byte[] stlBytes = readSTL(updatedFile).getData();
                    foreach(byte byteLength in BitConverter.GetBytes(stlBytes.Length)) {
                        bytesOfFiles.Add(byteLength);
                    }
                    foreach (byte stlSec in stlBytes) {
                        bytesOfFiles.Add(stlSec);
                    }
                }
                System.IO.File.Delete(file);
            }
            byte[] numOfStlBytes = BitConverter.GetBytes(numOfStls);
            for (int lengthBytes = 0; lengthBytes < numOfStlBytes.Length; lengthBytes++) {
                bytesOfFiles.Insert(lengthBytes, numOfStlBytes[lengthBytes]);
            }
            foreach(byte bmpByte in bmpBytes) {
                bytesOfFiles.Add(bmpByte);
            }
            foreach (FieldDataComponent fieldComponent in fieldComponents) foreach (byte attributeByte in processFieldData(fieldComponent)) bytesOfFiles.Add(attributeByte);
            return bytesOfFiles.ToArray();
        }
        public Dictionary<string, STLData> getSTLDict() {

            Dictionary<string, STLData> output = new Dictionary<string, STLData>();
            string[] paths = Directory.GetFiles(directoryPath);

            foreach (string path in paths) {
                if (path.Contains(".stl")) {
                    string name = path.Substring(path.LastIndexOf("\\") + 1, path.IndexOf(".") - 1 - (path.LastIndexOf("\\")));
                    output.Add(name, readSTL(name + ".stl"));
                }
            }
            return output;
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
