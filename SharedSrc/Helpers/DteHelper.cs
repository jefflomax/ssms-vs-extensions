using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace SharedSrc.Helpers
{
	public static class DteHelper
	{
		public static async Task<DTE2> GetDTEAsync()
		{
#if false
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			DTE2 dte = await VS.GetServiceAsync<EnvDTE.DTE, DTE2>();
			return dte;
#endif
			return await OnMainThreadAsync<DTE2>( async () => {
				DTE2 dte = await VS.GetServiceAsync<EnvDTE.DTE, DTE2>();
				return dte;
			} );
		}

		public static async Task<(string fullName, string type )>
			GetDocumentInfoAsync( DTE2 dte )
		{
#if true
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var activeDocument = dte.ActiveDocument;
			return (activeDocument.FullName, activeDocument.Type );
#else
			return await OnMainThreadSyncAsync<DTE2,( string fullName, string type)>
				( (dte) => {
					//await Task.CompletedTask;
#pragma warning disable VSTHRD010
					var activeDocument = dte.ActiveDocument;
					return (activeDocument.FullName, activeDocument.Type);
#pragma warning restore VSTHRD010
				}, dte );
#endif
		}

		private static async Task<T> OnMainThreadAsync<T>( Func<Task<T>> f )
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			return await f();
		}

#if false
		private static async Task<T> OnMainThreadSyncAsync<P1,T>
		(
			Func<P1,Task<T>> f,
			P1 p
		)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			return f(p);
		}
#endif

		public static async Task<EnvDTE.TextSelection> GetEnvDTETextSelectionAsync(this EnvDTE80.DTE2 dte)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			return dte.ActiveDocument.Selection as EnvDTE.TextSelection;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ts"></param>
		/// <param name="line">base 1 line</param>
		/// <param name="column">base 1 column</param>
		/// <returns></returns>
		public static async Task MoveToLineAndOffsetAsync
		(
			this EnvDTE.TextSelection ts,
			int lineBase1,
			int columnBase1
		)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			ts.MoveToLineAndOffset( lineBase1, columnBase1 );
		}

		public static async Task<EnvDTE.TextDocument> GetTextDocumentAsync( this EnvDTE.Document doc )
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			return doc.Object( "TextDocument" ) as TextDocument;
		}

		public static async Task<TextSelection> GetSelectionAsync( this EnvDTE.TextDocument textDocument )
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			return textDocument.Selection;
		}

		public static async Task StartOfDocumentAsync( this EnvDTE.TextSelection selection )
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			selection.StartOfDocument();
		}

		public static async Task<bool> NextBookmarkAsync( this EnvDTE.TextSelection selection )
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			return selection.NextBookmark();
		}

		public static async Task<int> CurrentLineAsync( this EnvDTE.TextSelection selection )
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			return selection.CurrentLine;
		}

	}
}
