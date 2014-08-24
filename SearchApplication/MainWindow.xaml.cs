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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lstResults.SelectedIndex = -1;
            lstResults.Items.Clear();
            GetDirectories();
        }

        private void GetDirectories()
        {
            List<string> files = new List<string>();
            var rootDirectory = tbFilePath.Text;

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
                    try { files.AddRange(Directory.EnumerateFiles(directory).Where(x => fileExtensions.Contains(x.Substring(x.Length - 4, 4)))); }
                    catch{}
                }
                );           

            DisplayResultInformation(directories.Count, files.Count, files);
        }

        private async void DisplayResultInformation(int directoryCount, int fileCount, List<String> directoriesToSearch)
        {
            lbNumberOfFiles.Content = String.Format("Number of folders: {0}. Files found: {1}", directoryCount.ToString(), fileCount.ToString());


            string line;
            foreach (var file in directoriesToSearch)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (line.Contains(tbSearchCriteria.Text))
                            {
                                lstResults.Items.Add(file);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    
                    
                }
                
            }
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
                Process.Start(@lstResults.SelectedItem.ToString());
            }
            catch (Exception)
            {
                
               
            }
            
        }
    }
}
