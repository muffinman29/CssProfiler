using System.Collections.Generic;
using SearchApplication.Annotations;

namespace SearchApplication.BusinessLogic
{
	class FileSearcher
	{
/*
	    private bool Error { [UsedImplicitly] get; set; }
*/

	    private List<string> Directories { [UsedImplicitly] get; set; }
	    private List<string> Files { [UsedImplicitly] get; set; }

		//private readonly bool _cancel;

		public FileSearcher()
		{
			Directories = new List<string>();
			Files = new List<string>();
			//_cancel = false;
		}

/*
		public void GetDirectories(string filePath, string fileExtension)
		{
			var hasError = false;

			var rootDirectories = filePath.Split(';');
			var tempRootDirectories = new List<string>();
			foreach (var rootDirectory in rootDirectories)
			{
				
				if (String.IsNullOrEmpty(rootDirectory))
				{
					System.Windows.Forms.MessageBox.Show(@"Please enter a path to search.", @"Error");
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

			foreach (var directory in Directories)
			{
			    try
			    {
			        tempRootDirectories.AddRange(Directory.GetDirectories(directory, "*", SearchOption.AllDirectories).ToList());
			    }
			    catch (Exception e)
			    {
			        Logger.WriteToFile(e.Message);
			        Error = true;
			    }
			}

		    var fileExtensions = fileExtension.Split(',');
			for (var i = 0; i < fileExtensions.Count(); i++)
			{
				fileExtensions[i] = fileExtensions[i].Trim();
			}

			Parallel.ForEach(Directories, directory =>
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
*/

/*
		private string FileExtension(string file)
		{
			var fileParts = file.Split('.');
			return "." + fileParts[fileParts.Length - 1];
		}
*/

/*
		private async Task<List<SearchResultData>> DisplayResultInformation(string searchCriteria)
		{
			//lbNumberOfFiles.Content = String.Format("Number of folders: {0}. Files found: {1}", directories.Count.ToString(), files.Count.ToString());
			//lbFound.Content = "Found: 0";

			var searchResults = new List<SearchResultData>();
		    int searchResultCount = 0;
		    int fileCounter = 0;
			foreach (var file in Files)
			{
				//try
				//{
					using (var reader = new System.IO.StreamReader(file))
					{
						int lineNumber = 1;
					    string line;
					    while ((line = await reader.ReadLineAsync()) != null && !_cancel)
						{
							if (line.Contains(searchCriteria))
							{
								var newItem = new SearchResultData
								{
								    FileNameAndPath = file,
								    LineNumber = String.Format("{0} - Ln {1}", file, lineNumber)
								};

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
*/

	}
}
