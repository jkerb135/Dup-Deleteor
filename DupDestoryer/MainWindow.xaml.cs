using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Forms;
using DupDestoryer.Classes;
using Microsoft.VisualBasic.FileIO;
using MessageBox = System.Windows.MessageBox;

namespace DupDestoryer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog {Description = "Please Select Folder", ShowNewFolderButton = false};
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            Box.Text = dialog.SelectedPath;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Files.ItemsSource = FileHelper.GetFiles(new DirectoryInfo(@Box.Text));
        }

        private void FileNavigation_OnClick(object sender, RoutedEventArgs e)
        {
            FileCleaner.Visibility = Visibility.Visible;
            SettingsPanel.Visibility = Visibility.Hidden;
        }

        private void SettingsNavigation_OnClick(object sender, RoutedEventArgs e)
        {
            FileCleaner.Visibility = Visibility.Hidden;
            SettingsPanel.Visibility = Visibility.Visible;
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var duplicates = new HashSet<FileAttrib>(Files.ItemsSource.Cast<FileAttrib>().ToList(), new FileImpressionComparer());
            Files.ItemsSource = duplicates.ToList();
            FileHelper.RemoveEmptyDirectory(@Box.Text);
        }
    }

    class FileImpressionComparer : IEqualityComparer<FileAttrib>
    {
        public bool Equals(FileAttrib p1, FileAttrib p2)
        {
            if (Math.Abs(p1.FileLength - p2.FileLength) > 0) return false;
            if (p1.FileImpression != p2.FileImpression) return false;
            FileSystem.DeleteFile(p2.FilePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            return true;
        }

        public int GetHashCode(FileAttrib p)
        {
            return base.GetHashCode();
        }
    }
}
