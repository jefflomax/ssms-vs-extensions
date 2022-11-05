using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
//using Task = System.Threading.Tasks.Task;
using Task = System.Threading.Tasks.Task;
using SourceControlDeepLinks.Helpers;
using SharedSrc.Commands;
using static SourceControlDeepLinks.Resources.Constants;
using SharedSrc.Helpers;
using SharedSrc.Interfaces;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using SourceControlDeepLinks.Options;

namespace SourceControlDeepLinks.Commands
{
	[Command( PackageIds.BitbucketDeepLinkCommand )]
	internal sealed class BitbucketDeepLinkCommand
		: CommandShared<BitbucketDeepLinkCommand, SourceControlDeepLinksPackage, ExtensionOptions>,
			ICommandShared<SourceControlDeepLinksPackage>
	{
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var package = GetPackage;

			var allOptions = await package.GetLiveOptionsInstanceAsync();
			var generalOptions = allOptions.General;

			var nl = Environment.NewLine;
			var sbDiagnostics = new StringBuilder();

			// Do not emit anything to the Output Pane until the
			// ActiveDocument is found, it receiving focus could
			// confuse GetActiveDocumentViewAsync.
			// GetActiveDocumentViewAsync also is confused by
			// ASPX files

			var activeFilePath = await GetActiveFilePathAsync( sbDiagnostics );

			if( string.IsNullOrEmpty( activeFilePath ) )
			{
				await package.ToOutputPaneAsync( sbDiagnostics.ToString() );
				return;
			}

			var file = Path.GetFileNameWithoutExtension( activeFilePath );
			var extension = Path.GetExtension( activeFilePath );
			var workingDirectory = Path.GetDirectoryName( activeFilePath );

			var gitExecutable = generalOptions.GitExecutable;
			var bypassGit = generalOptions.BypassGit;

			var gitHelper = new GitHelper( gitExecutable, bypassGit );

			var repoRoot = await gitHelper.GetRepositoryRootAsync( workingDirectory );
			if( string.IsNullOrEmpty( repoRoot ) )
			{
				await package.ToOutputPaneAsync
				(
					$"Could not locate '.git' folder for {workingDirectory}{nl}" +
					"This is difficult enough when the file is in a repository..."
				);
				return;
			}

			var remoteOrigin = await gitHelper.GetRemoteOriginUrlAsync( workingDirectory );
			if( string.IsNullOrEmpty( remoteOrigin ) )
			{
				await package.ToOutputPaneAsync
				(
					$"No Origin Url found, check your \".git/config\" file{nl}" +
					$"for a [remote \"origin\"] url{nl}"
				);
				return;
			}

			var currentBranch = await gitHelper.GetCurrentBranchAsync( workingDirectory );

			var providerInfo = allOptions.GetMatchingProvider( remoteOrigin, sbDiagnostics );
			if( providerInfo == null )
			{
				await package.ToOutputPaneAsync
				( 
					$"No matching provider found, in Tools, Options, {OptionsPageCategoryName}{nl}" +
					$"check the expected provider is enabled, and check that{nl}" +
					$"it's 'Origin Match' is a string that matches the remote origin url:{nl}" +
					sbDiagnostics.ToString()
				);
				return;
			}

			var bookmarkedLines = e.InValue as string;
			var providerHelper = new ProviderFactory();
			var providerLinkInfo = providerHelper.GetDeepLink
			(
				providerInfo,
				remoteOrigin,
				repoRoot,
				activeFilePath,
				bookmarkedLines,
				currentBranch
			);

			if( providerLinkInfo == null )
			{
				await package.ToOutputPaneAsync
				(
					$"{repoRoot}{nl}" +
					$"{remoteOrigin}{nl}" +
					$"{activeFilePath}{nl}"
				);
				return;
			}

			var deepLink = providerLinkInfo.DeepLink;
			var pathInRepo = providerLinkInfo.PathInRepo;

			if( generalOptions.DiagnosticOutput )
			{
				await package.ToOutputPaneAsync
				(
					$"Repo Root: {repoRoot}{nl}Repo Path: {pathInRepo}{nl}" +
					$"Remote Org: {remoteOrigin}{nl}Source File: {activeFilePath}{nl}"
				);
			}

			if( generalOptions.Format )
			{
				// {0} url {1} path {2} file {3} extension
				deepLink = string.Format( generalOptions.FormatString, deepLink, pathInRepo, file, extension );
			}

			if( generalOptions.OutputToClipboard )
			{
				ClipboardHelper.SetData( deepLink );
			}

			if( generalOptions.OutputToPane )
			{
				await package.ToOutputPaneAsync( deepLink );
			}
		}

