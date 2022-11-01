using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlDeepLinks.Helpers
{
	public class SettingsHelper
	{
		private string _collectionName;
		public SettingsHelper( Type baseOptionModelType )
		{
			_collectionName = baseOptionModelType.FullName;
		}

		public async Task<bool> PropertyExistsAsync( Type collectionType, string property )
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
#pragma warning disable CVST001 // Cast interop services to their specific type
			var manager = await VS.Services.GetSettingsManagerAsync() as IVsSettingsManager;
#pragma warning restore CVST001 // Cast interop services to their specific type
			var scope = SettingsScope.UserSettings;
			manager.GetReadOnlySettingsStore( (uint)scope, out var vsSettingsStore );
			var collectionName = collectionType.FullName;
			vsSettingsStore.PropertyExists
			(
				collectionName,
				property,
				out var propExists
			);
			return propExists != 0;
		}
	}
}
