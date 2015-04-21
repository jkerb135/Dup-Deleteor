using System.Collections.Generic;
using System.Linq;
using DupDestroyer_v2.Classes;
using Dup_Destroyer_v2_WPF.Properties;

namespace Dup_Destroyer_v2_WPF.Classes.FileScanner
{
    public class FileFinder
    {
        private static readonly List<string> CommonExtensions = new List<string> { ".txt", ".doc", ".docx", ".xls" };

        private static string _whereClause = "";

        public delegate void ProgressUpdate(int value, string message);

        public event ProgressUpdate OnProgressUpdate;

        private static readonly string ExcludedExtensionsSettings = Settings.Default.excludedImageExtensions;
        private static readonly List<string> ExcludedExtension = new List<string>();

        public List<FileAttrib> FindDuplicateFiles(List<FileAttrib> fileList)
        {
            var queryResult = new List<FileAttrib>();
            if (fileList.Count == 0) return queryResult;

            ExcludedExtension.AddRange(ExcludedExtensionsSettings.Split(','));
            var extensionsToCheckFor = new List<string>();

            if (ExcludedExtension.Count != 0)
            {
                extensionsToCheckFor.AddRange(CommonExtensions.Except(ExcludedExtension));
            }
            else
            {
                extensionsToCheckFor = CommonExtensions;
            }

            var increment = 100 / fileList.Count();

            for (int i = 0, len = extensionsToCheckFor.Count; i < len; i++)
            {
                for (int j = 0, len2 = fileList.Count(); j < len2; j++)
                {
                    if (fileList[j].FileExtension == extensionsToCheckFor[i]) ;
                    queryResult.Add(fileList[j]);
                    if (OnProgressUpdate != null)
                    {
                        OnProgressUpdate(increment, "Scanning list of files for duplicate files");
                    }
                }

            }
            return queryResult;
        }
    }
}
