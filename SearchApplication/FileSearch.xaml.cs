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
        //private List<string> directories;
       // private List<string> files;
        FileSearcher mySearcher;
        bool hasError;
        public FileSearch()
        {
            hasError = false;
            InitializeComponent();
            imgError.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            imgError.Width = imgError.Source.Width;
            imgError.Height = imgError.Source.Height;
            imgError.Visibility = Visibility.Hidden;           

            //directories = new List<string>();
            //files = new List<string>();
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
            //directories.Clear();
            //files.Clear();

            //  1. Perform Search
            //  2. Get Directories

            //  Order does not matter
            //  Can have multiple root folders
            var directories = new List<string>();
            var rootDirectories = tbFilePath.Text.Split(';');
            //foreach (var item in rootDirectories)
            //{
            //   directories.AddRange(GetDirectories(item, directories));
            //}
            

            Parallel.ForEach(rootDirectories, (item) => directories.AddRange(GetDirectories(item, directories)));

            //  3. Get Files in Directories
            //  4. Get Matching Results




            //FileSearcher mySearcher = new FileSearcher();
            //mySearcher.GetDirectories(tbFilePath.Text, tbFileExtensions.Text);
            //directories = mySearcher.Directories;
            //files = mySearcher.Files;
            //if (mySearcher.Error)
            //{
            //    ShowErrorIcon();
            //}
           
            //List<Task> tasks = new List<Task>();
           // Parallel.ForEach(rootDirectories, (root) => GetDirectories(root));
            //foreach (var root in rootDirectories)
            //{

            //    //tasks.Add(Task.Factory.StartNew(() =>
            //    //GetDirectories(root)));
            //    Parallel.Invoke(() => GetDirectories(root));
                
            //}            

            //Task.WaitAll(tasks.ToArray());
            string[] fileExtensions = tbFileExtensions.Text.Split(',');
            //Parallel.Invoke(() => GetFilesByExtension(fileExtensions));
            //Task.Factory.StartNew(() => GetFilesByExtension(fileExtensions));

            //Task.WaitAny();
            List<string> files = new List<string>();            
            files.AddRange(GetFilesByExtension(fileExtensions, directories, files));
            DisplayResultInformation(directories, files);

            if (hasError)
                ShowErrorIcon();
        }

        private List<string> GetDirectories(string rootDirectory, List<string> directories)
        {

            //if (String.IsNullOrEmpty(tbFilePath.Text))
            //{
            //    System.Windows.Forms.MessageBox.Show("Please enter a path to search.", "Error");
            //    return;
            //}  
            //List<string> directories = new List<string>();
            
            string[] subDirs = null;

            try
            {
                subDirs = Directory.GetDirectories(rootDirectory, "*", SearchOption.TopDirectoryOnly);
                //Parallel.ForEach(subDirs, (item) => directories.Add(item));
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
                //Parallel.ForEach(subDirs, (dirInfo) => GetDirectories(dirInfo));
                foreach (var dirInfo in subDirs)
                {
                    GetDirectories(dirInfo, directories);
                }
                
            }
            catch (Exception e)
            {
                Logger.WriteToFile(e.Message);
            }

            return directories;
                   

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

        private List<string> GetFilesByExtension(string[] fileExtensions, List<string> directories, List<string> files)
        {
            //bool hasError = false;
            //string[] fileExtensions = tbFileExtensions.Text.Split(',');

            for (int i = 0; i < fileExtensions.Count(); i++)
            {
                fileExtensions[i] = fileExtensions[i].Trim();
            }

            //List<Task> tasks = new List<Task>();

            foreach (var directory in directories)
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
            }

            return files;

            //Task.WaitAll(tasks.ToArray());


            //Parallel.ForEach(directories, (directory) =>
            //{
            //    try
            //    {

            //        files.AddRange(Directory.GetFiles(directory).Where(x => fileExtensions.Contains(FileExtension(x.ToLower()))));
            //    }
            //    catch (Exception e)
            //    {
            //        Logger.WriteToFile(e.Message);
            //        hasError = true;
            //    }
            //});

            //if (hasError)
            //{
            //    ShowErrorIcon();
            //}
        
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

        private async void DisplayResultInformation(List<string> directories, List<string> files)
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
                            string lineNumbers = "";
                            bool foundResult = false;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {

                                bool caseSensitiveSearch = cbCaseSensitive.IsChecked.Value;
                                
                                if (caseSensitiveSearch && CaseSensitiveSearch(line, tbSearchCriteria.Text))
                                {
                                    //AddResultToList(file, lineNumber.ToString());
                                    searchResultCount++;
                                    lbFound.Content = String.Format("Found: {0}", searchResultCount.ToString());
                                    lineNumbers += String.Format("{0}, ", lineNumber.ToString());
                                    foundResult = true;
                                    //oldFileName = file;
                                }
                                else if (!caseSensitiveSearch && CaseInsensitiveSearch(line, tbSearchCriteria.Text))
                                {
                                    //AddResultToList(file, lineNumber.ToString());
                                    searchResultCount++;
                                    lbFound.Content = String.Format("Found: {0}", searchResultCount.ToString());
                                    lineNumbers += String.Format("{0}, ", lineNumber.ToString());
                                    foundResult = true;
                                    //oldFileName = file;
                                }                               

                                lineNumber++;
                            }
                            if (foundResult)
                            {
                                AddResultToList(file, lineNumbers);
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

        private void AddResultToList(string file, string lineNumber)
        {
            SearchResultData newItem = new SearchResultData();
            lineNumber = lineNumber.Remove(lineNumber.LastIndexOf(','));
            newItem.FileNameAndPath = file;
            newItem.LineNumber = String.Format("{0} - Ln {1}", file, lineNumber);
            lstResults.Items.Add(newItem);
        }

        private bool CaseSensitiveSearch(string searchString, string searchCriteria)
        {
            return searchString.Contains(searchCriteria);   
        }

        private bool CaseInsensitiveSearch(string searchString, string searchCriteria)
        {
            return searchString.ToLower().Contains(searchCriteria.ToLower());
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
