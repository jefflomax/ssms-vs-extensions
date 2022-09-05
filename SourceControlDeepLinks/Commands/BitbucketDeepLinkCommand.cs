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
using Task = System.Threading.Tasks.Task;
using SourceControlDeepLinks.Helpers;
using SharedSrc.Commands;
using static SourceControlDeepLinks.Resources.Constants;
using SharedSrc.Helpers;
using SharedSrc.Interfaces;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;

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
			var state = package.GetOptionsState();
			var nl = Environment.NewLine;
			string activeFilePath = null;
			var sbDiagnostics = new StringBuilder();

			// Do not emit anything to the Output Pane until the
			// ActiveDocument is found, it receiving focus could
			// confuse GetActiveDocumentViewAsync.
			// GetActiveDocumentViewAsync also is confused by
			// ASPX files

			// Tweakster ResetZoomLevel does this, presumably
			// because GetActiveDocumentViewAsync is disappointing
			DTE2 dte = await DteHelper.GetDTEAsync();
			if( dte == null || dte.ActiveDocument == null )
			{
				return;
			}

			// It seems that ASPX files generally are not working
			// Further, GetActiveDocumentView may return the Output Pane
			//https://github.com/VsixCommunity/Community.VisualStudio.Toolkit/issues/317
			//https://stackoverflow.com/questions/2868127/get-the-selected-text-of-the-editor-window-visual-studio-extension

			// https://stackoverflow.com/questions/31759396/vsix-adding-a-menu-item-to-the-visual-studio-editor-context-menu/31769170#31769170

			var (hasTextView, docView) = await GetActiveDocumentViewAsync();
			if (!hasTextView)
			{
				return;
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

#if false
			string solutionFile = string.Empty;
			string solutionDirectory = string.Empty;
			var solution = await VS.Solutions.GetCurrentSolutionAsync();
			if( solution != null)
			{
				solutionFile = solution.FullPath;
				solutionDirectory = Path.GetDirectoryName( solutionFile );
			}
#endif

			if( string.IsNullOrEmpty(activeFilePath) )
			{
				await package.ToOutputPaneAsync( sbDiagnostics.ToString() );
				return;
			}

#if false
			// TODO: remove
			if( state.DiagnosticOutput && sbDiagnostics.Length > 0 )
			{
				await package.ToOutputPaneAsync( sbDiagnostics.ToString() );
			}
#endif

			var file = Path.GetFileNameWithoutExtension( activeFilePath );
			var extension = Path.GetExtension( activeFilePath );
			var workingDirectory = Path.GetDirectoryName( activeFilePath );

			var git = state.GitExecutable;
			var gitHelper = new GitHelper( git, state.BypassGit );

			var repoRoot = await gitHelper.GetRepositoryRootAsync( workingDirectory );
			var remoteOrigin = await gitHelper.GetRemoteOriginUrlAsync( workingDirectory );

			var appSettingsHelper = new AppSettingsHelper(Package);

			var bookmarkedLines = e.InValue as string;
			var (bbDeepLink, pathInRepo) = BitbucketHelper.GetBitbucketDeepLink
			(
				remoteOrigin,
				repoRoot,
				activeFilePath,
				bookmarkedLines,
				appSettingsHelper
			);

			if (state.DiagnosticOutput)
			{
				await package.ToOutputPaneAsync($"Repo Root: {repoRoot}{nl}Repo Path: {pathInRepo}{nl}Remote Org: {remoteOrigin}{nl}Source File: {activeFilePath}{nl}");
			}

			if ( state.Format)
			{
				// {0} url {1} path {2} file {3} extension
				bbDeepLink = string.Format(state.FormatString, bbDeepLink, pathInRepo, file, extension);
			}

			if( state.OutputToClipboard )
			{
				ClipboardHelper.SetData( bbDeepLink );
			}

			if( state.OutputToPane )
			{
				await package.ToOutputPaneAsync( bbDeepLink );
			}
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
