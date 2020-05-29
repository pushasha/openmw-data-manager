using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Configuration;

namespace OpenMwDataManager
{
	public class MainViewModel
	{
		private readonly List<string> _nonDataLines = new List<string>();
		private readonly List<string> _contentLines = new List<string>();

		private string ConfigPath
		{
			get { return Path.Combine(OpenMwUserFilesPath, "openmw.cfg"); }
		}

		public ObservableCollection<DataLine> DataLines { get; set; } = new ObservableCollection<DataLine>();
		public RelayCommand SyncModsDirectoryCommand { get; }
		public RelayCommand<DataLine> MoveUpCommand { get; }
		public RelayCommand<DataLine> MoveDownCommand { get; }
		public RelayCommand SavePathsCommand { get; }

		public string OpenMwUserFilesPath { get; set; }
		public string MwInstallPath { get; set; }
		public string ModsDirectoryPath { get; set; }

		public MainViewModel()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			OpenMwUserFilesPath = ConfigurationManager.AppSettings["OpenMwUserFilesPath"];
			MwInstallPath = ConfigurationManager.AppSettings["MorrowindInstallPath"];
			ModsDirectoryPath = ConfigurationManager.AppSettings["ModsDirectoryPath"];

			SyncModsDirectoryCommand = new RelayCommand(CreateDataLinesForMods);
			MoveUpCommand = new RelayCommand<DataLine>(MoveUp);
			MoveDownCommand = new RelayCommand<DataLine>(MoveDown);
			SavePathsCommand = new RelayCommand(SavePaths);

			if (!HasValidPaths())
			{
				return;
			}

			ReadConfigFile();
		}

		private void MoveUp(DataLine line)
		{
			if (line == null) return;

			int oldIndex = DataLines.IndexOf(line);
			int newIndex = (oldIndex - 1 >= 0) ? oldIndex - 1 : 0;
			DataLines.Move(oldIndex, newIndex);
		}

		private void MoveDown(DataLine line)
		{
			if (line == null) return;

			int oldIndex = DataLines.IndexOf(line);
			int newIndex = (oldIndex + 1) <= (DataLines.Count - 1) ? oldIndex + 1 : DataLines.Count - 1;
			DataLines.Move(oldIndex, newIndex);
		}

		private void ReadConfigFile()
		{
			// Read in the config file
			IEnumerable<string> lines = File.ReadLines(ConfigPath);
			string installDataLine = null;
			foreach (string line in lines)
			{
				if (line.Contains(MwInstallPath))
				{
					installDataLine = line;
				}
				else if (line.StartsWith(DataLine.DataLinePrefix))
				{
					DataLines.Add(new DataLine(line));
				}
				else if (line.StartsWith("content"))
				{
					_contentLines.Add(line);
				}
				else
				{
					_nonDataLines.Add(line);
				}
			}

			_nonDataLines.Add(installDataLine);
		}

		private void CreateDataLinesForMods()
		{
			if (!HasValidPaths())
			{
				MessageBox.Show("You cannot sync your mods unless you specify valid install/user file paths!", "Invalid Paths", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			string[] modDirs = Directory.GetDirectories(ModsDirectoryPath);

			// remove lines that don't correspond to mod dirs
			for (int i = DataLines.Count - 1; i >= 0; i--)
			{
				DataLine line = DataLines[i];
				if (!modDirs.Contains(line.DirPath))
				{
					DataLines.RemoveAt(i);
				}
			}

			// add lines for new mod directories
			foreach (string dir in modDirs)
			{
				if (DataLines.Any(line => line.DirPath.Equals(dir)))
				{
					continue;
				}

				DataLine modLine = DataLine.FromModPath(dir);
				DataLines.Add(modLine);
			}

			WriteDataLinesToConfig();
			MessageBox.Show("Mods synced!", "Operation Complete", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void WriteDataLinesToConfig()
		{
			List<string> dataLines = DataLines.Select(line => line.Line).ToList();
			IEnumerable<string> concat = _nonDataLines.Concat(dataLines).Concat(_contentLines);
			File.WriteAllLines(ConfigPath, concat);
		}

		private void SavePaths()
		{
			if (!HasValidPaths())
			{
				MessageBox.Show("You cannot save blank paths!", "Invalid Paths", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			SetAppConfigValue(config, "OpenMwUserFilesPath", OpenMwUserFilesPath);
			SetAppConfigValue(config, "MorrowindInstallPath", MwInstallPath);
			SetAppConfigValue(config, "ModsDirectoryPath", ModsDirectoryPath);
			config.Save(ConfigurationSaveMode.Modified);
			MessageBox.Show("Paths saved!", "Operation Complete", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private bool HasValidPaths()
		{
			return !String.IsNullOrWhiteSpace(OpenMwUserFilesPath) && !String.IsNullOrWhiteSpace(MwInstallPath) && !String.IsNullOrWhiteSpace(ModsDirectoryPath);
		}

		private static void SetAppConfigValue(Configuration config, string key, string value)
		{
			var entry = config.AppSettings.Settings[key];

			if (entry == null)
			{
				config.AppSettings.Settings.Add(key, value);
			}
			else
			{
				config.AppSettings.Settings[key].Value = value;
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			// Log the exception, display it, etc
			Exception theException = e.ExceptionObject as Exception;
			MessageBox.Show($"Exception: {theException.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
			Application.Current.Shutdown();
		}
	}
}
