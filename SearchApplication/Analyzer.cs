﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SearchApplication
{
    class Analyzer
    {

        public int GetDirectoryCount(string rootDirectory)
        {            

            var directories = Directory.GetDirectories(rootDirectory).ToList();
            var subDirectories = GetSubDirectories(directories);

            return directories.Count + subDirectories.Count + 1;
        }

        private List<string> GetSubDirectories(IEnumerable<string> parentDirectories)
        {
            var subDirectories = new List<string>();
            foreach (string directory in parentDirectories)
            {
                try
                {
                    subDirectories.AddRange(Directory.GetDirectories(directory).ToList());
                }
                catch (Exception)
                {

                }
            }

            return subDirectories;
            
        }
        public List<string> GetDirectories(string rootDirectory)
        {
            var files = new List<string>();
            
            if (String.IsNullOrEmpty(rootDirectory))
            {
                files.Add("Error");
                return files;
            }

            var directories = Directory.GetDirectories(rootDirectory).ToList();

            var subDirectories = GetSubDirectories(directories);
            directories.AddRange(subDirectories);
            directories.Add(rootDirectory);

            const string fileExtension = ".csv";
            
            Parallel.ForEach(directories, directory =>
            {
                try { files.AddRange(Directory.EnumerateFiles(directory).Where(x => FileExtension(x).Contains(fileExtension))); }
                catch { }
            }
                );

            return files;

            //DisplayResultInformation(directories.Count, files.Count, files);
        }

        private string FileExtension(string file)
        {
            var fileParts = file.Split('.');

            return "." + fileParts[fileParts.Length - 1];
        }

        public void GetFiles()
        { }
    }
}
