using System;
using System.Collections.Generic;
using System.Text;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Threading;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio;
using SharedSrc.Commands;
using SharedSrc.Extensions;
using SharedSrc.Helpers;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using EnvDTE;
using EnvDTE80;
using DBSchema.schema;
using System.IO;
using SharedSrc.Interfaces;


namespace SQLSynSugarAndValidation.Commands
{
	/// <summary>
	/// SSMS - Replace executed statement
	/// </summary>
	[Command( PackageIds.ExecuteStatementCommand )]
	internal sealed class ExecuteStatementCommand
		: CommandShared<ExecuteStatementCommand, SQLSynSugarAndValidationPackage, ExtensionOptions>,
			ICommandShared<SQLSynSugarAndValidationPackage>
	{
#pragma warning disable 414
		private static readonly string QueryExecuteCommand = "Query.Execute";
		private static readonly string EditDocumentEndCommand = "Edit.DocumentEnd";
		private static readonly Guid ssmsEditor = new Guid( "b5a506eb-11be-4782-9a18-21265c2ca0b4" );
#pragma warning restore 414

		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var nl = Environment.NewLine;

			var (hasTextView, docView) = await GetActiveDocumentViewAsync();
			if( !hasTextView || docView.TextBuffer == null )
			{
				return;
			}

			//var caption = docView.WindowFrame.Caption;
			var editorGuid = docView.WindowFrame.Editor;

			var package = GetPackage;

			var state = package.GetOptionsState();
			var queryExecuteCommand = await VS
				.Commands
				.FindCommandAsync( QueryExecuteCommand );

			if( queryExecuteCommand == null )
			{
				await package
					.ToOutputPaneAsync( $"Failed to find {QueryExecuteCommand}" );
				return;
			}

			var textView = docView.TextView;
			//var options = textView.Options;
			//var visualElement = textView.VisualElement;
			var currentSnapshot = docView.TextBuffer.CurrentSnapshot;

			var caretBufferPosition = textView.Caret.GetBufferPosition();
			var restoreSelection = false;
			var selStartPosition = 0;
			var selEndPosition = 0;
			var selection = textView.Selection;
			string sql;
			if( selection.IsEmpty )
			{
				await package.ToOutputPaneAsync( "textView.Selection.IsEmpty" );
				sql = currentSnapshot.GetText();
			}
			else
			{
				// Box selection not supported
				if( selection.Mode != TextSelectionMode.Stream )
				{
					await package.ToOutputPaneAsync
					(
						$"Only Stream Selection Mode, supported, not Box"
					);
					return;
				}
				restoreSelection = true;
				(selStartPosition, selEndPosition) = selection.GetStartEnd();
				sql = selection.StreamSelectionSpan.GetText();
			}

			await package.ToOutputPaneAsync( state.DiagnosticOutput, $"({sql})" );

			//https://github.com/ctlajoie/DupSelection/blob/master/DupSelectionCommand.cs

			var caretAtEnd = currentSnapshot.MoveCaretToLastLineAndColumn
			(
				textView.Caret
			);

			var sb = new StringBuilder();
			var openingComment = $"/* GENERATED{nl}";
			sb.Append( openingComment );
			var runnableSql = await GetRewrittenSqlAsync
			(
				package,
				state,
				sql,
				nl
			);
			sb.Append( runnableSql );
			sb.Append( $"{nl}END GENERATED */" );
			var insertText = sb.ToString();

			var entireBufferSnapShot = docView.TextBuffer.Insert
			(
				caretBufferPosition,
				insertText
			);

			var insertedAt = caretBufferPosition;
			var sqlStart = insertedAt + openingComment.Length;
			var sqlEnd = sqlStart + runnableSql.Length;
			var snapshotSpan = entireBufferSnapShot.SpanFromTo
			(
				sqlStart,
				sqlEnd
			);

			selection.Select( snapshotSpan, isReversed: false );

			await package.ToOutputPaneAsync
			(
				state.DiagnosticOutput,
				$"HiddenSel {sqlStart} {sqlEnd} <{snapshotSpan.GetText()}>{nl}"
			);

			// Execute generated query
			await queryExecuteCommand.ExecuteAsync();

			// Delete generated query
			var entireInsertSpan = new Span(insertedAt,insertText.Length);
			var afterDeleteSnapshot = docView.TextBuffer.Delete( entireInsertSpan );

			// Restore Caret
			var oldCaretSnapshotPoint = afterDeleteSnapshot
				.SnapshotPointAt( caretBufferPosition );
			textView.Caret.MoveTo( oldCaretSnapshotPoint );

			// Restore Selection
			if( restoreSelection )
			{
				var selectionSnapshotSpan = afterDeleteSnapshot.SnapshotSpanFromBufferStartEnd
				(
					selStartPosition,
					selEndPosition
				);
				textView.Selection.Select
				(
					selectionSnapshotSpan,
					isReversed: false
				);
			}

		}

		private async Task<string> GetRewrittenSqlAsync
		(
			SQLSynSugarAndValidationPackage package,
			ExtensionOptions state,
			string originalSql,
			string nl
		)
		{
			// The schema is cached in the package
			var server = package.GetServer();

			var rewriter = new Rewriter.SqlRewriter();

			var dto = await rewriter.RewriteSql( server, originalSql );

			if( state.ShowParsed )
			{
				var hasParseTree = ! string.IsNullOrEmpty( dto.Validations.Lisp );
				package.SetToolWindowText
				(
					hasParseTree
						? dto.Validations.Lisp
						: string.Empty,
					dto.RewrittenSql,
					dto.Validations.OriginalFromParse
				);
			}

			package.SetToolWindowValidations( dto.Validations );

			return dto.RewrittenSql;
		}
#if false
		/// <summary>
		/// Given an IWpfTextView, find the position of the caret and report its column
		/// number. The column number is 0-based
		/// </summary>
		/// <param name="textView">The text view containing the caret</param>
		/// <returns>The column number of the caret's position. When the caret is at the
		/// leftmost column, the return value is zero.</returns>
		private static (int lineb0, int columnb0, Microsoft.VisualStudio.Text.Formatting.ITextViewLine caretTextViewLine)
			GetCaretLineColumn( IWpfTextView textView )
		{
			var line = textView.Caret.Position.BufferPosition.GetContainingLine();

			// This is the code the editor uses to populate the status bar.
			Microsoft.VisualStudio.Text.Formatting.ITextViewLine caretViewLine =
				textView.Caret.ContainingTextViewLine;

			var columnWidth = textView.FormattedLineSource.ColumnWidth;
			var column = (int)Math.Round
			(
				(textView.Caret.Left - caretViewLine.Left) / columnWidth
			);
			return (line.LineNumber, column, caretViewLine);
		}
#endif

	}
}
