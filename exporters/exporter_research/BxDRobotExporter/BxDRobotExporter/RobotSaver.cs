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
            foreach (JointData joint in jointDataList) {
                jointDataList.Add(joint);
            }
            tempReader = new TempReader((AssemblyDocument)currentApplication.ActiveDocument);
            tempWriter = new TempWriter(currentApplication, ((AssemblyDocument)currentApplication.ActiveDocument).Thumbnail);
            jointResolver = new JointResolver(currentApplication, tempReader.getSTLDict(), jointDataList);
            this.currentApplication = currentApplication;

        }
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Conversion " + (((bool)e.Result) ? "successful." : "failed."));
        }
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            e.Result = Manager();
        }
        private bool Manager() {
            try {
                tempWriter.Save();
                foreach(byte fileSec in tempReader.readFiles()) {
                    fileData.Add(fileSec);
                }
                foreach (byte jointSec in jointResolver.readJoints()) {
                    fileData.Add(jointSec);
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
            using (BinaryWriter robotWriter = new BinaryWriter(new FileStream("C:\\Users\\" + System.Environment.UserName + "\\AppData\\Roaming\\Autodesk\\Synthesis\\" + 
                currentApplication.ActiveDocument.DisplayName.Substring(0, currentApplication.ActiveDocument.DisplayName.Length-3) + ".robot", FileMode.Append))) {
                foreach (byte fileSection in fileData) {
                    robotWriter.Write(fileSection);
                }
            }
        }
        public void beginExport() {
            BackgroundWorker converter = new BackgroundWorker();
            converter.DoWork += backgroundWorker_DoWork;
            converter.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            if (!converter.IsBusy) converter.RunWorkerAsync();
        }
    }
}
