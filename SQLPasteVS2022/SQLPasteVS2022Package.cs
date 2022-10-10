using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using static SQLPaste.Resources.Constants;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using SharedSrc.Commands;
using SharedSrc.Interfaces;

namespace SQLPaste
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[ProvideOptionPage( typeof( OptionsProvider.ExtensionOptionsProv ), OptionsPageCategoryName, OptionsPageName, 0, 0, true )]
	[ProvideProfile( typeof( OptionsProvider.ExtensionOptionsProv ), OptionsPageCategoryName, OptionsPageName, 0, 0, true )]
	[Guid(PackageGuids.SQLPasteString)]
	public sealed class SQLPastePackage 
		: BasePackage<ExtensionOptions>, IBasePackage<ExtensionOptions>
	{
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			//await this.RegisterCommandsAsync();
			// Setup output pane first so we can log there
			var outputWindowPane = await SetupOutputPaneAsync( PasteSpecial );

			// From AsyncPackageExtensions:
			// Calls Initialize( this )
			// Populate Command (Guid,id) as OleMenuCommand|DynamicItemMenuCommand
			// Add command to IMenuCommandService
			// Populate Package = this
			await this.RegisterCommandsAsync();
			this.RegisterToolWindows();

			_state = new ExtensionOptions();
			_state.CSVPasteDirection = true;
			_state.ValuesPasteDirection = false;

			await GetOptionsFromVSSettingsAsync();

			await LogStartupInformationAsync( OptionsPageCategoryName, Vsix.Version );

			ExtensionOptions.Saved += OptionsSaved;

		}

		public ExtensionOptions GetOptionsState() => _state;

		private void OptionsSaved( ExtensionOptions pasteOptions )
		{
			_ = ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
			{
				await ToOutputPaneAsync( $"PasteOptionsSaved {pasteOptions}" );
			} );

			//_toolWindowState.FromOptions( pasteOptions );
		}

		public override async Task<ExtensionOptions> GetLiveSettingsInstanceAsync()
		{
			return await ExtensionOptions.GetLiveInstanceAsync();
		}
	}
}