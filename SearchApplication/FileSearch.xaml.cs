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
using System.Xml;
using System.Xml.Linq;

namespace SearchApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FileSearch : Window
    {
        bool cancel = false;
        bool hasError;
        public FileSearch()
        {
            hasError = false;
            InitializeComponent();
            imgError.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            imgError.Width = imgError.Source.Width;
            imgError.Height = imgError.Source.Height;
            imgError.Visibility = Visibility.Hidden;
        }

        private bool ValidateFields()
        {
            return !(String.IsNullOrEmpty(tbFileExtensions.Text.Trim()) || String.IsNullOrEmpty(tbFilePath.Text.Trim()) || String.IsNullOrEmpty(tbSearchCriteria.Text.Trim()));
        }

        private void ClearScreen()
        {
            prgSearch.Value = 0;
            cancel = false;
            btnSearch.IsEnabled = false;
            lstResults.SelectedIndex = -1;
            lstResults.Items.Clear();
            lbNumberOfFiles.Content = "Searching, please wait...";
            imgError.Visibility = System.Windows.Visibility.Hidden;
            lbFound.Content = "";
        }

        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                System.Windows.Forms.MessageBox.Show("Unable to perform the search. Please make sure you have entered the correct information and try again.", "Error");
                EnableControls();
                return;
            }

            ClearScreen();

            var directories = new List<string>();
            var rootDirectories = tbFilePath.Text.Split(';');
            int counter = 0;

            foreach (var item in rootDirectories)
            {
               var dirs = await Task.Factory.StartNew(() => GetDirectories(item, directories));
               counter++;
               prgSearch.Value = ((double)rootDirectories.Count() / (double)counter) * 100;                
               directories.AddRange(dirs);
            }

            directories = directories.Distinct().ToList();

            string[] fileExtensions = tbFileExtensions.Text.Split(',');
           
            List<string> files = new List<string>();            
            files.AddRange(GetFilesByExtension(fileExtensions, directories, files));
            files = files.Distinct().ToList();
            files.Sort();
            directories.Sort();

            DisplayResultInformation(directories, files);

            if (hasError)
                ShowErrorIcon();
        }

        private void EnableControls()
        {
            prgSearch.Value = 0;
            cancel = false;
            btnSearch.IsEnabled = true;
            lstResults.SelectedIndex = -1;
            lstResults.Items.Clear();
            lbNumberOfFiles.Content = "";
            imgError.Visibility = System.Windows.Visibility.Hidden;
            lbFound.Content = "";
        }

        private List<string> GetDirectories(string rootDirectory, List<string> directories)
        {   
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
                    GetDirectories(dirInfo, directories);
                }                
            }
            catch (Exception e)
            {
                Logger.WriteToFile(e.Message);
            }

            return directories.Distinct().ToList();            
        }

        private List<string> GetFilesByExtension(string[] fileExtensions, List<string> directories, List<string> files)
        {           
            for (int i = 0; i < fileExtensions.Count(); i++)
            {
                fileExtensions[i] = fileExtensions[i].Trim();
            }

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
                                
                                if ((caseSensitiveSearch && CaseSensitiveSearch(line, tbSearchCriteria.Text)) 
                                    || (!caseSensitiveSearch && CaseInsensitiveSearch(line, tbSearchCriteria.Text)))
                                {
                                    searchResultCount++;
                                    lbFound.Content = String.Format("Found: {0}", searchResultCount.ToString());
                                    lineNumbers += String.Format("{0}, ", lineNumber.ToString());
                                    foundResult = true;
                                }                                                           

                                lineNumber++;
                            }
                            if (foundResult)
                            {
                                SearchResultData newItem = new SearchResultData();
                                lineNumbers = lineNumbers.Remove(lineNumbers.LastIndexOf(','));
                                newItem.FileNameAndPath = file;
                                newItem.LineNumber = String.Format("{0} - Ln {1}", file, lineNumbers);
                                AddResultToList(newItem);
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

        private void AddResultToList(SearchResultData newItem)
        {                       
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

            if (tbFilePath.Text == "")
            {
                tbFilePath.Text = fbd.SelectedPath;
            }
            else
            {
                tbFilePath.Text += ";" + fbd.SelectedPath;
            }  
        }        

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancel = true;
        }

        private void imgError_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DisplayErrorLog();
        }

        private void DisplayErrorLog()
        {
            var loggingDirectory = String.Format("{0}\\Logs\\errors.txt", Environment.CurrentDirectory);
            if (File.Exists(loggingDirectory))
            {
                Process.Start(@loggingDirectory);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("The error log does not exist on this sytem.", "Error");
            }
            
        }

        private void ShowErrorIcon()
        {
            imgError.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideErrorIcon()
        {
            imgError.Visibility = System.Windows.Visibility.Hidden;
        }

        private void lstResults_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(lstResults, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null && item.IsSelected)
            {
                try
                {
                    var currentItem = item.Content as SearchResultData;
                    var fileName = currentItem.FileNameAndPath; 
                    Process.Start(@fileName);

                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(ex.Message);
                    ShowErrorIcon();
                }
            }
        }

        private void viewErrorLog_Click(object sender, RoutedEventArgs e)
        {
            DisplayErrorLog();
        }

        private void closeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (lstResults.SelectedIndex > -1)
            {
                System.Windows.Clipboard.SetText(lstResults.SelectedValue.ToString());
            }
        }

        private void saveSearchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filter = "Extensible Markup Language(*.xml)|*.xml|All(*.*)|*" };

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                
                XDocument doc = new XDocument(new XElement("Main", 
                                                    new XAttribute("FolderPaths", tbFilePath.Text.Trim()), 
                                                    new XAttribute("SearchTerms", tbSearchCriteria.Text.Trim()), 
                                                    new XAttribute("FileExtensions", tbFileExtensions.Text.Trim()),
                                                    new XAttribute("CaseSensitive", cbCaseSensitive.IsChecked.Value.ToString())));               

                doc.Save(dialog.FileName);              
            }
        }

        private void loadSearchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() { Filter = "Extensible Markup Language(*.xml)|*.xml|All(*.*)|*" };

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                XDocument doc = XDocument.Load(dialog.FileName);

                var xml = from p in doc.Descendants("Main") select new 
                            { 
                                FolderPaths = p.Attribute("FolderPaths").Value, 
                                SearchTerms = p.Attribute("SearchTerms").Value, 
                                FileExtensions = p.Attribute("FileExtensions").Value, 
                                CaseSensitive = p.Attribute("CaseSensitive").Value 
                            };

                foreach (var item in xml)
                {
                    tbFileExtensions.Text = item.FileExtensions;
                    tbFilePath.Text = item.FolderPaths;
                    tbSearchCriteria.Text = item.SearchTerms;
                    cbCaseSensitive.IsChecked = item.CaseSensitive == "True" ? true : false;
                }                
            }
        }

       
    }
}
