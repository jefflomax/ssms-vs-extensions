using System;
using System.Collections.Generic;
using System.Text;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using SharedSrc.Commands;
using SharedSrc.Helpers;
using SharedSrc.Interfaces;
using SQLPaste.Processor;
using Task = System.Threading.Tasks.Task;

namespace SQLPaste.Commands
{
	[Command( PackageIds.ValuesPasteCommand )]
	internal sealed class ValuesPasteCommand
		: CommandShared<ValuesPasteCommand, SQLPastePackage, ExtensionOptions>,
			ICommandShared<SQLPastePackage>
	{
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var (hasTextView, docView) = await GetActiveDocumentViewAsync();
			if (!hasTextView)
			{
				return;
			}

			if( ClipboardHelper.GetText( out var clipboardText ) )
			{
				// Analyze clipboard text into contiguous lines
				// of compatible types.
				var pasteOptionsState = GetOptions();

				var direction = (pasteOptionsState.ValuesPasteDirection)
					? Direction.Horizontal
					: Direction.Vertical;

				var stringProcessor = new StringProcessor();
				var (lineTypes, lines, maxColumn) = stringProcessor
					.Process
					(
						clipboardText,
						direction,
						DelimiterType.Values,
						pasteOptionsState.AlwaysQuoteIntegers
					);

				clipboardText = stringProcessor.GetFormattedText
				(
					clipboardText,
					direction,
					DelimiterType.Values,
					pasteOptionsState.AlwaysQuoteIntegers
				);
			}
			else
			{
				await VS.StatusBar.ShowMessageAsync( "No text in clipboard" );
				return;
			}

			SnapshotPoint position = docView
				.TextView
				.Caret
				.Position
				.BufferPosition;
			docView.TextBuffer?.Insert( position, clipboardText );
		}
	}
}
