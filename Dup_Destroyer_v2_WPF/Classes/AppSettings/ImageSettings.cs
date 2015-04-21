using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dup_Destroyer_v2_WPF.Properties;
using Microsoft.VisualBasic;

namespace Dup_Destroyer_v2_WPF.Classes.AppSettings
{
    class ImageSettings
    {
        private string _includedExtensionsSettings = Settings.Default.includedImageExtentions;
        private string _excludedExtensionsSettings = Settings.Default.excludedImageExtensions;

        private List<string> _includedExtension; 
        private List<string> _excludedExtension; 

        public List<String> GetIncludedImageExtentions()
        {
            return _includedExtension = new List<string>(_includedExtensionsSettings.Split(' '));
        }

        public List<String> GetExcludedImageExtentions()
        {
            return _excludedExtension = new List<string>(_excludedExtensionsSettings.Split(' '));
        }

        public void IncludeExtention(string extention)
        {

            if (_excludedExtension.Contains(extention))
            {
                _excludedExtension.Remove(extention);
                Settings.Default.excludedImageExtensions = String.Join(" ", _excludedExtension.ToArray());
                Settings.Default.Save();
            }
            if (_includedExtension.Contains(extention)) return;
            _includedExtension.Add(extention);
            Settings.Default.includedImageExtentions = String.Join(" ", _includedExtension.ToArray());
            Settings.Default.Save();
        }

        public void ExcludeExtention(string extention)
        {
            if (_includedExtension.Contains(extention))
            {
                _includedExtension.Remove(extention);
                Settings.Default.includedImageExtentions = String.Join(" ", _includedExtension.ToArray());
                Settings.Default.Save();
            }
            if (_excludedExtension.Contains(extention)) return;
            _excludedExtension.Add(extention);
            Settings.Default.excludedImageExtensions = String.Join(" ", _excludedExtension.ToArray());
            Settings.Default.Save();
        }

        public void RefreshSettings()
        {
            _includedExtensionsSettings = Settings.Default.includedImageExtentions;
            _excludedExtensionsSettings = Settings.Default.excludedImageExtensions;
        }
    }
}
