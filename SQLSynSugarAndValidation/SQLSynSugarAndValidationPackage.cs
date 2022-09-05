using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using static SQLSynSugarAndValidation.Resources.Constants;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using SharedSrc.Commands;
using SharedSrc.Interfaces;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using SQLSynSugarAndValidation.Helpers;
using SQLSynSugarAndValidation.ToolWindows;
using System.Text;

namespace SQLSynSugarAndValidation
{

	[PackageRegistration( UseManagedResourcesOnly = true, AllowsBackgroundLoading = true )]
	[InstalledProductRegistration( Vsix.Name, Vsix.Description, Vsix.Version )]
	[ProvideMenuResource( "Menus.ctmenu", 1 )]
	[ProvideToolWindow( typeof( ExtensionOptionsToolWindow.Pane ), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer )]
	[ProvideOptionPage(typeof(OptionsProvider.ExtensionOptionsProv), OptionsPageCategoryName, OptionsPageName, 0, 0, true)]
	[ProvideProfile(typeof(OptionsProvider.ExtensionOptionsProv), OptionsPageCategoryName, OptionsPageName, 0, 0, true)]
	[Guid( PackageGuids.SQLSynSugarAndValidationString )]
	[ProvideAutoLoad( VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad )]
	[ProvideAutoLoad( VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad )]
	public sealed class SQLSynSugarAndValidationPackage
		: BasePackage<ExtensionOptions>, IBasePackage<ExtensionOptions>
	{
		private DBSchema.schema.Server _server = null ;
		private const string _schemaFolder = @"C:\programs\vs2019\antlrSql\antlr\generated";

		private ExtensionOptionsToolWindowControl _toolWindowCtrl;

		protected override async Task InitializeAsync
		(
			CancellationToken cancellationToken,
			IProgress<ServiceProgressData> progress
		)
		{
			// Keep System.Text.Json & System.Runtime.CompilerServices.Unsafe
			// from crashing SSMS
			// TODO: Check Visual Studio
			SetupAssemblyRedirects();

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
			_state.SchemaFolder = _schemaFolder;
			_state.ShowParsed = true;  // TODO: Testing

			await GetOptionsFromVSSettingsAsync();

			await LogStartupInformationAsync( OptionsPageName, Vsix.Version );

			ExtensionOptions.Saved += OptionsSaved;

			var serverUtil = new ServersUtil();
			_server = await serverUtil.DeserializeAsync
			(
				".",
				_schemaFolder
			);
#if false
			await JoinableTaskFactory.SwitchToMainThreadAsync( cancellationToken );

			//var x = VS.Settings.OpenAsync
			//var x = VS.Events.ShellEvents.
			//var x = VS.Events.SolutionEvents.OnAfterOpenFolder
			var settingsManager = await VS.Services.GetSettingsManagerAsync()
				.ConfigureAwait( false ) as IVsSettingsManager;
			//.ConfigureAwait( cancellationToken );
			IVsWritableSettingsStore store;
			var userSettings = (uint)SettingsScope.UserSettings;
			if (settingsManager.GetWritableSettingsStore( userSettings, out store ) == VSConstants.S_OK)
			{
				//https://docs.microsoft.com/en-us/visualstudio/ide/managing-application-settings-dotnet?view=vs-2022
				//https://www.csharpcodi.com/csharp-examples/Microsoft.VisualStudio.Settings.SettingsStore.GetString(string,%20string,%20string)/
				//https://www.codeproject.com/articles/599504/visual-studio-visualizer-part-3-collection-visuali

				int collectionExists = 0;
				var collectionName = RegistryHelper.CollectionName;
				if( store.CollectionExists( collectionName, out collectionExists ) == VSConstants.S_OK)
				{
					uint subcollectionCount;
					await ToOutputPaneAsync( outputWindowPane, $"Found {collectionName}" );
					if (collectionExists == 1)
					{
						// proves the collection delimiter is \
						await EnumerateStoreAsync( store, outputWindowPane, collectionName, 0 );
					}
				}
			}
#endif
		}

		public void SetToolWindowControl
		(
			ExtensionOptionsToolWindowControl toolWindowCtrl
		)
		{
			_toolWindowCtrl = toolWindowCtrl;
		}

		public void SetToolWindowText
		(
			string lispExpressionTree,
			string rewritten,
			string original
		)
		{
			if( _toolWindowCtrl != null )
			{
				var prevContext = ToolWindowState;

				// TODO: Get List Font Size from options
				var newContext = new ExtensionOptionsState
				(
					prevContext,
					RtfUtil.FormatParseTree( lispExpressionTree, 18 ),
					rewritten,
					original
				);

				// Brute force resetting data context forces update
				// should be smarter
				_toolWindowCtrl.DataContext = newContext;
			}
		}

		public void SetToolWindowValidations
		(
			antlr.Validation.Validations validations
		)
		{
			var prevContext = ToolWindowState;
			if( prevContext != null )
			{
				var tree = prevContext.Items;
				ValidationDisplayUtils.ValidationTree( validations, tree );
			}
		}

		private ExtensionOptionsState ToolWindowState =>
			_toolWindowCtrl?.DataContext as ExtensionOptionsState;

		/// <summary>
		/// Get cached DB schema
		/// </summary>
		/// <returns></returns>
		public DBSchema.schema.Server GetServer()
		{
			return _server;
		}

		private static void SetupAssemblyRedirects()
		{
#if false
			var jsonVersion = new Version( 6, 0 );
			RedirectAssembly
			(
				"System.Text.Json",
				jsonVersion,
				publicKeyToken: null
			);
			var unsafeVersion = new Version( 6, 0 );
			RedirectAssembly
			(
				"System.Runtime.CompilerServices.Unsafe",
				unsafeVersion,
				"b03f5f7f11d50a3a"
			);
#endif
		}

		private async Task EnumerateStoreAsync
		(
			IVsWritableSettingsStore store,
			OutputWindowPane pane,
			string collectionName,
			int level
		)
		{
			uint subcollectionCount;

			if (store.GetSubCollectionCount( collectionName, out subcollectionCount ) == VSConstants.S_OK )
			{
				string subCollectionName;
				if( subcollectionCount != 0)
				{
					for (uint i = 0; i < subcollectionCount; i++)
					{
						if (store.GetSubCollectionName( collectionName, i, out subCollectionName ) == VSConstants.S_OK)
						{
							await EnumerateStoreAsync( store, pane, $@"{collectionName}\{subCollectionName}", level + 1 );
						}
					}
				}
				else
				{
					await ToOutputPaneAsync( pane, $"End1 {collectionName} {level}" );
				}
			}
			else
			{
				await ToOutputPaneAsync( pane, $"End2 {collectionName} {level}" );
			}
		}
		public ExtensionOptions GetOptionsState() => _state;

		private void OptionsSaved(ExtensionOptions extensionOptions)
		{
			ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
			{
				await ToOutputPaneAsync( $"Saved {extensionOptions}" );
			} );

			_state = extensionOptions;
		}

