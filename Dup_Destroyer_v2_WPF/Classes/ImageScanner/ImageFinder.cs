using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DupDestroyer_v2.Classes;
using Dup_Destroyer_v2_WPF.Properties;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace Dup_Destroyer_v2_WPF.Classes.ImageScanner
{
    class ImageFinder
    {
        private static readonly List<string> CommonExtensions = new List<string> { ".gif", ".tif", ".jpg", ".bmp", ".png", ".psd", ".thm", ".yuv" };

        private static string _whereClause = "";

        private static readonly string ExcludedExtensionsSettings = Settings.Default.excludedImageExtensions;
        private static readonly List<string> ExcludedExtension = new List<string>();

        public static List<FileAttrib> FindDuplicateImages(DirectoryInfo path, ProgressBar imgBar)
        {
            ExcludedExtension.AddRange(ExcludedExtensionsSettings.Split(' '));

            var extensionsToCheckFor = new List<string>();
            var queryResult = new List<FileAttrib>();
            var sameImage = true;

            var fileList = GetFiles(path);

            if (ExcludedExtension.Count != 0)
            {
                extensionsToCheckFor.AddRange(CommonExtensions.Except(ExcludedExtension));
            }
            else
            {
                extensionsToCheckFor = CommonExtensions;
            }

            for (int i = 0, len = extensionsToCheckFor.Count; i < len; i++)
            {
                var i1 = i;
                var result = fileList.Where(f => String.Equals(f.FileExtension, extensionsToCheckFor[i1],StringComparison.CurrentCultureIgnoreCase));
                queryResult.AddRange(result);
            }

            var duplicateTuples = ImageTool.GetDuplicateImages(queryResult.Select(x => x.FilePath));
               
            var endResult = (from d in duplicateTuples from filepath in d from file in queryResult where String.Equals(file.FilePath, filepath, StringComparison.CurrentCultureIgnoreCase) select file).ToList();

            return endResult;
        }

        private static List<FileAttrib> GetFiles(DirectoryInfo directory)
        {
            var files = new List<FileAttrib>();
            try
            {
                if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    files.AddRange(directory.GetFiles().Select(FileAttrib.GetFileInfo));
                }

                GetAllSubDirectoriesFiles(directory.FullName, files);
            }
            catch (UnauthorizedAccessException) { }

            return files;
        }

        private static void GetAllSubDirectoriesFiles(string rootDirectoryInfo, List<FileAttrib> files)
        {
            for (int i = 0, len1 = Directory.GetDirectories(rootDirectoryInfo).Length; i < len1; i++)
            {
                var dir = Directory.GetDirectories(rootDirectoryInfo)[i];

                var dirInfo = new DirectoryInfo(@dir);

                if ((dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    files.AddRange(dirInfo.GetFiles().Select(FileAttrib.GetFileInfo));
                }

                GetAllSubDirectoriesFiles(dir, files);
            }
        }
    }
}
