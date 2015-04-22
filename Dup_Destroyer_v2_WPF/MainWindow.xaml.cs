using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Dup_Destroyer_v2_WPF.Classes;
using Dup_Destroyer_v2_WPF.Classes.AppSettings;
using Dup_Destroyer_v2_WPF.Classes.FileScanner;
using Dup_Destroyer_v2_WPF.Classes.ImageScanner;
using Dup_Destroyer_v2_WPF.Classes.Media;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Dup_Destroyer_v2_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private DirectoryInfo _standardPath = null;
        private int _selectedTab;
        private readonly ImageSettings _settings = new ImageSettings();
        private readonly String _systemDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            
            FilePathTxt.Text = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
            ImageFilePathTxt.Text = _systemDrive;
            BindImageSettings();
        }

        private void BindImageSettings()
        {
            IncludedExtentionListBox.ItemsSource = _settings.GetIncludedImageExtentions();
            ExcludedExtentionListBox.ItemsSource = _settings.GetExcludedImageExtentions();
            IncludedExtentionListBox.Items.Refresh();
            ExcludedExtentionListBox.Items.Refresh();    
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog {Description = "Select Your Folder", ShowNewFolderButton = false};
            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
             if (_selectedTab.Equals(1))
            {
                FilePathTxt.Text = folderDialog.SelectedPath;
            }
             else if (_selectedTab.Equals(2))
            {
                ImageFilePathTxt.Text = folderDialog.SelectedPath;
            }
             else if (_selectedTab.Equals(3))
            {
                AudioFilePathTxt.Text = folderDialog.SelectedPath;
            }
        }

        private void listDupsBtn_Click(object sender, RoutedEventArgs e)
        {
            fileDestroyerPGB.Value = 0;
            var fileWorker = new GetFiles(_standardPath,GetFilters(tabControl1.SelectedIndex));

            fileWorker.ProgressChanged += Report_ProgressChanged;
            fileWorker.RunWorkerCompleted += FileWorker_RunWorkerCompleted;

            if (fileWorker.IsBusy) return;

            var count = fileWorker.GetDirectoryCount(_standardPath.FullName);
            fileDestroyerPGB.Maximum = count;
            ImageProgressBar.Maximum = count;
            audioDestroyerPBG.Maximum = count;
            fileWorker.RunWorkerAsync();
        }

        private void OpenFileLocation(object sender, RoutedEventArgs e)
        {
            foreach (var path in ContentsFilesDataGrid.SelectedItems.Cast<object>().Select(file => ((FileAttrib)file).FilePath).Where(File.Exists))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + path));
            }
        }

        private void deleteDupsBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileRemoval = new FileRemoval(_standardPath.FullName,ContentsFilesDataGrid.ItemsSource.Cast<FileAttrib>().ToList());
            //fileRemoval.ProgressChanged += FileHelper_ProgressChanged;
            //fileRemoval.RunWorkerCompleted += FileHelper_RunWorkerCompleted;

            if (fileRemoval.IsBusy) return;
            fileRemoval.RunWorkerAsync();
        }

        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            _selectedTab = (tabControl1.SelectedIndex);
        }

        private void GoHome(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedIndex = 0;
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedIndex = 4;
        } 

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox != null && textbox.Text.Length != 0)
            {
                if (textbox.Name.Equals("includeTxtBox", StringComparison.CurrentCultureIgnoreCase))
                {
                    addInImageExtentionsBtn.IsEnabled = true;
                }
                else if (textbox.Name.Equals("excludeTxtBox", StringComparison.CurrentCultureIgnoreCase))
                {
                    addExImageExtentionsBtn.IsEnabled = true;
                }
            }
            else if (textbox != null && textbox.Text.Length == 0)
            {
                if (textbox.Name.Equals("includeTxtBox", StringComparison.CurrentCultureIgnoreCase))
                {
                    addInImageExtentionsBtn.IsEnabled = false;
                }
                else if (textbox.Name.Equals("excludeTxtBox", StringComparison.CurrentCultureIgnoreCase))
                {
                    addExImageExtentionsBtn.IsEnabled = false;
                }
            }
        }

        private void SettingsAddRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            var add = sender as Button;
            if (add != null && add.Name.Equals("addInImageExtentionsBtn", StringComparison.InvariantCultureIgnoreCase))
            {
                _settings.IncludeExtention(includeTxtBox.Text);
                includeTxtBox.Text = String.Empty;
            }
            else if (add != null && add.Name.Equals("addExImageExtentionsBtn", StringComparison.CurrentCultureIgnoreCase))
            {
                _settings.ExcludeExtention(excludeTxtBox.Text);
                excludeTxtBox.Text = String.Empty;
            }
            _settings.RefreshSettings();
            BindImageSettings();
        }

        protected void MediaHelper_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null) return;
            var userstate = e.UserState.ToString().Split(',');
            if (userstate[0].TrimStart('(').Trim().Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                audioDestroyerPBG.Value = 0;
                audioDestroyerPBG.Maximum = e.ProgressPercentage;
            }
            ProgressLabel3.Content = userstate[1].TrimEnd(')');
            audioDestroyerPBG.Value += 1;
        }

        private static IEnumerable<string> GetFilters(int index)
        {
            switch (index)
            {
                case 1:
                    return new List<string> { "*.txt", "*.doc", "*.docx", "*.xls" };
                case 2:
                    return new List<string> { "*.gif", "*.tif", "*.jpg", "*.bmp", "*.png", "*.psd", "*.thm", "*.yuv" };
                case 3:
                   return new List<string> { "*.mp3", "*.ogg", "*.flac", "*.wav" };
            }
            return null;
        } 

        protected void Report_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null) return;
            var userstate = e.UserState.ToString().Split(',');
            var message = userstate[1].TrimEnd(')');
            UpdateGui(message);
        }

        private void FileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StartNextTask(e.Result as List<FileAttrib>);
        }

        private void ChangePath(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (@textbox != null) _standardPath = new DirectoryInfo(@textbox.Text);
        }

        private void StartNextTask(List<FileAttrib> files)
        {
            var count = 0;
            for (var i = files.Count - 1; i > 0; i--)
            {
                count += i;
            }
            switch (_selectedTab)
            {
                case 1:
                    fileDestroyerPGB.Maximum = count;
                    fileDestroyerPGB.Value = 0;
                    var fileAnalyzer = new FileHelper(files);
                    fileAnalyzer.ProgressChanged += Report_ProgressChanged;
                    fileAnalyzer.RunWorkerCompleted += Run_WorkCompleted;
                    fileAnalyzer.RunWorkerAsync();
                    break;
                case 2:
                    ImageProgressBar.Maximum = count;
                    ImageProgressBar.Value = 0;
                    var imageAnaylzer = new ImageAnalyzer(files);
                    imageAnaylzer.ProgressChanged += Report_ProgressChanged;
                    imageAnaylzer.RunWorkerCompleted += Run_WorkCompleted;
                    imageAnaylzer.RunWorkerAsync();
                    break;
                case 3:
                    audioDestroyerPBG.Maximum = count;
                    audioDestroyerPBG.Value = 0;
                    var audioAnalyzer = new MediaFinder(files);
                    audioAnalyzer.ProgressChanged += Report_ProgressChanged;
                    audioAnalyzer.RunWorkerCompleted += Run_WorkCompleted;
                    audioAnalyzer.RunWorkerAsync();
                    break;
            }
        }

        private void Run_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateGrid(e.Result as IEnumerable<FileAttrib>);
        }

        private void UpdateGui(string message)
        {
            switch (_selectedTab)
            {
                case 1:
                    ProgressLabel.Content = message;
                    fileDestroyerPGB.Value += 1;
                    break;
                case 2:
                    ProgressLabel2.Content = message;
                    ImageProgressBar.Value += 1;
                    break;
                case 3:
                    ProgressLabel3.Content = message;
                    audioDestroyerPBG.Value += 1;
                    break;
            }
        }
        
        private void UpdateGrid(IEnumerable<FileAttrib> items)
        {
            switch (_selectedTab)
            {
                case 1:
                    ContentsFilesDataGrid.ItemsSource = items;
                    break;
                case 2:
                    ContentsImagesDataGrid.ItemsSource = items;
                    break;
                case 3:
                    MessageBox.Show(items.Count().ToString());
                    ContentsAudioDataGrid.ItemsSource = items;
                    break;
            }
        }
    }
}
