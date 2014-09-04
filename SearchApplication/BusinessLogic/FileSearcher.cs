using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delimon.Win32.IO;

namespace SearchApplication.BusinessLogic
{
	class FileSearcher
	{        
		public bool Error { get; set; }

		public List<string> Directories { get; set; }
		public List<string> Files { get; set; }

		private bool cancel;

		public FileSearcher()
		{
			Directories = new List<string>();
			Files = new List<string>();
			cancel = false;
		}

		public void GetDirectories(string filePath, string fileExtension)
		{
			bool hasError = false;

			var rootDirectories = filePath.Split(';');
			var tempRootDirectories = new List<string>();
			foreach (var rootDirectory in rootDirectories)
			{
				
				if (String.IsNullOrEmpty(rootDirectory))
				{
					System.Windows.Forms.MessageBox.Show("Please enter a path to search.", "Error");
					return;
				}

				try
				{
					tempRootDirectories = Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories).ToList();
				}
				catch (Exception e)
				{
					Logger.WriteToFile(e.Message);
					Error = true;
				}
				

				

				

				//Directory.GetDirectories("", "", SearchOption.AllDirectories);

				tempRootDirectories.Add(rootDirectory);

				Directories.AddRange(tempRootDirectories);
			}

			for (int i = 0; i < Directories.Count; i++)
			{
				try
				{
					tempRootDirectories.AddRange(Directory.GetDirectories(Directories[i], "*", SearchOption.AllDirectories).ToList());
				}
				catch (Exception e)
				{
					Logger.WriteToFile(e.Message);
					Error = true;
				}
			}

			string[] fileExtensions = fileExtension.Split(',');
			for (int i = 0; i < fileExtensions.Count(); i++)
			{
				fileExtensions[i] = fileExtensions[i].Trim();
			}

			Parallel.ForEach(Directories, (directory) =>
			{
				try
				{
					Files.AddRange(Directory.GetFiles(directory).Where(x => fileExtensions.Contains(FileExtension(x))));
				}
				catch (Exception e)
				{
					Logger.WriteToFile(e.Message);
					hasError = true;
				}
			});

			//foreach (var directory in Directories)
			//{
			//    try 
			//    {	        
			//            Files.AddRange(Directory.GetFiles(directory).Where(x => fileExtensions.Contains(FileExtension(x))));
			//    }
			//    catch (Exception e)
			//    {
		
			//       Logger.WriteToFile(e.Message);
			//        hasError = true;
			//    }
			//}

			//Directories.ForEach(directory => Files.AddRange(Directory.GetFiles(directory).Where(x => fileExtensions.Contains(FileExtension(x))));

			if (hasError)
			{
				Error = true;
			}
		}

		private string FileExtension(string file)
		{
			var fileParts = file.Split('.');
			return "." + fileParts[fileParts.Length - 1];
		}

		private async Task<List<SearchResultData>> DisplayResultInformation(string searchCriteria)
		{
			//lbNumberOfFiles.Content = String.Format("Number of folders: {0}. Files found: {1}", directories.Count.ToString(), files.Count.ToString());
			//lbFound.Content = "Found: 0";

			List<SearchResultData> searchResults = new List<SearchResultData>();
			string line;
			int searchResultCount = 0;
			int fileCounter = 0;
			foreach (var file in Files)
			{
				//try
				//{
					using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
					{
						int lineNumber = 1;
						while ((line = await reader.ReadLineAsync()) != null && !cancel)
						{
							if (line.Contains(searchCriteria))
							{
								SearchResultData newItem = new SearchResultData();
								newItem.FileNameAndPath = file;
								newItem.LineNumber = String.Format("{0} - Ln {1}", file, lineNumber.ToString());

								searchResults.Add(newItem);
								
								//lstResults.Items.Add(newItem);

								searchResultCount++;

								//lbFound.Content = String.Format("Found: {0}", searchResultCount.ToString());

							}

							lineNumber++;
						}
					}
					fileCounter++;
					//prgSearch.Value = ((double)fileCounter / (double)Files.Count) * 100;

				//}
				//catch (Exception e)
				//{
				//    //Logger.WriteToFile(e.Message);
				//    //Error = true;
				//}

			}

			return searchResults;

			//lbFound.Content = "Search complete. " + lbFound.Content;
			//btnSearch.IsEnabled = true;
		}

	}
}
