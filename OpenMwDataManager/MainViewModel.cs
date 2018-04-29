using OpenMwDataManager.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace OpenMwDataManager
{
	public class MainViewModel
	{
		private const string ModsDirectoryName = "mods";
		private readonly List<string> _nonDataLines = new List<string>();
		private readonly List<string> _contentLines = new List<string>();
		private readonly string _configPath = Path.Combine(Settings.Default.OpenMwUserFilesPath, "openmw.cfg");

		public ObservableCollection<DataLine> DataLines {get; set;} = new ObservableCollection<DataLine>();
		public RelayCommand SyncModsDirectoryCommand { get; }
		public RelayCommand<DataLine> MoveUpCommand { get; }
		public RelayCommand<DataLine> MoveDownCommand { get; }

		public MainViewModel()
		{
			SyncModsDirectoryCommand = new RelayCommand(CreateDataLinesForMods);
			MoveUpCommand = new RelayCommand<DataLine>(MoveUp);
			MoveDownCommand = new RelayCommand<DataLine>(MoveDown);
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
			IEnumerable<string> lines = File.ReadLines(_configPath);
			string installDataLine = null;
			foreach (string line in lines)
			{
				if (line.Contains(Settings.Default.MorrowindInstallPath))
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
			string modsDir = Path.Combine(Settings.Default.OpenMwUserFilesPath, ModsDirectoryName);
			string[] modDirs = Directory.GetDirectories(modsDir);

			// remove lines that don't correspond to mod dirs
			foreach (DataLine line in DataLines)
			{
				if (!modDirs.Contains(line.DirPath))
				{
					DataLines.Remove(line);
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
		}

		private void WriteDataLinesToConfig()
		{
			List<string> dataLines = DataLines.Select(line => line.Line).ToList();
			IEnumerable<string> concat = _nonDataLines.Concat(dataLines).Concat(_contentLines);
			File.WriteAllLines(_configPath, concat);
		}
	}
}
