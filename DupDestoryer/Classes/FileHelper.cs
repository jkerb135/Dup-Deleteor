using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DupDestoryer.Classes
{
    public class FileAttrib
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileImpression { get; set; }
        public double FileLength { get; set; }
    }
    class FileHelper
    {
        public static List<FileAttrib> GetFiles(DirectoryInfo directory)
        {
            var files = new List<FileAttrib>();

            try
            {
                if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    files.AddRange(directory.GetFiles().Select(GetFileInfo));
                }

                GetAllSubDirectoriesFiles(directory.FullName, files);
            }
            catch (UnauthorizedAccessException)
            {
            }
            return files.GroupBy(i => i.FileImpression)
                    .Where(g => g.Count() > 1)
                    .SelectMany(a => a)
                    .ToList().Select(x => new FileAttrib()
                    {
                        FileName = x.FileName,
                        FilePath = x.FilePath,
                        FileImpression = x.FileImpression,
                        FileLength = x.FileLength
                    }).OrderByDescending(x => x.FileName).ToList();
        }
        public static void RemoveEmptyDirectory(string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                RemoveEmptyDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }

        private static void GetAllSubDirectoriesFiles(string rootDirectoryInfo, List<FileAttrib> files)
        {
            for (int i = 0, len1 = Directory.GetDirectories(rootDirectoryInfo).Length; i < len1; i++)
            {
                var dir = Directory.GetDirectories(rootDirectoryInfo)[i];

                var dirInfo = new DirectoryInfo(@dir);

                if ((dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    files.AddRange(dirInfo.GetFiles().Select(GetFileInfo));
                }

                GetAllSubDirectoriesFiles(dir, files);
            }
        }
        private static FileAttrib GetFileInfo(FileInfo finfo)
        {
            return new FileAttrib
            {
                FileName = finfo.Name,
                FilePath = finfo.FullName,
                FileImpression = FileToMd5Hash(finfo.FullName),
                FileLength = finfo.Length
            };
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

    }
}
