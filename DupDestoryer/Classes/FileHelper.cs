using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupDestoryer.Classes
{
    class FileHelper
    {
        public static DataTable ShowAllFoldersUnder(string path)
        {
            var dt = new DataTable();
            dt.Columns.Add("File Name");
            try
            {
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint)
                    != FileAttributes.ReparsePoint)
                {
                    foreach (string folder in Directory.GetDirectories(path))
                    {
                        Console.WriteLine(Path.GetFile(folder));
                        DataRow dr = dt.NewRow();
                        dr["File Name"] = Path.GetFileName(folder);
                        dt.Rows.Add(dr);
                        ShowAllFoldersUnder(folder);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            return dt;
        }
    }
}
