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

namespace SourceControlDeepLinks
{
	[PackageRegistration( UseManagedResourcesOnly = true, AllowsBackgroundLoading = true )]
	[InstalledProductRegistration( Vsix.Name, Vsix.Description, Vsix.Version )]
	[ProvideMenuResource( "Menus.ctmenu", 1 )]
	[ProvideOptionPage(typeof(OptionsProvider.ExtensionOptionsProv), OptionsPageCategoryName, OptionsPageName, 0, 0, true)]
	[ProvideProfile(typeof(OptionsProvider.ExtensionOptionsProv), OptionsPageCategoryName, OptionsPageName, 0, 0, true)]
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

			_extensionOutputPaneName = ExtensionOutputPane;
			_state = new ExtensionOptions();

			await GetOptionsFromVSSettingsAsync();

			await LogStartupInformationAsync( ExtensionOutputPane, Vsix.Version );

			ExtensionOptions.Saved += OptionsSaved;
		}

		public ExtensionOptions GetOptionsState() => _state;

		private void OptionsSaved(ExtensionOptions extensionOptions)
		{
			ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
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