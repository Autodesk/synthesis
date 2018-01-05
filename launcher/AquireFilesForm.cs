using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;

namespace SynthesisLauncher
{
    public partial class AquireFilesForm : Form
    {
        public AquireFilesForm()
        {
            InitializeComponent();
        }

        int currentFile = 0;
        private void ProgressStep()
        {
            if (InvokeRequired)
                Invoke(new Action(ProgressStep));
            else
            {
                progressBar.PerformStep();
                currentFile++;
            }
        }
        private void UpdateTextDownloading(int progress)
        {
            if (InvokeRequired)
                Invoke(new Action<int>(UpdateTextDownloading), progress);
            else
            {
                progressLabel.Text = fileInfoList[currentFile].name + ": Downloading..." + progress.ToString() + "%";
            }
        }
        private void UpdateTextExtracting()
        {
            if (InvokeRequired)
                Invoke(new Action(UpdateTextExtracting));
            else
            {
                progressLabel.Text = fileInfoList[currentFile].name + ": Extracting";
            }
        }

        private volatile List<ContentDistributor.FileInfo> fileInfoList;
        private volatile WebClient webClient;

        public AquireFilesForm(List<ContentDistributor.FileInfo> fileInfoList, WebClient webClient)
        {
            InitializeComponent();
            this.fileInfoList = fileInfoList;
            this.webClient = webClient;
            downloadWorker.RunWorkerCompleted += DownloadWorker_RunWorkerCompleted;

            progressBar.Maximum = fileInfoList.Count;
            progressBar.Step = 0;

            this.Shown += delegate (object sender, EventArgs e) { downloadWorker.RunWorkerAsync(); };
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressLabel.Text = fileInfoList[currentFile].name + ": Downloading..." + e.ProgressPercentage.ToString() + "%";
        }

        private void DownloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        private void DownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SynthesisTEMP"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SynthesisTEMP");
            var targetDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SynthesisTEMP";
            this.webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

            foreach (ContentDistributor.FileInfo fileInfo in fileInfoList)
            {
                try
                {
                    UpdateTextDownloading(0);
                    webClient.DownloadFile(fileInfo.path, targetDir + "\\" + Path.GetFileName(fileInfo.path));
                    UpdateTextExtracting();
                    if (fileInfo.type == ContentDistributor.FileType.ROBOT)
                    {
                        ZipFile.ExtractToDirectory(targetDir + "\\" + Path.GetFileName(fileInfo.path), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Synthesis\Robots\" + fileInfo.name);
                    }
                    else
                    {
                        ZipFile.ExtractToDirectory(targetDir + "\\" + Path.GetFileName(fileInfo.path), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Synthesis\Fields\" + fileInfo.name);
                    }
                    File.Delete(targetDir + "\\" + Path.GetFileName(fileInfo.path));
                    ProgressStep();
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show(ex.ToString());
#else
                    MessageBox.Show("An error has occurred. Please try again later."); 
#endif
                    throw; 
                }
            }
        }
    }
}