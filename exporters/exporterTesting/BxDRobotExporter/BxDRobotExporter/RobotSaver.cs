using System;
using System.Collections.Generic;
using Inventor;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using BxDRobotExporter;

namespace ExportProcess {
    public class RobotSaver {
        #region State Variables
        private TempReader tempReader;
        private TempWriter tempWriter;
        private JointResolver jointResolver;
        private Inventor.Application currentApplication;
        private List<byte> fileData = new List<byte>();
        private List<JointData> jointDataList = new List<JointData>();
        #endregion
        public RobotSaver(Inventor.Application currentApplication, ArrayList jDataList) {
            foreach (JointData joint in jDataList) {
                jointDataList.Add(joint);
            }
            tempReader = new TempReader((AssemblyDocument)currentApplication.ActiveDocument);
            tempWriter = new TempWriter(currentApplication, ((AssemblyDocument)currentApplication.ActiveDocument).Thumbnail);
            jointResolver = new JointResolver(currentApplication, tempReader.GetSTLDict(), jointDataList);
            this.currentApplication = currentApplication;

        }
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Conversion " + (((bool)e.Result) ? "successful." : "failed."));
        }
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            e.Result = Manager();
        }
        private bool Manager() {
            try {
                tempWriter.Save();
                byte[] fileBytes;
                fileBytes = tempReader.ReadFiles();
                foreach (byte fileSec in fileBytes)
                {
                    fileData.Add(fileSec);
                }
                byte[] jointBytes;
                jointBytes = jointResolver.readJoints();
                if (jointBytes != null)
                {
                    foreach (byte jointSec in jointResolver.readJoints())
                    {
                        fileData.Add(jointSec);
                    }
                }

                Assembler();
                return true;
            }
            catch(Exception e) {
                MessageBox.Show(e.Message + e.StackTrace);
                return false;
            }
        }

        private void Assembler() {
            using (BinaryWriter robotWriter = new BinaryWriter(new FileStream("C:\\Users\\" + System.Environment.UserName + "\\Documents\\" + 
                currentApplication.ActiveDocument.DisplayName.Substring(0, currentApplication.ActiveDocument.DisplayName.Length-3) + "robot", FileMode.Append))) {
                foreach (byte fileSection in fileData) {
                    robotWriter.Write(fileSection);
                }
            }
        }
        public void BeginExport() {
            BackgroundWorker converter = new BackgroundWorker();
            converter.DoWork += BackgroundWorker_DoWork;
            converter.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            if (!converter.IsBusy) converter.RunWorkerAsync();
        }
    }
}
