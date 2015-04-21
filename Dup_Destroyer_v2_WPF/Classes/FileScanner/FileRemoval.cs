using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using DupDestroyer_v2.Classes;
using Microsoft.VisualBasic.FileIO;

namespace Dup_Destroyer_v2_WPF.Classes.FileScanner
{
    class FileRemoval : BackgroundWorker
    {
        private static readonly string Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private readonly string _path;
        private readonly List<FileAttrib> _data;
        public FileRemoval(string path, List<FileAttrib> data)
        {
            WorkerReportsProgress = true;
            _path = path;
            _data = data;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            Directory.CreateDirectory(Desktop + @"\DupDeletions\FileRemoval");

            var duplicates = _data.GroupBy(s => s.MD5).SelectMany(grp => grp.Skip(1));

            ReportProgress(duplicates.Count(), new Tuple<string, string>("true", ""));

            foreach (var file in duplicates)
            {
                ReportProgress(0, new Tuple<string, string>("false", "Moving " + file.FileName + " to " + Desktop + @"\DupDeletions\FileRemoval"));
                FileSystem.MoveFile(file.FilePath, Desktop + @"\DupDeletions\FileRemoval\" + @file.FileName);
                Thread.Sleep(100);
            }

            RemoveEmptyDirectory(_path);

        }

        public void RemoveEmptyDirectory(string path)
        {
            var directories = Directory.GetDirectories(path);
            ReportProgress(directories.Count(), new Tuple<string, string>("true", ""));

            foreach (var directory in Directory.GetDirectories(path))
            {
                ReportProgress(0, new Tuple<string, string>("false", "Cleaning Up"));
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
                Thread.Sleep(100);
                RemoveEmptyDirectory(directory);
            }
        }
    }
}
