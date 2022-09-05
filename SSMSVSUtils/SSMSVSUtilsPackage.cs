using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using static SSMSVSUtils.Resources.Constants;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using SharedSrc.Commands;
using SharedSrc.Interfaces;

namespace SSMSVSUtils
{
	[PackageRegistration( UseManagedResourcesOnly = true, AllowsBackgroundLoading = true )]
	[InstalledProductRegistration( Vsix.Name, Vsix.Description, Vsix.Version )]
	[ProvideMenuResource( "Menus.ctmenu", 1 )]
	//[ProvideToolWindow( typeof( ExtensionOptionsToolWindow.Pane ), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer )]
	[ProvideOptionPage(typeof(OptionsProvider.ExtensionOptionsProv), OptionsPageCategoryName, OptionsPageName, 0, 0, true)]
	[ProvideProfile(typeof(OptionsProvider.ExtensionOptionsProv), OptionsPageCategoryName, OptionsPageName, 0, 0, true)]
	[Guid( PackageGuids.SSMSVSUtilsString )]
	[ProvideAutoLoad( VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad )]
	[ProvideAutoLoad( VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad )]
	public sealed class SSMSVSUtilsPackage
		: BasePackage<ExtensionOptions>, IBasePackage<ExtensionOptions>
	{
		protected override async Task InitializeAsync
		(
			CancellationToken cancellationToken,
			IProgress<ServiceProgressData> progress
		)
		{
			// Setup output pane first so we can log there
			var outputWindowPane = await SetupOutputPaneAsync( OptionsPageName );

			// From AsyncPackageExtensions:
			// Calls Initialize( this )
			// Populate Command (Guid,id) as OleMenuCommand|DynamicItemMenuCommand
			// Add command to IMenuCommandService
			// Populate Package = this
			await this.RegisterCommandsAsync();
			this.RegisterToolWindows();

			_state = new ExtensionOptions();

			await GetOptionsFromVSSettingsAsync();

			await LogStartupInformationAsync( OptionsPageName, Vsix.Version );

			ExtensionOptions.Saved += OptionsSaved;
		}
		public ExtensionOptions GetOptionsState() => _state;

		private void OptionsSaved(ExtensionOptions pasteOptions)
		{
			ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
			{
				await ToOutputPaneAsync( $"{OptionsPageCategoryName} {pasteOptions}" );
			} );

			_state = pasteOptions;
		}
		public override async Task<ExtensionOptions> GetLiveSettingsInstanceAsync()
		{
			return await ExtensionOptions.GetLiveInstanceAsync();
		}
	}
}