using System;
using System.Collections.Generic;
using Inventor;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using BxDFieldExporter;

namespace ExportProcess
{
    public class FieldSaver
    {
        #region State Variables
        private TempReader tempReader;
        private TempWriter tempWriter;
        private JointResolver jointResolver;
        private Inventor.Application currentApplication;
        private List<byte> fileData = new List<byte>();
        private InvAddIn.LoadingForm loadAnimation = new InvAddIn.LoadingForm();
        private readonly byte majVersion = 1, minVersion = 0, patVersion = 0, intVersion = 0;
        private byte[] majVersionBytes, minVersionBytes, patVersionBytes, intVersionBytes;
        private readonly string filePath = "C:\\Users\\" + System.Environment.UserName + "\\Documents\\Synthesis\\Fields\\";
        #endregion
        /// <summary>
        /// Constructor for the field saver object which will begin the export process
        /// </summary>
        /// <param name="currentApplication"></param>
        /// <param name="fieldDataList"></param>
        public FieldSaver(Inventor.Application currentApplication, ArrayList fieldDataList)
        {
            try
            {
                if (System.IO.File.Exists(filePath + currentApplication.ActiveDocument.DisplayName.Substring
                    (0, currentApplication.ActiveDocument.DisplayName.Length - 3) + "field"))
                {
                    DialogResult dialogResult = MessageBox.Show("This file already exists, would you like to replace it?", "Error", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes) System.IO.File.Delete(filePath + currentApplication.ActiveDocument.DisplayName.Substring
                      (0, currentApplication.ActiveDocument.DisplayName.Length - 3) + "field");
                    if (dialogResult == DialogResult.No) throw new Exception();
                }
                majVersionBytes = BitConverter.GetBytes(majVersion);
                minVersionBytes = BitConverter.GetBytes(minVersion);
                patVersionBytes = BitConverter.GetBytes(patVersion);
                intVersionBytes = BitConverter.GetBytes(intVersion);
                tempReader = new TempReader((AssemblyDocument)currentApplication.ActiveDocument, fieldDataList);
                tempWriter = new TempWriter(currentApplication, (currentApplication.ActiveDocument).Thumbnail, fieldDataList);
                this.currentApplication = currentApplication;
            }
            catch (Exception e)
            {
                return;
            }
        }
 
        private bool Manager()
        {
            try
            {
                List<byte> versionBytes = new List<byte>();
                versionBytes.AddRange(majVersionBytes);
                versionBytes.AddRange(minVersionBytes);
                versionBytes.AddRange(patVersionBytes);
                versionBytes.AddRange(intVersionBytes);
                fileData.AddRange(versionBytes);
                for (int bit = 0; bit != 72; bit++)
                {
                    fileData.Add(BitConverter.GetBytes(' ')[0]);
                }
                tempWriter.Save();
                jointResolver = new JointResolver(currentApplication, tempReader.GetSTLDict());
                fileData.AddRange(tempReader.ReadFiles());
                byte[] jointBytes;
                jointBytes = jointResolver.ReadJoints();
                if (jointBytes != null) fileData.AddRange(jointBytes);
                Assembler();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
                return false;
            }
        }

        private void Assembler()
        {
            try
            {
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                using (BinaryWriter fieldWriter = new BinaryWriter(new FileStream(filePath +
                    currentApplication.ActiveDocument.DisplayName.Substring(0, currentApplication.ActiveDocument.DisplayName.Length - 3) + "field", FileMode.Append)))
                {
                    foreach (byte fileSection in fileData)
                    {
                        fieldWriter.Write(fileSection);
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void BeginExport()
        {
            MessageBox.Show("Conversion " + ((Manager()) ? "successful." : "failed."));
        }
    }
}
