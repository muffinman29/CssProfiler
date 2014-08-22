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
            lstResults.Items.Clear();
            List<string> files = new List<string>();
            foreach (var item in ParseFileExtensions())
            {
                files.AddRange(Directory.EnumerateFiles(tbFilePath.Text).Where(x => x.EndsWith(item)));
            }
            
            
            foreach (var file in files)
            {  
                    var fileContents = File.ReadAllLines(file);

                    for (int i = 0; i < fileContents.Length; i++)
                    {
                        if (fileContents[i].Contains(tbSearchCriteria.Text))
                        {
                            lstResults.Items.Add(String.Format("{0} : Ln {1}", file, i.ToString()));
                        }
                    }      
            }
        }

        private string[] ParseFileExtensions()
        {
            return tbFileExtensions.Text.Split(',');
        }
    }
}
