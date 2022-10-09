using System;
using System.Collections.Generic;
using System.Text;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using SharedSrc.Helpers;
using SQLPaste.Processor;
using Task = System.Threading.Tasks.Task;
using SharedSrc.Commands;
using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel.Design;
using SharedSrc.Interfaces;

namespace SQLPaste.Commands
{
	[Command( PackageIds.CsvPasteCommand )]
	internal sealed class CsvPasteCommand 
		: CommandShared<CsvPasteCommand, SQLPastePackage, ExtensionOptions>,
			ICommandShared<SQLPastePackage>
	{
		// <Parent guid="VSMainMenu" id="View.DevWindowsGroup.OtherWindows.Group1"/>7
		// https://github.com/VsixCommunity/Community.VisualStudio.Toolkit

		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var pasteOptionsState = GetOptions();
			var stateFromPackageDescription = pasteOptionsState.ToString();

			var (hasTextView, docView) = await GetActiveDocumentViewAsync();
			if (!hasTextView)
			{
				return;
			}

			//var clipboardTextFormats = ClipboardHelper.GetDataFormats();
			//if( clipboardTextFormats.IndexOf("System.String")!= -1)
			//{
			//	object o = ClipboardHelper.GetData();
			//}

			if( ClipboardHelper.GetText( out var clipboardText ) )
			{
				var direction = (pasteOptionsState.CSVPasteDirection)
					? Direction.Horizontal
					: Direction.Vertical;
				var alwaysQuoteIntegers = pasteOptionsState.AlwaysQuoteIntegers;

				// Analyze clipboard text into contiguous lines
				// of compatible types.
				var stringProcessor = new StringProcessor();
				var (lineTypes, lines, maxColumn) = stringProcessor
					.Process( clipboardText, direction, DelimiterType.Csv, alwaysQuoteIntegers );

				await OutputToExtensionWindowPaneAsync( stringProcessor.ToString() );

				clipboardText = stringProcessor.GetFormattedText
				(
					clipboardText,
					direction,
					DelimiterType.Csv,
					alwaysQuoteIntegers
				);
			}
			else
			{
				await VS.StatusBar.ShowMessageAsync( "No text in clipboard" );
				return;
			}

			await VS.StatusBar.ShowMessageAsync( stateFromPackageDescription );

			SnapshotPoint position = docView
				.TextView
				.Caret
				.Position
				.BufferPosition;

			// Maybe we could call CommandID Edit_Paste
			// but that would overwrite the clipboard
			docView.TextBuffer?.Insert( position, clipboardText );
		}
	}
}
