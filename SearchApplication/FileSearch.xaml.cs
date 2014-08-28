using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace SearchApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FileSearch : Window
    {
        public FileSearch()
        {
            InitializeComponent();
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lstResults.SelectedIndex = -1;
            lstResults.Items.Clear();
            lbNumberOfFiles.Content = "Searching, please wait...";
            GetDirectories();
        }

        private void GetDirectories()
        {
            
            List<string> files = new List<string>();
            var rootDirectory = tbFilePath.Text;
            if (String.IsNullOrEmpty(rootDirectory))
            {
                System.Windows.Forms.MessageBox.Show("Please enter a path to search.", "Error");
                return;
            }

            var directories = Directory.GetDirectories(rootDirectory).ToList();

            for (int i = 0; i < directories.Count; i++)
            {
                try
                {
                    directories.AddRange(Directory.GetDirectories(directories[i]).ToList());
                }
                catch
                {

                }
            }

            directories.Add(rootDirectory);
            string[] fileExtensions = tbFileExtensions.Text.Split(',');

            Parallel.ForEach(directories, (directory) => 
                {
                    try { files.AddRange(Directory.EnumerateFiles(directory).Where(x => fileExtensions.Contains(FileExtension(x)))); }
                    catch{}
                }
                );           

            DisplayResultInformation(directories.Count, files.Count, files);
        }

        private string FileExtension(string file)
        {
            var fileParts = file.Split('.');

            return "." + fileParts[fileParts.Length - 1];
        }

        private async void DisplayResultInformation(int directoryCount, int fileCount, List<String> directoriesToSearch)
        {
            lbNumberOfFiles.Content = String.Format("Number of folders: {0}. Files found: {1}", directoryCount.ToString(), fileCount.ToString());
            lbFound.Content = "Found: 0";

            string line;
            int searchResultCount = 0;
            foreach (var file in directoriesToSearch)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        int lineNumber = 1;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (line.Contains(tbSearchCriteria.Text))
                            {
                                SearchResultData newItem = new SearchResultData();
                                newItem.FileNameAndPath = file;
                                newItem.LineNumber = String.Format("{0} - Ln {1}", file, lineNumber.ToString());

                                
                                
                                lstResults.Items.Add(newItem);
                                
                                searchResultCount++;
                                lineNumber++;
                                lbFound.Content = String.Format("Found: {0}",searchResultCount.ToString());                               
                                
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    
                    
                }
                
            }

            lbFound.Content = "Search complete. " + lbFound.Content;
        }
             
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            //string[] files = Directory.GetFiles(fbd.SelectedPath);
            //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");

            tbFilePath.Text = fbd.SelectedPath;

            
        }

        private void lstResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            try
            {
                
                var fileName = lstResults.SelectedValue.ToString();
                Process.Start(@fileName);
            }
            catch (Exception)
            {
                
               
            }
            
        }
    }
}
