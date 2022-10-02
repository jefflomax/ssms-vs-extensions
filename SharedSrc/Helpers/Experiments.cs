using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
//using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;

namespace SharedSrc.Helpers
{
	public class Experiments
	{
		public static async Task<string> ExperimentWithCommandNamesAsync
		(
			ToolkitPackage package,
			Services services
		)
		{
			var cmdNameMapping = await services.GetCommandNameMappingAsync();
			string[] strs = new string[ 1 ];

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			var sb = new StringBuilder();

			if( cmdNameMapping.EnumNames
				(
					VSCMDNAMEOPTS.CNO_GETENU,
					out IEnumString ies
				) == VSConstants.S_OK )
			{
				while( ies.Next( 1, strs, out uint fetched ) == VSConstants.S_OK )
				{
					if( fetched == 1 )
					{
						sb.AppendLine( strs[ 0 ] );
					}
				}
			}
			return sb.ToString();
		}

		public static bool SearchText
		(
			DocumentView docView,
			string search,
			out (int line, int column) lineColumn,
			int startLine = 0,
			int startColumn = 0
		)
		{
			lineColumn = (-1, -1);
			var ss = docView.TextBuffer.CurrentSnapshot;
			foreach( var line in ss.Lines )
			{
				if( line.LineNumber < startLine || line.Length < search.Length )
				{
					continue;
				}
				var text = line.GetText();
				var column = text.IndexOf( search, startColumn );
				if( startColumn != 0 )
				{
					startColumn = 0;
				}
				if( column != -1 )
				{
					lineColumn = (line.LineNumber, column);
					return true;
				}
			}
			return false;
		}

		public static async Task ExperimentWithFindAsync( DTE2 dte )
		{
			var find = dte.Find;
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			find.FindWhat = "HELLO";
			find.Target = vsFindTarget.vsFindTargetCurrentDocument;
			find.Action = vsFindAction.vsFindActionFind;
			find.Backwards = false;
			find.MatchCase = true;
			find.MatchInHiddenText = true;
			find.ResultsLocation = vsFindResultsLocation.vsFindResultsNone;
			var findResult = find.Execute();
			if( findResult == vsFindResult.vsFindResultFound )
			{
				var fs = find.ToString();
			}
		}

		public static async Task AnalyzeDocumentWindowsAsync()
		{
			var shell = await VS.Services.GetShellAsync();
			var uiShell = await VS.Services.GetUIShellAsync();
			IEnumWindowFrames enumWindows;

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var docWindows = uiShell.GetDocumentWindowEnum( out enumWindows );
			IVsWindowFrame[] rgelt = new IVsWindowFrame[ 1 ];
			uint pceltFetched;
			while( enumWindows.Next( 1, rgelt, out pceltFetched ) == VSConstants.S_OK )
			{
				if( pceltFetched == 0 )
				{
					break;
				}
				var x = rgelt[ 0 ];
				var s = x.ToString();
			}
		}

		public static async Task AnalyzeToolWindowsAsync( DocumentView docView )
		{
			var docWindowFrame = docView.WindowFrame;

			// Results, Messages are not tool windows
			var windowFrames = await VS.Windows.GetAllToolWindowsAsync();
			foreach( var windowFrame in windowFrames )
			{
				var guid = windowFrame.Editor;
				var x = windowFrame.Caption;
			}
		}
	}
}
