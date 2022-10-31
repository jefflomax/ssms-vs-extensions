using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using static SourceControlDeepLinks.Resources.Constants;
using Microsoft.VisualStudio;
using SharedSrc.Commands;
using SharedSrc.Interfaces;
using Microsoft.VisualStudio.Settings;
using SourceControlDeepLinks.Helpers;
using SourceControlDeepLinks.Options;

namespace SourceControlDeepLinks
{
	[PackageRegistration( UseManagedResourcesOnly = true, AllowsBackgroundLoading = true )]
	[InstalledProductRegistration( Vsix.Name, Vsix.Description, Vsix.Version )]
	[ProvideMenuResource( "Menus.ctmenu", 1 )]
	[ProvideOptionPage( typeof( OptionsProvider.ExtensionOptionsProv ), OptionsPageCategoryName, OptionsPageName, 0, 0, true )]
	[ProvideProfile( typeof( OptionsProvider.ExtensionOptionsProv ), OptionsPageCategoryName, OptionsPageName, 0, 0, true )]
	[ProvideOptionPage( typeof( OptionsProvider1.Provider1OptionsProv ), OptionsPageCategoryName, Options1PageName, 0, 0, true, Sort = 1 )]
	[ProvideProfile( typeof( OptionsProvider1.Provider1OptionsProv ), OptionsPageCategoryName, Options1PageName, 0, 0, true )]
	[ProvideOptionPage( typeof( OptionsProvider2.Provider2OptionsProv ), OptionsPageCategoryName, Options2PageName, 0, 0, true, Sort = 2 )]
	[ProvideProfile( typeof( OptionsProvider2.Provider2OptionsProv ), OptionsPageCategoryName, Options2PageName, 0, 0, true )]
	[ProvideOptionPage( typeof( OptionsProvider3.Provider3OptionsProv ), OptionsPageCategoryName, Options3PageName, 0, 0, true, Sort = 3 )]
	[ProvideProfile( typeof( OptionsProvider3.Provider3OptionsProv ), OptionsPageCategoryName, Options3PageName, 0, 0, true )]
	[Guid( PackageGuids.SourceControlDeepLinksString )]
	[ProvideAutoLoad( VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad )]
	[ProvideAutoLoad( VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad )]
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

			// From AsyncPackageExtensions:
			// Calls Initialize( this )
			// Populate Command (Guid,id) as OleMenuCommand|DynamicItemMenuCommand
			// Add command to IMenuCommandService
			// Populate Package = this
			await this.RegisterCommandsAsync();
			this.RegisterToolWindows();

			//_extensionOutputPaneName = ExtensionOutputPane;
			//_state = new ExtensionOptions();

			// sets _state var
			await GetOptionsFromVSSettingsAsync();

			await LogStartupInformationAsync( ExtensionOutputPane, Vsix.Version );

			//ExtensionOptions.Saved += OptionsSaved;
		}

		public ExtensionOptions GetOptionsState() => _state;
#if false
		private void OptionsSaved(ExtensionOptions extensionOptions)
		{
			ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
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