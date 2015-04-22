using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Dup_Destroyer_v2_WPF.Classes.FileScanner
{
    public class FileHelper : BackgroundWorker
    {
        private static readonly List<string> FileFilters = new List<string> {"*.txt", "*.doc", "*.docx", "*.xls"};
        private readonly List<FileAttrib> _images = new List<FileAttrib>();
        private readonly List<FileAttrib> _files;

        public FileHelper(List<FileAttrib> files)
        {
            WorkerReportsProgress = true;
            _files = files;
        }

        private IEnumerable<FileAttrib> AnalyzeFiles(IReadOnlyList<FileAttrib> files)
        {
            /*for (var i = 0; i < files.Count(); i++)
            {
                var f = files[i];

                for (var j = i + 1; j < files.Count(); j++)
                {
                    var f2 = files[j];
                    ReportProgress(0, new Tuple<string, string>("false", "Comparing " + f.FileName + " with " + f2.FileName));
                    if (Math.Abs(f.FileLength - f2.FileLength) < 0) continue;
                    f.MD5 = FileToMd5Hash(f.FilePath);
                    f2.MD5 = FileToMd5Hash(f2.FilePath);

                    if (!_images.Contains(f2) && f.MD5.Equals(f2.MD5))
                    {
                        _images.Add(f2);
                    }
                    Thread.Sleep(50);
                }
            }*/

            return
                _files.GroupBy(x => FileToMd5Hash(x.FilePath))
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x)
                    .OrderByDescending(x => x.FileName);
        }

        private static string FileToMd5Hash(string fileName)
        {
            try
            {
                using (var stream = new BufferedStream(File.OpenRead(fileName), 1200000))
                {
                    var sha = new SHA256Managed();
                    var checksum = sha.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", string.Empty);
                }
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }


        protected override void OnDoWork(DoWorkEventArgs e)
        {
            e.Result = AnalyzeFiles(_files);
            ReportProgress(0, new Tuple<string, string>("false", "Done"));
        }
    }
}