		public override async Task<ExtensionOptions> GetLiveSettingsInstanceAsync()
		{
			return await ExtensionOptions.GetLiveInstanceAsync();
		}

		// https://blog.slaks.net/2013-12-25/redirecting-assembly-loads-at-runtime/
		/// <summary>
		/// devenv.exe.config and Ssms.exe.config have binding redirects that block
		/// the versions we need, intercept Fusion assembly request and allow ours
		/// </summary>
		/// <param name="shortName"></param>
		/// <param name="targetVersion"></param>
		/// <param name="publicKeyToken"></param>
		public static void RedirectAssembly
		(
			string shortName,
			Version targetVersion,
			string publicKeyToken
		)
		{
			ResolveEventHandler handler = null;

			handler = ( sender, args ) => {
				// Use latest strong name & version when trying to load SDK assemblies
				var requestedAssembly = new AssemblyName( args.Name );
				
				if( requestedAssembly.Name != shortName )
					return null;
				
				Debug.WriteLine( "Redirecting assembly load of " + 
					args.Name + ",\tloaded by " + 
					(args.RequestingAssembly == null 
						? "(unknown)"
						: args.RequestingAssembly.FullName)
					);

				requestedAssembly.Version = targetVersion;
				if( publicKeyToken != null )
				{
					var fakeAssembly = new AssemblyName( "x, PublicKeyToken=" + publicKeyToken );
					var token = fakeAssembly.GetPublicKeyToken();
					requestedAssembly.SetPublicKeyToken( token );
				}
				requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

				AppDomain.CurrentDomain.AssemblyResolve -= handler;

				return Assembly.Load( requestedAssembly );
			};
			AppDomain.CurrentDomain.AssemblyResolve += handler;
		}

	}
}