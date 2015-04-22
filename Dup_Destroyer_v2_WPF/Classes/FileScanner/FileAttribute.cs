using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualBasic.FileIO;

namespace Dup_Destroyer_v2_WPF.Classes.FileScanner
{
    public class FileAttrib : IEqualityComparer<FileAttrib>
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileModifiedTime { get; set; }
        public ulong ImageHash { get; set; }
        public float[] MediaHash { get; set; }
        public string MD5 { get; set; }
        public double FileLength { get; set; }
        public string FileExtension { get; set; }

        public static FileAttrib GetFileInfo(FileInfo finfo)
        {
            return new FileAttrib
            {
                FileName = finfo.Name,
                FilePath = finfo.FullName,
                FileExtension = finfo.Extension,
                FileModifiedTime = finfo.LastWriteTimeUtc.ToString(),
                FileLength = finfo.Length
            };
        }

        public static string GetSha256(string filepath)
        {
            return BitConverter.ToString(SHA256.Create().ComputeHash(File.ReadAllBytes(filepath))).Replace("-", "");
        }

        public bool Equals(FileAttrib p1, FileAttrib p2)
        {
            if (Math.Abs(p1.FileLength - p2.FileLength) > 0) return false;
            return p1.MD5 == p2.MD5;
        }

        public int GetHashCode(FileAttrib p)
        {
            return base.GetHashCode();
        }
    }

}
