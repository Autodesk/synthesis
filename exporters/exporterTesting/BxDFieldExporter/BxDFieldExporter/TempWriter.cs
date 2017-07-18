using System;
using Inventor;
using System.Windows.Forms;
using System.Drawing;
using BxDFieldExporter;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace ExportProcess {
    public class TempWriter {
        #region State Variables
        private string path;
        private Inventor.Application currentApplication;
        private AssemblyDocument currentDocument;
        private stdole.IPictureDisp thumbnailDisp;
        private List<FieldDataComponent> fieldDataComponents = new List<FieldDataComponent>();
        #endregion
        public TempWriter(Inventor.Application currentApplication, stdole.IPictureDisp thumbnail, ArrayList fieldDataComponents) {
            //The path to the temp directory where temp folders will be saved
            foreach (FieldDataComponent fieldComponent in fieldDataComponents) {
                this.fieldDataComponents.Add(fieldComponent);
            }
            path = "C:\\Users\\" + System.Environment.UserName + "\\AppData\\Roaming\\Autodesk\\Synthesis\\";
            //the active document
            currentDocument = (AssemblyDocument)currentApplication.ActiveDocument;
            this.currentApplication = currentApplication;
            thumbnailDisp = thumbnail;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
        public void Save() {
            SaveBMP();
            SaveSTLs();
        }
        private void SaveSTLs() {
            try {
               
                //Temporary document that holds the components
                Document tempDoc;
                //Interates though all top level components and saves them into seperate STLs

                foreach (ComponentOccurrence component in currentDocument.ComponentDefinition.Occurrences) {
                    //Assigns the tempDoc to the current ComponentOccurrence
                    
                    tempDoc = (Document)component.Definition.Document;
                    //Saves the tempDoc to the temporary directory
                    TranslatorSave(currentApplication, tempDoc, NameFilter(component.Name));
                    foreach (FieldDataComponent fieldComponent in fieldDataComponents)
                    { 
                        foreach (ComponentOccurrence checkedComponent in fieldComponent.compOcc) {
                            if (component.Name.Equals(checkedComponent.Name)) {
                                byte[] stlIn = System.IO.File.ReadAllBytes(path + NameFilter(component.Name) + ".stl");
                                List<byte> tempStl = new List<byte>();
                                foreach (byte stlByte in stlIn) {
                                    tempStl.Add(stlByte);
                                }
                                foreach (byte fieldIDByte in BitConverter.GetBytes(fieldComponent.ID))
                                {
                                    tempStl.Add(fieldIDByte);
                                }
                                Random uint32Gen = new Random();
                                byte[] meshID = BitConverter.GetBytes(uint32Gen.Next(0, int.MaxValue));
                                for (int byteCounter = 0; byteCounter < meshID.Length; byteCounter++)
                                {
                                    tempStl.Insert(byteCounter, meshID[byteCounter]);
                                }
                                using (BinaryWriter stlOut = new BinaryWriter(new FileStream(path + NameFilter(component.Name) + ".stl", FileMode.Create))) {
                                    foreach (byte stlByteOut in tempStl) stlOut.Write(stlByteOut);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
            }
        }
        private void SaveBMP() {
            try {
                System.Drawing.Image thumbnail = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(thumbnailDisp);
                Bitmap b = new Bitmap(thumbnail, new Size(256, 256));
                b.Save(path + "thumb.bmp");
            }
            catch (Exception e) {
                MessageBox.Show(e.Message + "\n\n\n" + e.StackTrace);
            }
        }
        private void TranslatorSave(Inventor.Application application, object saveDataIn, string name) {
            try
            {
                //translator object does all the complicated saving
                TranslatorAddIn tAddIn = (TranslatorAddIn)application.ApplicationAddIns.ItemById["{81CA7D27-2DBE-4058-8188-9136F85FC859}"];

                //the document that we are saving
                object saveData = saveDataIn;

                //translator context holds the IOMechanism, don't worry about it
                TranslationContext context = application.TransientObjects.CreateTranslationContext();
                context.Type = IOMechanismEnum.kFileBrowseIOMechanism;

                //options hold all the options of the save, like quality and file structure
                NameValueMap options = application.TransientObjects.CreateNameValueMap();

                //these options are needed in case the inventor defaults change
                options.Value["ExportFileStructure"] = 0; //make sure all parts are saved as seperate files
                options.Value["Resolution"] = 20; //low resolution save (0 for high, 1 for medium)
                options.Value["ExportColor"] = true; //makes sure color is exported

                //the data medium holds the destination
                DataMedium dataMed = application.TransientObjects.CreateDataMedium();
                dataMed.FileName = path + name + ".stl";
                //calls the save
                tAddIn.SaveCopyAs(saveData, context, options, dataMed);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
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
            name = name.Replace(":", "");
            return name;
        }
    }
}
