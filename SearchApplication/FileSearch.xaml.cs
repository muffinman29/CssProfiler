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
//using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using SearchApplication.BusinessLogic;
using System.Windows.Interop;
using System.Drawing;
using Delimon.Win32.IO;

namespace SearchApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FileSearch : Window
    {
        bool cancel = false;
        private List<string> directories;
        private List<string> files;
        FileSearcher mySearcher;
        
        public FileSearch()
        {
            InitializeComponent();
            imgError.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            imgError.Width = imgError.Source.Width;
            imgError.Height = imgError.Source.Height;
            imgError.Visibility = Visibility.Hidden;           

            directories = new List<string>();
            files = new List<string>();
            mySearcher = new FileSearcher();
        }

        private bool ValidateFields()
        {
            return !(String.IsNullOrEmpty(tbFileExtensions.Text.Trim()) || String.IsNullOrEmpty(tbFilePath.Text.Trim()) || String.IsNullOrEmpty(tbSearchCriteria.Text.Trim()));
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                System.Windows.Forms.MessageBox.Show("Unable to perform the search. Please make sure you have entered the correct information and try again.", "Error");
                return;
            }
            prgSearch.Value = 0;
            cancel = false;
            btnSearch.IsEnabled = false;
            lstResults.SelectedIndex = -1;
            lstResults.Items.Clear();
            lbNumberOfFiles.Content = "Searching, please wait...";
            directories.Clear();
            files.Clear();
            //FileSearcher mySearcher = new FileSearcher();
            //mySearcher.GetDirectories(tbFilePath.Text, tbFileExtensions.Text);
            //directories = mySearcher.Directories;
            //files = mySearcher.Files;
            //if (mySearcher.Error)
            //{
            //    ShowErrorIcon();
            //}
            var rootDirectories = tbFilePath.Text.Split(';');
            foreach (var root in rootDirectories)
            {
                GetDirectories(root);
            }

            GetFilesByExtension();
            
            DisplayResultInformation();
        }

        private void GetDirectories(string rootDirectory)
        {

            //if (String.IsNullOrEmpty(tbFilePath.Text))
            //{
            //    System.Windows.Forms.MessageBox.Show("Please enter a path to search.", "Error");
            //    return;
            //}  

            string[] subDirs = null;

            try
            {
                subDirs = Directory.GetDirectories(rootDirectory, "*", SearchOption.TopDirectoryOnly);
                foreach (var item in subDirs)
                {
                    directories.Add(item);
                }

            }
            catch (Exception e)
            {
                Logger.WriteToFile(e.Message);
            }

            try
            {
                foreach (var dirInfo in subDirs)
                {
                    GetDirectories(dirInfo);
                }
            }
            catch (Exception e)
            {
                Logger.WriteToFile(e.Message);
            }

                   

            //var tempRootDirectories = new List<string>();

            //tempRootDirectories = Directory.GetDirectories(rootDirectory).ToList();



            //for (int i = 0; i < directories.Count; i++)
            //{
            //    try
            //    {
            //        tempRootDirectories.AddRange(Directory.GetDirectories(directories[i], "*", SearchOption.AllDirectories).ToList());
            //    }
            //    catch (Exception e)
            //    {
            //        Logger.WriteToFile(e.Message);
            //        ShowErrorIcon();
            //    }
            //}

            //tempRootDirectories.Add(rootDirectory);

            //directories.AddRange(tempRootDirectories);

            //for (int i = 0; i < directories.Count; i++)
            //{
            //    while (HasSubFolders(directories[i]))
            //    {
            //        GetDirectories(directories[i]);
            //    }
            //}
            
        }

        private void GetFilesByExtension()
        {
            bool hasError = false;
            string[] fileExtensions = tbFileExtensions.Text.Split(',');

            for (int i = 0; i < fileExtensions.Count(); i++)
            {
                fileExtensions[i] = fileExtensions[i].Trim();
            }

            Parallel.ForEach(directories, (directory) =>
            {
                try
                {

                    files.AddRange(Directory.GetFiles(directory).Where(x => fileExtensions.Contains(FileExtension(x.ToLower()))));
                }
                catch (Exception e)
                {
                    Logger.WriteToFile(e.Message);
                    hasError = true;
                }
            });

            if (hasError)
            {
                ShowErrorIcon();
            }
        
        }

        private bool HasSubFolders(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            DirectoryInfo[] subdirs = directory.GetDirectories();
            return subdirs.Length != 0;
        }

        private string FileExtension(string file)
        {
            var fileParts = file.Split('.');

            return "." + fileParts[fileParts.Length - 1];
        }

        private async void DisplayResultInformation()
        {
            lbNumberOfFiles.Content = String.Format("Number of folders: {0}. Files found: {1}", directories.Count.ToString(), files.Count.ToString());
            lbFound.Content = "Found: 0";

            string line;
            int searchResultCount = 0;
            int fileCounter = 0;
            foreach (var file in files)
            {
                try
                {
                    if (!cancel)
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
                        {
                            int lineNumber = 1;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                if (line.ToLower().Contains(tbSearchCriteria.Text.ToLower()))
                                {
                                    SearchResultData newItem = new SearchResultData();
                                    newItem.FileNameAndPath = file;
                                    newItem.LineNumber = String.Format("{0} - Ln {1}", file, lineNumber.ToString());



                                    lstResults.Items.Add(newItem);

                                    searchResultCount++;

                                    lbFound.Content = String.Format("Found: {0}", searchResultCount.ToString());

                                }

                                lineNumber++;
                            }
                        }
                        fileCounter++;
                        prgSearch.Value = ((double)fileCounter / (double)files.Count) * 100;
                    }
                    else
                    {
                        SearchEnd();
                        return;
                    }

                }
                catch (Exception e)
                {
                    Logger.WriteToFile(e.Message);
                    ShowErrorIcon();
                }
                
            }
            SearchEnd();
            
        }

        private void SearchEnd()
        {
            lbFound.Content = "Search complete. " + lbFound.Content;
            btnSearch.IsEnabled = true;
        }
             
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            //string[] files = Directory.GetFiles(fbd.SelectedPath);
            //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
            if (tbFilePath.Text == "")
            {
                tbFilePath.Text = fbd.SelectedPath;
            }
            else
            {
                tbFilePath.Text += ";" + fbd.SelectedPath;
            }
            

            
        }

        private void lstResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            try
            {                
                var fileName = lstResults.SelectedValue.ToString();
                Process.Start(@fileName);
                
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
               ShowErrorIcon();
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancel = true;
        }

        private void imgError_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            var loggingDirectory = String.Format("{0}\\Logs\\errors.txt", Environment.CurrentDirectory);
            Process.Start(@loggingDirectory);
        }

        private void ShowErrorIcon()
        {
            imgError.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideErrorIcon()
        {
            imgError.Visibility = System.Windows.Visibility.Hidden;
        }

       
    }
}
