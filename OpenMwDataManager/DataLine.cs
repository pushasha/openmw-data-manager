using System;
using System.IO;

namespace OpenMwDataManager
{
	public class DataLine
	{
		public const string DataLinePrefix = "data=\"";

		public DataLine(string dataLine)
		{
			Line = dataLine;
			DirPath = GetPathFromDataLine(dataLine);
			Name = Path.GetFileNameWithoutExtension(DirPath);
		}

		public string Line { get; set; }
		public string Name { get; set; }
		public string DirPath { get; set; }

		public static string GetPathFromDataLine(string dataLine)
		{
			int pathStartIndex = DataLinePrefix.Length;
			int pathEndIndex = dataLine.LastIndexOf("\"", StringComparison.Ordinal);
			return dataLine.Substring(pathStartIndex, pathEndIndex - pathStartIndex);
		}

		public static DataLine FromModPath(string modPath)
		{
			string dataLine = $"{DataLinePrefix}{modPath}\"";
			return new DataLine(dataLine);
		}
	}
}
