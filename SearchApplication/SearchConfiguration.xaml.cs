using Delimon.Win32.IO;
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
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace SearchApplication
{
    /// <summary>
    /// Interaction logic for SearchConfiguration.xaml
    /// </summary>
    public partial class SearchConfiguration : Window
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
                    cbCaseSensitive.IsChecked = Convert.ToBoolean(elements.CaseSensitive);
                    cbSearchFileNames.IsChecked = Convert.ToBoolean(elements.SearchFileNames);
                    tbIgnoreFolders.Text = elements.IgnoreFolders.ToString();
                }

            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            CreateOrUpdateXmlConfiguration();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

            XDocument doc = new XDocument(new XElement("Configuration",
                                                   new XAttribute("CaseSensitive", cbCaseSensitive.IsChecked.Value.ToString()),
                                                   new XAttribute("SearchFileNames", cbSearchFileNames.IsChecked.Value.ToString()),
                                                   new XAttribute("IgnoreFolders", tbIgnoreFolders.Text.Trim())));
            doc.Save(filename);
        }
    }
}
