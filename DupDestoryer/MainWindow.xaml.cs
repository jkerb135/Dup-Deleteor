using System.Net.Mime;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.IO;
using System.Data;
using System;
using DupDestoryer.Classes;

namespace DupDestoryer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            Title = "Dup Destroyer";
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            Box.Text = dialog.SelectedPath;
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            Files.ItemsSource = FileHelper.ShowAllFoldersUnder(@Box.Text).DefaultView;
        }
    }
}
