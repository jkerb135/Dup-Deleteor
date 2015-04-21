using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using DupDestroyer_v2.Classes;

namespace Dup_Destroyer_v2_WPF.Classes.FileScanner
{
    public class FileHelper : BackgroundWorker
    {
        private readonly DirectoryInfo _rootDirectory;
        private static readonly List<string> CommonExtensions = new List<string> { ".txt", ".doc", ".docx", ".xls" };


        public FileHelper(DirectoryInfo rootDirectory)
        {
            WorkerReportsProgress = true;
            _rootDirectory = @rootDirectory;
        }

        public List<FileAttrib> GetFiles(DirectoryInfo directory)
        {
            var filesList = new List<FileAttrib>();

            try
            {
                // var directories = Directory.EnumerateDirectories(directory.FullName, "*", SearchOption.AllDirectories).ToList();
                ReportProgress(0, new Tuple<string, string>("false", "Gathering Directories Please Wait"));
                if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    var files = directory.GetFiles().ToList();
                    ReportProgress(files.Count(), new Tuple<string, string>("true", ""));

                    foreach (var file in files)
                    {
                        ReportProgress(0,
                            new Tuple<string, string>("false", "Scanning for Duplicate Files in \t" + directory.FullName));
                        Thread.Sleep(50);
                        filesList.Add(FileAttrib.GetFileInfo(new FileInfo(file.FullName)));
                    }

                    GetAllSubDirectoriesFiles(directory.FullName, filesList);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            return filesList.GroupBy(i => i.MD5)
                    .Where(g => g.Count() > 1)
                    .SelectMany(a => a).ToList();
        }

        private void GetAllSubDirectoriesFiles(string rootDirectoryInfo, ICollection<FileAttrib> filesList)
        {
            for (int i = 0, len1 = Directory.GetDirectories(rootDirectoryInfo).Length; i < len1; i++)
            {
                var dir = Directory.GetDirectories(rootDirectoryInfo)[i];

                var dirInfo = new DirectoryInfo(@dir);

                if ((dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    var files = dirInfo.GetFiles().ToList();
                    ReportProgress(files.Count(), new Tuple<string, string>("true", ""));

                    foreach (var file in files)
                    {
                        ReportProgress(0,
                            new Tuple<string, string>("false", "Scanning for Duplicate Files in \t" + dirInfo.FullName));
                        Thread.Sleep(50);
                        filesList.Add(FileAttrib.GetFileInfo(new FileInfo(file.FullName)));
                    }
                }

                GetAllSubDirectoriesFiles(dir, filesList);
            }
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            e.Result = GetFiles(@_rootDirectory);
            ReportProgress(0, new Tuple<string, string>("false", "Done"));
        }
    }
}
