using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedSrc.Helpers
{
	public class AppSettingsHelper
	{
		private readonly KeyValueConfigurationCollection _appSettings;
		public AppSettingsHelper()
		{
			// Default ConfigurationManger AppSettings will be the DevEnv.exe one
			// Need to reference our config
			var appConfigPath = System.IO
				.Path
				.GetDirectoryName(this.GetType().Assembly.Location);

			var configMap = new ExeConfigurationFileMap();
			configMap.ExeConfigFilename = System.IO
				.Path
				.Combine(appConfigPath, "App.config");
			Configuration config = ConfigurationManager
				.OpenMappedExeConfiguration
				(
					configMap,
					ConfigurationUserLevel.None
				);
			_appSettings = config
				.AppSettings
				.Settings;
		}

		public string GetString(string key) => _appSettings[key].Value;
	}
}
