using System;
using System.Collections.Generic;
using System.Text;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Threading.Tasks;
using SharedSrc.Commands;
using SharedSrc.Interfaces;

namespace SSMSVSUtils.Commands
{
	/// <summary>
	/// Hard Line Down - if on the last line, combine move to end and newline
	/// Currently on SHIFT-ALT-PAGEDOWN, wishing it could be shift downarrow
	/// </summary>
	[Command( PackageIds.HardLineDownCommand )]
	internal sealed class HardLineDownCommand
		: CommandShared<HardLineDownCommand, SSMSVSUtilsPackage, ExtensionOptions>,
			ICommandShared<SSMSVSUtilsPackage>
	{
		// bound to SHIFT ALT PAGEDOWN
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var (hasTextView, docView) = await GetActiveDocumentViewAsync();
			if (!hasTextView)
			{
				return;
			}

			var activeTextView = await GetActiveNativeTextViewAsync();
			activeTextView.GetSelection
			(
				out int piAnchorLine,
				out int piAnchorCol,
				out int piEndLine,
				out int piEndCol
			);

			var inVirtualSpace = docView.TextView.Caret.InVirtualSpace;

			var options = GetOptions();
			if( options.DiagnosticOutput )
			{
				await VS.StatusBar.ShowMessageAsync
				(
					$"Hard Line Down {piAnchorLine} {piAnchorCol} {piEndLine} {piEndCol} {inVirtualSpace}"
				);
			}

			ITextSnapshotLine line = docView
				.TextView
				.Caret
				.Position
				.BufferPosition
				.GetContainingLine();
			var lineBreakLength = line.LineBreakLength;
			if( lineBreakLength == 0 )
			{
				await KnownCommands.Edit_LineEnd.ExecuteAsync();
				await KnownCommands.Edit_BreakLine.ExecuteAsync();
				// should advance column ?
			}
		}

		// was in Documents.cs
		/// <summary>Gets the native text view from the currently active document.</summary>
		private async Task<IVsTextView> GetActiveNativeTextViewAsync()
		{
			// https://michaelscodingspot.com/visual-studio-2017-extension-development-tutorial-part-3-add-context-menu-get-selected-code/
			// shows using textManager.GetActiveView2
			IVsTextManager textManager = await VS.GetRequiredServiceAsync<SVsTextManager, IVsTextManager>();
			textManager.GetActiveView( 1, null, out IVsTextView activeView );

			return activeView;
		}
	}
}
