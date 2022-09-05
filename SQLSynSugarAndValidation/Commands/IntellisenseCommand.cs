using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using System;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Threading;
using SharedSrc.Commands;
using SharedSrc.Extensions;
using SharedSrc.Helpers;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Threading.Tasks;
using SharedSrc.Interfaces;

namespace SQLSynSugarAndValidation
{
	[Command( PackageIds.IntellisenseCommand )]
	internal sealed class IntellisenseCommand
		: CommandShared<IntellisenseCommand, SQLSynSugarAndValidationPackage, ExtensionOptions>,
			ICommandShared<SQLSynSugarAndValidationPackage>
	{
#pragma warning disable 414
		private static readonly string QueryIntellisense = "Query.IntelliSenseEnabled";
#pragma warning restore 414
		//private static readonly string RefreshCache = "Edit.RefreshLocalCache";
		//Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell\SqlEditor
		//EnableIntellisense
		//EnableCodeSense
		//TextCasing
		//WordWrapGlyphs
		//Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell\SQLSynSugarAndValidation.PasteOptions
		//Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell_Config\Diff\BlockedCommands\All
		//Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell_Config\KeyBindingTables
		//Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell_Config\Menus
		//Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell_Config\Peek\BlockedCommands

		protected override Task ExecuteAsync( OleMenuCmdEventArgs e )
		{
			return Task.CompletedTask;
		}

#if false
			var package = GetPackage;
			
			{
				await GetSettingFromReaderAsync( package );
#if false
				// Regex update worked, but vS didn't know about it
				var intellisenseEnabled = RegistryHelper.GetStringBooleanValue( RegistryHelper.IntellisenseEnabled );
				if (intellisenseEnabled == SSMSIntellisense.NotFound)
				{
					await package
						.ToOutputPaneAsync( $"Could not process registry" );
					return;
				}

				var toggledIntellisenseState = RegistryHelper.Toggle( intellisenseEnabled );
				RegistryHelper.SetStringValueFromBool( RegistryHelper.IntellisenseEnabled, toggledIntellisenseState );
#endif

#if false
				if (queryExecuteCommand == null)
				{
					await package
						.ToOutputPaneAsync( $"Failed to find {QueryIntellisense}" );
					return;
				}

				var state = package.GetPasteOptionsState();

				var parm = state.CommandParameter ?? "0";
				await package
					.ToOutputPaneAsync( $"Execute {QueryIntellisense} parm ({parm}) Reg: {intellisenseEnabled}" );
#endif
				//System.UnauthorizedAccessException: Cannot write to the registry key

				//await queryExecuteCommand.ExecuteAsync(parm);
			}
#endif


		private async Task GetSettingFromReaderAsync( SQLSynSugarAndValidationPackage package )
		{
			var ct = System.Threading.CancellationToken.None;
			await this.Package.JoinableTaskFactory.SwitchToMainThreadAsync( ct );

			//Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell\SqlEditor\EnableIntellisense

			var vsSettingsManager = await GetSettingsManagerAsync();
			IVsWritableSettingsStore store;
			var userSettings = (uint)SettingsScope.UserSettings;

			if( vsSettingsManager.GetWritableSettingsStore( userSettings, out store ) == VSConstants.S_OK )
			{
				int collectionExists = 0;
				var collectionName = RegistryHelper.PrivateSettingsCollectionName;
				var sqlEditorCollection = RegistryHelper.SqlEditorCollectionName;
				if( store.CollectionExists( collectionName, out collectionExists ) == VSConstants.S_OK )
				{
					var subCollection = $@"{collectionName}\{RegistryHelper.RadLangSvcSqlLang}";
					string regValue;
					if( store.GetString( subCollection, RegistryHelper.IntellisenseEnabled, out regValue ) == VSConstants.S_OK )
					{
						await package.ToOutputPaneAsync( $"Found {regValue} at {subCollection}" );

						var ssmsIntellisense = RegistryHelper.ProcessRegistryString( regValue );
						ssmsIntellisense = RegistryHelper.Toggle( ssmsIntellisense );
						var newValue = RegistryHelper.Serialize( ssmsIntellisense );

						if( store.SetString
							(
								subCollection,
								RegistryHelper.IntellisenseEnabled,
								newValue
							) == VSConstants.S_OK )
						{
							await package.ToOutputPaneAsync
							(
								$"Set '{newValue}' at {subCollection}"
							);
							string regStrValue;
							if( store.CollectionExists( sqlEditorCollection, out collectionExists ) == VSConstants.S_OK )
							{
								if( store.GetString
									(
										sqlEditorCollection,
										RegistryHelper.IntellisenseEnabled,
										out regStrValue
									) == VSConstants.S_OK )
								{
									await package.ToOutputPaneAsync
									(
										$@"Found {regStrValue} at {sqlEditorCollection}\{RegistryHelper.IntellisenseEnabled}"
									);
									var b = RegistryHelper.ToBoolean( ssmsIntellisense );
									var bstr = b.ToString();
									if( store.SetString( sqlEditorCollection,
										RegistryHelper.IntellisenseEnabled,
										bstr ) == VSConstants.S_OK )
									{
										await package.ToOutputPaneAsync
										(
											$@"Wrote '{bstr}' to {sqlEditorCollection}\{RegistryHelper.IntellisenseEnabled}"
										);
									}
									else
									{
										await package.ToOutputPaneAsync
										(
											$@"Failed set {sqlEditorCollection}\{RegistryHelper.IntellisenseEnabled}"
										);
									}
								}
							}
							else
							{
								await package.ToOutputPaneAsync
								(
									$@"Failed to find collection {sqlEditorCollection}"
								);
							}
						}
					}
				}
			}
		}

		private async Task<IVsSettingsManager> GetSettingsManagerAsync()
		{
			var ct = System.Threading.CancellationToken.None;
			await this.Package.JoinableTaskFactory.SwitchToMainThreadAsync( ct );

#pragma warning disable CVST001
			var settingsManager = await VS.Services.GetSettingsManagerAsync()
				.ConfigureAwait( false ) as IVsSettingsManager;
#pragma warning restore CVST001
			return settingsManager;
		}

	}
}
