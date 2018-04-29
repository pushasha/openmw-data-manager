using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using OpenMwDataManager.Properties;

namespace OpenMwDataManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Load user settings and make sure they're all there
			string morrowindInstallPath = Settings.Default.MorrowindInstallPath;
			string openMwUserFilesPath = Settings.Default.OpenMwUserFilesPath;

			if (String.IsNullOrWhiteSpace(morrowindInstallPath))
			{
				// TODO: Prompt user
				Settings.Default.MorrowindInstallPath = @"C:\Program Files (x86)\Steam\steamapps\common\Morrowind";
			}

			if (String.IsNullOrWhiteSpace(openMwUserFilesPath))
			{
				// TODO: Prompt user
				Settings.Default.OpenMwUserFilesPath = @"C:\Users\Sarah\Documents\My Games\OpenMW";
			}

			Settings.Default.Save(); // save changes to settings
		}
	}
}
