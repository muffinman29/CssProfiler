using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
//using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using SearchApplication.BusinessLogic;
using System.Windows.Interop;
using System.Drawing;
using Delimon.Win32.IO;
using System.Xml.Linq;

namespace SearchApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FileSearch
    {
        bool _cancel;
        bool _hasError;

        private SearchConfig _config;
        public FileSearch()
        {
            _hasError = false;
            InitializeComponent();
            ImgError.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            ImgError.Width = ImgError.Source.Width;
            ImgError.Height = ImgError.Source.Height;
            ImgError.Visibility = Visibility.Hidden;

            _config = new SearchConfig();
        }

        private bool ValidateFields()
        {
            return !(String.IsNullOrEmpty(TbFileExtensions.Text.Trim()) || String.IsNullOrEmpty(TbFilePath.Text.Trim()) || String.IsNullOrEmpty(TbSearchCriteria.Text.Trim()));
        }

        private void ClearScreen()
        {
            PrgSearch.Value = 0;
            _cancel = false;
            BtnSearch.IsEnabled = false;
            LstResults.SelectedIndex = -1;
            LstResults.Items.Clear();
            LbNumberOfFiles.Content = "Searching, please wait...";
            ImgError.Visibility = Visibility.Hidden;
            LbFound.Content = "";
        }

        private IEnumerable<string> IgnoredFolders()
        {
            return _config.IgnoreFolders.Split(';');
        }

        private bool CaseSensitive()
        {
            return _config.CaseSensitive;
        }

/*
        private bool SearchFileNames()
        {
            return _config.SearchFileNames;
        }
*/

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                System.Windows.Forms.MessageBox.Show(@"Unable to perform the search. Please make sure you have entered the correct information and try again.", @"Error");
                EnableControls();
                return;
            }

            ClearScreen();

            var directories = new List<string>();
            var rootDirectories = TbFilePath.Text.Split(';');
            //int counter = 0;

            foreach (var item in rootDirectories)
            {
                string item1 = item;
                List<string> directories1 = directories;
                LbFound.Content = "Getting directories...";
                var dirs = await Task.Factory.StartNew(() => GetDirectories(item1, directories1));

               //counter++;
               directories.AddRange(dirs);
            }

            LbFound.Content = "Directories collected. Sorting list...";
            directories = directories.Distinct().ToList();

            string[] fileExtensions = TbFileExtensions.Text.Split(',');
           
            var files = new List<string>();
            LbFound.Content = "Searching in files...";
            files.AddRange(GetFilesByExtension(fileExtensions, directories, files));
            files = files.Distinct().ToList();
            files.Sort();
            directories.Sort();
            LbFound.Content = "Search complete. Displaying results.";
            DisplayResultInformation(directories, files);

            if (_hasError)
                ShowErrorIcon();
        }

        private void EnableControls()
        {
            PrgSearch.Value = 0;
            _cancel = false;
            BtnSearch.IsEnabled = true;
            LstResults.SelectedIndex = -1;
            LstResults.Items.Clear();
            LbNumberOfFiles.Content = "";
            ImgError.Visibility = Visibility.Hidden;
            LbFound.Content = "";
        }

        private List<string> GetDirectories(string rootDirectory, List<string> directories)
        {   
            string[] subDirs = null;
            IEnumerable<string> ignoredDirectories = IgnoredFolders();

            try
            {
                subDirs = Directory.GetDirectories(rootDirectory, "*", SearchOption.TopDirectoryOnly)
                            .Where(x => !ignoredDirectories.Contains(x))
                            .ToArray();

                directories.AddRange(subDirs);
            }
            catch (Exception e)
            {
                Logger.WriteToFile(e.Message);
            }

            try
            {
                if (subDirs != null)
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

        private IEnumerable<string> GetFilesByExtension(string[] fileExtensions, IEnumerable<string> directories, List<string> files)
        {           
            for (var i = 0; i < fileExtensions.Count(); i++)
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
                    _hasError = true;
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
            LbNumberOfFiles.Content = String.Format("Number of folders: {0}. Files found: {1}", directories.Count, files.Count);
            LbFound.Content = "Found: 0";

            int searchResultCount = 0;
            int fileCounter = 0;
            foreach (var file in files)
            {
                try
                {
                    if (!_cancel)
                    {
                        using (var reader = new System.IO.StreamReader(file))
                        {
                            int lineNumber = 1;                            
                            string lineNumbers = "";
                            bool foundResult = false;
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                //bool caseSensitiveSearch = cbCaseSensitive.IsChecked.Value;
                                
                                if ((CaseSensitive() && CaseSensitiveSearch(line, TbSearchCriteria.Text))
                                    || (!CaseSensitive() && CaseInsensitiveSearch(line, TbSearchCriteria.Text)))
                                {
                                    searchResultCount++;
                                    LbFound.Content = String.Format("Found: {0}", searchResultCount);
                                    lineNumbers += String.Format("{0}, ", lineNumber);
                                    foundResult = true;
                                }                                                           

                                lineNumber++;
                            }
                            if (foundResult)
                            {
                                var newItem = new SearchResultData();
                                lineNumbers = lineNumbers.Remove(lineNumbers.LastIndexOf(','));
                                newItem.FileNameAndPath = file;
                                newItem.LineNumber = String.Format("{0} - Ln {1}", file, lineNumbers);
                                AddResultToList(newItem);
                            }
                            
                        }
                        fileCounter++;
                        PrgSearch.Value = (fileCounter / (double)files.Count) * 100;
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
            LstResults.Items.Add(newItem);            
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
            LbFound.Content = "Search complete. " + LbFound.Content;
            BtnSearch.IsEnabled = true;
        }
             
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.ShowDialog();

            if (TbFilePath.Text == "")
            {
                TbFilePath.Text = fbd.SelectedPath;
            }
            else
            {
                TbFilePath.Text += ";" + fbd.SelectedPath;
            }  
        }        

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cancel = true;
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
                System.Windows.Forms.MessageBox.Show(@"The error log does not exist on this sytem.", @"Error");
            }
            
        }

        private void ShowErrorIcon()
        {
            ImgError.Visibility = Visibility.Visible;
        }

/*
        private void HideErrorIcon()
        {
            ImgError.Visibility = Visibility.Hidden;
        }
*/

        private void lstResults_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e == null) return;
            if (e.OriginalSource == null) return;
            var originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null) return;
            var item = ItemsControl.ContainerFromElement(LstResults, originalSource) as ListBoxItem;
            if (item == null || !item.IsSelected) return;
            try
            {
                var currentItem = item.Content as SearchResultData;
                if (currentItem == null) return;
                var fileName = currentItem.FileNameAndPath; 
                Process.Start(@fileName);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
                ShowErrorIcon();
            }
        }

        private void viewErrorLog_Click(object sender, RoutedEventArgs e)
        {
            DisplayErrorLog();
        }

        private void closeWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (LstResults.SelectedIndex > -1)
            {
                System.Windows.Clipboard.SetText(LstResults.SelectedValue.ToString());
            }
        }

        private void saveSearchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = @"Extensible Markup Language(*.xml)|*.xml|All(*.*)|*" };

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                
                var doc = new XDocument(new XElement("Main", 
                                                    new XAttribute("FolderPaths", TbFilePath.Text.Trim()), 
                                                    new XAttribute("SearchTerms", TbSearchCriteria.Text.Trim()), 
                                                    new XAttribute("FileExtensions", TbFileExtensions.Text.Trim())));               

                doc.Save(dialog.FileName);              
            }
        }

        private void loadSearchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = @"Extensible Markup Language(*.xml)|*.xml|All(*.*)|*" };

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                XDocument doc = XDocument.Load(dialog.FileName);

                var xml = from p in doc.Descendants("Main") select new 
                            { 
                                FolderPaths = p.Attribute("FolderPaths").Value, 
                                SearchTerms = p.Attribute("SearchTerms").Value, 
                                FileExtensions = p.Attribute("FileExtensions").Value
                            };

                foreach (var item in xml)
                {
                    TbFileExtensions.Text = item.FileExtensions;
                    TbFilePath.Text = item.FolderPaths;
                    TbSearchCriteria.Text = item.SearchTerms;                    
                }                
            }
        }

        private void searchOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var config = new SearchConfiguration();
            config.Closed += config_Closed;
            config.Owner = this;
            config.Show();
        }

        void config_Closed(object sender, EventArgs e)
        {
            _config = new SearchConfig();
        }

       
    }
}
