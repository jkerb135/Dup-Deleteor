using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DupDestroyer_v2.Classes;
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
        private String _selectedTab;
        private readonly ImageSettings _settings = new ImageSettings();
        private readonly BackgroundWorker _listImagesBackgroundWorker = new BackgroundWorker();
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
            _listImagesBackgroundWorker.DoWork += ListImagesBackgroundWorker_DoWork;
            _listImagesBackgroundWorker.RunWorkerCompleted += ListImagesBackgroundWorker_RunWorkerCompleted;
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
             if (_selectedTab.Equals("File", StringComparison.InvariantCultureIgnoreCase))
            {
                FilePathTxt.Text = folderDialog.SelectedPath;
                fileDestroyerPGB.Value = 0;
            }
             else if (_selectedTab.Equals("Image", StringComparison.InvariantCultureIgnoreCase))
            {
                ImageFilePathTxt.Text = folderDialog.SelectedPath;
            }
             else if (_selectedTab.Equals("Audio", StringComparison.InvariantCultureIgnoreCase))
            {
                AudioFilePathTxt.Text = folderDialog.SelectedPath;
            }
        }

        private void listDupsBtn_Click(object sender, RoutedEventArgs e)
        {

            var button = sender as Button;

            if (button != null && button.Name.Equals("FileScan", StringComparison.CurrentCultureIgnoreCase))
            {
                _standardPath = new DirectoryInfo(@FilePathTxt.Text);
                var fileHelper = new FileHelper(_standardPath);
                fileHelper.ProgressChanged += FileHelper_ProgressChanged;
                fileHelper.RunWorkerCompleted += FileHelper_RunWorkerCompleted;

                if (fileHelper.IsBusy) return;
                fileHelper.RunWorkerAsync();

            }
            else if (button != null && button.Name.Equals("ImageScan", StringComparison.CurrentCultureIgnoreCase))
            {
                _listImagesBackgroundWorker.RunWorkerAsync();
            }
            else if (button != null && button.Name.Equals("AudioScan", StringComparison.CurrentCultureIgnoreCase))
            {
                _standardPath = new DirectoryInfo(@AudioFilePathTxt.Text);
                var mediaHelper = new MediaFinder(_standardPath);
                mediaHelper.ProgressChanged += MediaHelper_ProgressChanged;
                mediaHelper.RunWorkerCompleted += MediaHelper_RunWorkerCompleted;
                if (mediaHelper.IsBusy) return;
                mediaHelper.RunWorkerAsync();
            }
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
            fileRemoval.ProgressChanged += FileHelper_ProgressChanged;
            //fileRemoval.RunWorkerCompleted += FileHelper_RunWorkerCompleted;

            if (fileRemoval.IsBusy) return;
            fileRemoval.RunWorkerAsync();
        }

        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            _selectedTab = ((TabItem)tabControl1.SelectedItem).Name;
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

        protected void FileHelper_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null) return;
            var userstate = e.UserState.ToString().Split(',');
            if (userstate[0].TrimStart('(').Trim().Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                fileDestroyerPGB.Value = 0;
                fileDestroyerPGB.Maximum = e.ProgressPercentage;
            }
            ProgressLabel.Content = userstate[1].TrimEnd(')');
            fileDestroyerPGB.Value += 1;
        }

        private void FileHelper_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (List<FileAttrib>) e.Result;
            ContentsFilesDataGrid.ItemsSource = result;
            
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

        private void MediaHelper_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (List<FileAttrib>)e.Result;
            ContentsFilesDataGrid.ItemsSource = result;

        }

        private void ListImagesBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                    _standardPath = new DirectoryInfo(@ImageFilePathTxt.Text);
                    e.Result = ImageFinder.FindDuplicateImages(_standardPath, ImageProgressBar);
            });
        }

        private void ListImagesBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (List<FileAttrib>)e.Result;
            ContentsImagesDataGrid.ItemsSource = result;
        }

    }
}
