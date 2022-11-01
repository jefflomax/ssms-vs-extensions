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
using SourceControlDeepLinks.Options;

namespace SourceControlDeepLinks
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuids.SourceControlDeepLinksString)]
	[ProvideOptionPage( typeof( OptionsProvider.ExtensionOptionsProv ), OptionsPageCategoryName, OptionsPageName, 0, 0, true )]
	[ProvideProfile( typeof( OptionsProvider.ExtensionOptionsProv ), OptionsPageCategoryName, OptionsPageName, 0, 0, true )]
	[ProvideOptionPage( typeof( OptionsProvider1.Provider1OptionsProv ), OptionsPageCategoryName, Options1PageName, 0, 0, true, Sort = 1 )]
	[ProvideProfile( typeof( OptionsProvider1.Provider1OptionsProv ), OptionsPageCategoryName, Options1PageName, 0, 0, true )]
	[ProvideOptionPage( typeof( OptionsProvider2.Provider2OptionsProv ), OptionsPageCategoryName, Options2PageName, 0, 0, true, Sort = 2 )]
	[ProvideProfile( typeof( OptionsProvider2.Provider2OptionsProv ), OptionsPageCategoryName, Options2PageName, 0, 0, true )]
	[ProvideOptionPage( typeof( OptionsProvider3.Provider3OptionsProv ), OptionsPageCategoryName, Options3PageName, 0, 0, true, Sort = 3 )]
	[ProvideProfile( typeof( OptionsProvider3.Provider3OptionsProv ), OptionsPageCategoryName, Options3PageName, 0, 0, true )]
	public sealed class SourceControlDeepLinksPackage 
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

			// TODO: Switch pattern to Migrate settings from app.config
			// on first launch

			_extensionOutputPaneName = ExtensionOutputPane;
			_state = new ExtensionOptions();

			await GetOptionsFromVSSettingsAsync();

			await LogStartupInformationAsync( ExtensionOutputPane, Vsix.Version );

			// Could hook all 4 settings to notify on save
			// ExtensionOptions.Saved += OptionsSaved;
		}
		public ExtensionOptions GetOptionsState() => _state;

#if false
		private void OptionsSaved( ExtensionOptions extensionOptions )
		{
			_ = ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
			{
				await ToOutputPaneAsync( $"{ExtensionOutputPane} {extensionOptions}" );
			} );
		}
#endif

		public override async Task<ExtensionOptions> GetLiveSettingsInstanceAsync()
		{
			return await ExtensionOptions.GetLiveInstanceAsync();
		}

		public async Task<AllOptions> GetLiveOptionsInstanceAsync()
		{
			var general = await ExtensionOptions.GetLiveInstanceAsync();
			var provider1 = await Provider1Options.GetLiveInstanceAsync();
			var provider2 = await Provider2Options.GetLiveInstanceAsync();
			var provider3 = await Provider3Options.GetLiveInstanceAsync();
			var allOptions = new AllOptions(general, provider1, provider2, provider3);
			return allOptions;
		}
	}
}