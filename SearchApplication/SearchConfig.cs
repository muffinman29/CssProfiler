using Delimon.Win32.IO;
using System;
using System.Linq;
using System.Xml.Linq;

namespace SearchApplication
{
    class SearchConfig
    {
        public SearchConfig()
        {
            LoadSearchConfiguration();
        }

        private void LoadSearchConfiguration()
        {
            string filename = String.Format("{0}\\configuration\\configuration.xml", Environment.CurrentDirectory);

            if (File.Exists(filename))
            {
                XDocument doc = XDocument.Load(filename);

                var elements = (from p in doc.Descendants("Configuration") select new { CaseSensitive = p.Attribute("CaseSensitive").Value, SearchFileNames = p.Attribute("SearchFileNames").Value, IgnoreFolders = p.Attribute("IgnoreFolders").Value }).First();

                if (elements != null)
                {
                    CaseSensitive = Convert.ToBoolean(elements.CaseSensitive);
                    SearchFileNames = Convert.ToBoolean(elements.SearchFileNames);
                    IgnoreFolders = elements.IgnoreFolders;
                }

            }
            else
            {
                CaseSensitive = false;
                SearchFileNames = false;
                IgnoreFolders = String.Empty;
            }
        }
        public bool CaseSensitive { get; private set; }
        public bool SearchFileNames { get; private set; }
        public string IgnoreFolders { get; private set; }
    }
}
