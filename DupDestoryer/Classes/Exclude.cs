using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupDestoryer.Classes
{
    class Exclude
    {
        private static readonly List<string> DirExclusionList = new List<String>();
        private static readonly List<string> ExtExclusionList = new List<String>();

        public static void SetExcludedDirectories(List<string> exclusions)
        {
            DirExclusionList.AddRange(exclusions);
        }

        public static List<string> GetExcludedDirectories()
        {
            return DirExclusionList;
        } 

        public static void SetExcludedExtentions(List<string> exclusions)
        {
            ExtExclusionList.AddRange(exclusions);
        }

        public static List<string> GetExcludedExtentions()
        {
            return ExtExclusionList;
        } 

    }
}
