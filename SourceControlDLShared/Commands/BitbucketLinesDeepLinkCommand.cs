using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
//using Task = System.Threading.Tasks.Task;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using SharedSrc.Commands;
using SharedSrc.Helpers;
using System.ComponentModel.Design;
using SharedSrc.Interfaces;

namespace SourceControlDeepLinks.Commands
{
	[Command( PackageIds.BitbucketLinesDeepLinkCommand )]
	internal sealed class BitbucketLinesDeepLinkCommand
		: CommandShared<BitbucketLinesDeepLinkCommand, SourceControlDeepLinksPackage, ExtensionOptions>,
			ICommandShared<SourceControlDeepLinksPackage>
	{
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var package = GetPackage;

			var (hasTextView, docView) = await GetActiveDocumentViewAsync();
			if (!hasTextView)
			{
				return;
			}

			var dte = await DteHelper.GetDTEAsync();

			var insertLine = false;
			//var textDocument = dte.ActiveDocument.Object( "TextDocument" ) as TextDocument;
			//var selection = textDocument.Selection;
			//selection.StartOfDocument();
			var textDocument = await dte.ActiveDocument.GetTextDocumentAsync();
			var selection = await textDocument.GetSelectionAsync();

			await selection.StartOfDocumentAsync();
			bool nb;
			int cl;
			var lines = new HashSet<int>();

			do
			{
				nb = await selection.NextBookmarkAsync();
				cl = await selection.CurrentLineAsync();
				insertLine = !lines.Contains( cl );
				if( nb && insertLine )
				{
					lines.Add( cl );
					//selection.Cancel();
				}
			} while (nb && insertLine);
			var bookmarkLines = lines.ToArray();
			var bookmarkLinesStr = string.Join( ",", bookmarkLines );

			// Get our sibling command
			var commandId = new CommandID(PackageGuids.SourceControlDeepLinks, PackageIds.BitbucketDeepLinkCommand);
			await VS.Commands.ExecuteAsync(commandId, bookmarkLinesStr);
		}
	}
}
