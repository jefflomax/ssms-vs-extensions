global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
//global using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using SharedSrc.Commands;
using SharedSrc.Interfaces;
using System.Runtime.InteropServices;
using System.Threading;
using static SourceControlDeepLinks.Resources.Constants;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Settings;
using SourceControlDeepLinks.Helpers;

namespace SourceControlDeepLinks
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuids.SourceControlDeepLinksString)]
	[ProvideOptionPage( typeof( OptionsProvider.ExtensionOptionsProv ), OptionsPageCategoryName, OptionsPageName, 0, 0, true )]
	[ProvideProfile( typeof( OptionsProvider.ExtensionOptionsProv ), OptionsPageCategoryName, OptionsPageName, 0, 0, true )]
	public sealed class SourceControlDeepLinksVS2022Package 
		: BasePackage<ExtensionOptions>, IBasePackage<ExtensionOptions>
	{
		protected override async Task InitializeAsync
		(
			CancellationToken cancellationToken,
			IProgress<ServiceProgressData> progress
		)
		{
			// Setup output pane first so we can log there
			var outputWindowPane = await SetupOutputPaneAsync( ExtensionOutputPane );

			await this.RegisterCommandsAsync();
			this.RegisterToolWindows();

			var settingsHelper = new SettingsHelper( typeof( ExtensionOptions ) );
			var propertyInSettings = nameof( ExtensionOptions.Provider );
			if( ! await settingsHelper.PropertyExistsAsync( propertyInSettings ) )
			{
				await outputWindowPane.WriteLineAsync( $"Property {propertyInSettings} not found in Setting" );
			}

			_extensionOutputPaneName = ExtensionOutputPane;
			_state = new ExtensionOptions();

			await GetOptionsFromVSSettingsAsync();

			await LogStartupInformationAsync( ExtensionOutputPane, Vsix.Version );

			ExtensionOptions.Saved += OptionsSaved;
		}
		public ExtensionOptions GetOptionsState() => _state;

		private void OptionsSaved( ExtensionOptions extensionOptions )
		{
			_ = ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
			{
				await ToOutputPaneAsync( $"{ExtensionOutputPane} {extensionOptions}" );
			} );
		}

		public override async Task<ExtensionOptions> GetLiveSettingsInstanceAsync()
		{
			return await ExtensionOptions.GetLiveInstanceAsync();
		}
	}
}