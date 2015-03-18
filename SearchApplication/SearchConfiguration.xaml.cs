using Delimon.Win32.IO;
using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace SearchApplication
{
    /// <summary>
    /// Interaction logic for SearchConfiguration.xaml
    /// </summary>
    public partial class SearchConfiguration
    {
        public SearchConfiguration()
        {
            InitializeComponent();

            LoadForm();
        }

        private void LoadForm()
        {
            string filename = String.Format("{0}\\configuration\\configuration.xml", Environment.CurrentDirectory);

            if (File.Exists(filename))
            {
                XDocument doc = XDocument.Load(filename);

                var elements = (from p in doc.Descendants("Configuration") select new { CaseSensitive = p.Attribute("CaseSensitive").Value, SearchFileNames = p.Attribute("SearchFileNames").Value, IgnoreFolders = p.Attribute("IgnoreFolders").Value }).First();

                if (elements != null)
                {
                    CbCaseSensitive.IsChecked = Convert.ToBoolean(elements.CaseSensitive);
                    CbSearchFileNames.IsChecked = Convert.ToBoolean(elements.SearchFileNames);
                    TbIgnoreFolders.Text = elements.IgnoreFolders;
                }

            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            CreateOrUpdateXmlConfiguration();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateOrUpdateXmlConfiguration()
        {           

            string filename = String.Format("{0}\\configuration\\configuration.xml", Environment.CurrentDirectory);

            if (!Directory.Exists(String.Format("{0}\\configuration", Environment.CurrentDirectory)))
            {
                Directory.CreateDirectory(String.Format("{0}\\configuration", Environment.CurrentDirectory));
            }
            
            
            if (File.Exists(filename))
            {
                File.Delete(filename);               
            }

            var doc = new XDocument(new XElement("Configuration",
                                                   new XAttribute("CaseSensitive", (CbCaseSensitive.IsChecked != null && CbCaseSensitive.IsChecked.Value).ToString()),
                                                   new XAttribute("SearchFileNames", (CbSearchFileNames.IsChecked != null && CbSearchFileNames.IsChecked.Value).ToString()),
                                                   new XAttribute("IgnoreFolders", TbIgnoreFolders.Text.Trim())));
            doc.Save(filename);
        }
    }
}
