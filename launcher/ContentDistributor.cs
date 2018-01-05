using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SynthesisLauncher
{
    public static class ContentDistributor
    {
        public enum FileType { FIELD, ROBOT, OTHER }
        public struct FileInfo
        {
            public FileType type;
            public string path;
            public string name;

            public bool Compare(FileInfo fileInfo)
            {
                return (fileInfo.name == this.name && fileInfo.type == this.type);
            }
        }
        private class FileInfoComparer : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo x, FileInfo y)
            {
                return (x.Compare(y));
            }

            public int GetHashCode(FileInfo obj)
            {
                return obj.name.GetHashCode();
            }
        }

        public static void ListFiles()
        {
            try
            { //Get index of robot/field files from server
                WebClient webClient = new WebClient
                {
                    BaseAddress = "http://bxd.autodesk.com/Downloadables/"
                };
                List<FileInfo> webFiles = new List<FileInfo>();
                using (var reader = XmlReader.Create(webClient.OpenRead("FieldRobotIndex.xml")))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            default:
                                break;
                            case XmlNodeType.Element:
                                FileInfo fileInfo = new FileInfo();

                                if (reader.Name == "fileList")
                                    break;
                                else if (reader.Name == "field")
                                    fileInfo.type = FileType.FIELD;
                                else if (reader.Name == "robot")
                                    fileInfo.type = FileType.ROBOT;
                                else //For if we ever decide to add more assets. This prevents breaking backwards compatability
                                {
                                    fileInfo.type = FileType.OTHER;
                                    break;
                                }

                                reader.MoveToFirstAttribute();
                                if (reader.Name == "path")
                                    fileInfo.path = reader.Value;
                                reader.MoveToNextAttribute();
                                if (reader.Name == "name")
                                    fileInfo.name = reader.Value;
                                webFiles.Add(fileInfo);
                                break;
                        }
                    }
                }

                //Get index of local robot/field files
                List<FileInfo> localFiles = new List<FileInfo>();
                foreach (string fieldFolder in Directory.EnumerateDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\synthesis\fields"))
                {
                    if (File.Exists(fieldFolder + @"\definition.bxdf"))
                    {
                        localFiles.Add(new FileInfo
                        {
                            type = FileType.FIELD,
                            name = fieldFolder.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last()
                        });
                    }
                }
                foreach (string robotFolder in Directory.EnumerateDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\synthesis\robots"))
                {
                    if (File.Exists(robotFolder + @"\skeleton.bxdj"))
                    {
                        localFiles.Add(new FileInfo
                        {
                            type = FileType.ROBOT,
                            name = Path.GetDirectoryName(robotFolder)
                        });
                    }
                }

                var fileInfoComparer = new FileInfoComparer();
                List<FileInfo> getQueue = new List<FileInfo>();
                foreach (FileInfo file in webFiles)
                {
                    if ((file.type == FileType.ROBOT && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Synthesis\Robots\" + file.name + @"\skeleton.bxdj")) ||
                        (file.type == FileType.FIELD && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Synthesis\Fields\" + file.name + @"\definition.bxdf")))
                    {
                        Debug.WriteLine("{0} {1} checked out successfully.", (file.type == FileType.FIELD) ? "Field " : "Robot ", file.name);
                    }
                    else
                    {
                        Debug.WriteLine("{0} {1} not found. Adding to update queue.", (file.type == FileType.FIELD) ? "Field" : "Robot", file.name);
                        getQueue.Add(file);
                    }
                }

                int robotCount = getQueue.Count(x => x.type == FileType.ROBOT);
                int fieldCount = getQueue.Count(x => x.type == FileType.FIELD);


                if (robotCount != 0 || fieldCount != 0)
                {
                    if (System.Windows.Forms.MessageBox.Show(string.Format("{0}{1}{2} {3} available for you to download. Would you like to download {4} now?",
                (robotCount != 0) ? string.Format("{0} {1}", robotCount, (robotCount == 1) ? "robot" : "robots") : "",
                (fieldCount != 0 && robotCount != 0) ? " and " : "",
                (fieldCount != 0) ? string.Format("{0} {1}", fieldCount, (fieldCount == 1) ? "field" : "fields") : "",
                (fieldCount + robotCount > 1) ? "are" : "is",
                (fieldCount + robotCount > 1) ? "them" : "it"), "New Fields/Robots",
                System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        AquireFilesForm aquireFilesForm = new AquireFilesForm(getQueue, webClient);
                        aquireFilesForm.ShowDialog();
                    }
                }
            }
            catch { }
        }
    }
}