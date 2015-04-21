using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DupDestroyer_v2.Classes
{
    public class FileAttrib
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileModifiedTime { get; set; }
        public string MD5 { get; set; }
        public string Sha256 { get; set; }
        public double FileLength { get; set; }
        public string FileExtension { get; set; }

        public static FileAttrib GetFileInfo(FileInfo finfo)
        {
            return new FileAttrib
            {
                FileName = finfo.Name,
                FilePath = finfo.FullName,
                FileExtension = finfo.Extension,
                MD5 = FileToMd5Hash(finfo.FullName),
                FileModifiedTime = finfo.LastWriteTimeUtc.ToString(),
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

        public static string GetSha256(string filepath)
        {
            return BitConverter.ToString(SHA256.Create().ComputeHash(File.ReadAllBytes(filepath))).Replace("-", "");
        }
    }

}
