using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using Dup_Destroyer_v2_WPF.Classes.FileScanner;

namespace Dup_Destroyer_v2_WPF.Classes
{
    class GetFiles : BackgroundWorker
    {
        private readonly DirectoryInfo _rootPath;
        private readonly IEnumerable<string> _fileFilters; 
        private int _fileCount = 0; 
        public GetFiles(DirectoryInfo path, IEnumerable<string> filters)
        {
            WorkerReportsProgress = true;
            _rootPath = path;
            _fileFilters = filters;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            e.Result = ScanForFiles(_rootPath, _fileFilters);
        }

        public int GetDirectoryCount(string root)
        {
            _fileCount = 0;
            var directory = new DirectoryInfo(root);
            try
            {
                if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    _fileCount++;
                }

                GetAllSubDirectoriesCount(directory.FullName);
            }
            catch (UnauthorizedAccessException)
            {
            }
            return _fileCount;
        }
        private void GetAllSubDirectoriesCount(string rootDirectoryInfo)
        {
            for (int i = 0, len1 = Directory.GetDirectories(rootDirectoryInfo).Length; i < len1; i++)
            {
                var dir = Directory.GetDirectories(rootDirectoryInfo)[i];

                var dirInfo = new DirectoryInfo(@dir);

                if ((dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    _fileCount++;
                }

                GetAllSubDirectoriesCount(dir);
            }
        }

        public List<FileAttrib> ScanForFiles(DirectoryInfo rootDirectoryInfo, IEnumerable<string> filters)
        {
            var filesList = new List<FileAttrib>();
            var listOfFilters = filters as IList<string> ?? filters.ToList();
            ReportProgress(0, new Tuple<string, string>("false", "Searching Directory " + rootDirectoryInfo.FullName));
            Thread.Sleep(50);
            foreach (var filter in listOfFilters)
            {
                try
                {
                    var files = rootDirectoryInfo.GetFiles(filter, SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        try
                        {
                            filesList.Add(FileAttrib.GetFileInfo(file));
                        }
                        catch (PathTooLongException)
                        {
                            /*255 item range is too long*/
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    /*Admin Access*/
                }
                catch (SecurityException)
                {
                    /*you don't have the permission of viewing this files*/
                }
                catch (DirectoryNotFoundException)
                {
                    /*directory not found*/
                }
                catch (ArgumentNullException)
                {
                    /*bad parameters*/
                }
            }
            try
            {
                var directories = rootDirectoryInfo.GetDirectories();
                foreach (var directory in directories)
                {
                    try
                    {
                        filesList.AddRange(ScanForFiles(directory, listOfFilters));
                    }
                    catch (PathTooLongException)
                    {
                        //continue the foreach
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {

            }
            catch (DirectoryNotFoundException)
            {
                /*directory not found*/
                return filesList;
            }

            return filesList;
        }
    }
}
