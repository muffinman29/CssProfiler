using System;
using System.Windows;
using System.Windows.Forms;

namespace SearchApplication
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var newSearch = new FileSearch {Owner = this};
            newSearch.Show();            
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.ShowDialog();

            //string[] files = Directory.GetFiles(fbd.SelectedPath);
            //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");

            TbParentDirectory.Text = fbd.SelectedPath;
        }

        private void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            //  STEP 1. Get a list of folders to search.
            //  var foldersToSearch =   GetFolders(string root);
            //  STEP 2. Get a list of css, html, cfm, aspx files found in step 1.
            //  var files   =   GetFiles(foldersToSearch);
            //  STEP 3. Analyze each file for issues
            //  FindInLineCss(files);
            //  STEP 4. Report issues
            //  DisplayResults();
            


            
            
        }

        private string GetFolders(string root)
        {
            return "";
        }

        private string GetFiles(string foldersToSearch)
        {
            return "";
        }
        
        private void FindInlineCss()
        { 
            
        }
        


    }
}