		private static async Task<string> GetSolutionFolderAsync()
		{
			var solutionFile = string.Empty;
			var solutionDirectory = string.Empty;
			var solution = await VS.Solutions.GetCurrentSolutionAsync();
			if( solution != null )
			{
				solutionFile = solution.FullPath;
				solutionDirectory = Path.GetDirectoryName( solutionFile );
			}
			return solutionDirectory;
		}

		private async Task<string> GetActiveFilePathAsync( StringBuilder sbDiagnostics )
		{
			string activeFilePath = null;

			// Tweakster ResetZoomLevel does this, presumably
			// because GetActiveDocumentViewAsync is disappointing
			DTE2 dte = await DteHelper.GetDTEAsync();
			if( dte == null || dte.ActiveDocument == null )
			{
				return null;
			}

			// It seems that ASPX files generally are not working
			// Further, GetActiveDocumentView may return the Output Pane
			// https://github.com/VsixCommunity/Community.VisualStudio.Toolkit/issues/317
			// https://stackoverflow.com/questions/2868127/get-the-selected-text-of-the-editor-window-visual-studio-extension
			// https://stackoverflow.com/questions/31759396/vsix-adding-a-menu-item-to-the-visual-studio-editor-context-menu/31769170#31769170

			var (hasTextView, docView) = await GetActiveDocumentViewAsync();
			if( !hasTextView )
			{
				return null;
			}

			// Try to detect non Editor window
			if( docView.WindowFrame.Editor == Guid.Empty ||
				docView.Document == null ||
				docView.FilePath == null
			)
			{
				sbDiagnostics
					.Append( "GetActiveDocumentViewAsync reports null, it can be confused " )
					.AppendLine( "by OutputWindowPane, ASPX, ..." );

				var editorPath = await GetActiveEditorAsync( sbDiagnostics );
				if( !string.IsNullOrEmpty( editorPath ) )
				{
					activeFilePath = editorPath;
				}
				else
				{
					var (filePath, fileType) = await DteHelper.GetDocumentInfoAsync( dte );
					if( !string.IsNullOrEmpty( filePath ) )
					{
						activeFilePath = filePath;
						sbDiagnostics.Append( $"DTE reports {filePath} {fileType}" );
					}
					else
					{
						sbDiagnostics.Append( $"DTE finds no active document" );
					}
				}
			}
			else
			{
				activeFilePath = docView.FilePath;
			}

			return activeFilePath;
		}

		private async Task<string> GetActiveEditorAsync( StringBuilder sbDiagnostics )
		{
			// https://michaelscodingspot.com/visual-studio-2017-extension-development-tutorial-part-3-add-context-menu-get-selected-code/
			// This did not find a codeWindow for ASPX
			// Or limit to window that really has focus
			string activeEditorFilePath = null;
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var textManager = await VS.GetServiceAsync<SVsTextManager, IVsTextManager2>();

			IVsTextView view;

			int result = textManager.GetActiveView2( 1, null, (uint)_VIEWFRAMETYPE.vftAny, out view );
			if( result == VSConstants.S_OK )
			{
				// https://stackoverflow.com/questions/27791773/ivstextmanagergetactiveviewtrue-null-returns-non-focused-view
				IVsTextView viewWithBuffer;
				IVsTextLines vsTextLines;
				if(view.GetBuffer(out vsTextLines) == VSConstants.S_OK &&
					textManager.GetActiveView2(1, vsTextLines, (uint)_VIEWFRAMETYPE.vftAny, out viewWithBuffer) == VSConstants.S_OK)
				{
					sbDiagnostics.AppendLine("GetActiveView2 succeeded");
					var documentView = await view.ToDocumentViewAsync();
					if (!string.IsNullOrEmpty(documentView?.FilePath))
					{
						activeEditorFilePath = documentView.FilePath;
						sbDiagnostics.AppendLine($"Found DocumentView {activeEditorFilePath}");
					}
					else
					{
						var wpfDocumentView = await view.ToIWpfTextViewAsync();
						var documentViewWpf = await wpfDocumentView.ToDocumentViewAsync();
						if (!string.IsNullOrEmpty(documentViewWpf?.FilePath))
						{
							activeEditorFilePath = documentView.FilePath;
							sbDiagnostics.AppendLine($"Found wpfDocumentView {activeEditorFilePath}");
						}
					}
				}
			}
			else
			{
				sbDiagnostics.AppendLine( "IVsTextManager2 GetActiveView2 failed" );
			}
			if( activeEditorFilePath == null )
			{
				sbDiagnostics.AppendLine( "No Active Editor Found" );
			}

			return activeEditorFilePath;
		}
	}
}
