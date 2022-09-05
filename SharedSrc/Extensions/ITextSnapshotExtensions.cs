using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedSrc.Extensions
{
	public static class ITextSnapshotExtensions
	{
		public static SnapshotPoint SnapshotPointAt
		(
			this ITextSnapshot iTextSnapshot,
			int bufferPosition
		)
		{
			return new SnapshotPoint( iTextSnapshot, bufferPosition );
		}

		public static SnapshotSpan SnapshotSpanFromBufferStartEnd
		(
			this ITextSnapshot iTextSnapshot,
			int bufferStartPosition,
			int bufferEndPosition
		)
		{
			var span = new Span
			(
				bufferStartPosition,
				bufferEndPosition - bufferStartPosition
			);
			return new SnapshotSpan
			(
				iTextSnapshot,
				span
			);
		}

		public static SnapshotSpan SpanFromTo
		(
			this ITextSnapshot iTextSnapshot,
			int from,
			int to
		)
		{
			var start = new SnapshotPoint( iTextSnapshot, from );
			var end = new SnapshotPoint( iTextSnapshot, to );

			var snapshotSpan = new SnapshotSpan( start, end );
			return snapshotSpan;
		}

		public static CaretPosition MoveCaretToLastLineAndColumn
		(
			this ITextSnapshot currentSnapshot,
			ITextCaret caret
		)
		{
			var bufferPositionBySnapLength = currentSnapshot.Length;
			var eodSnapPoint = new SnapshotPoint
			(
				currentSnapshot,
				bufferPositionBySnapLength
			);
			return caret.MoveTo( eodSnapPoint );
		}
	}
}
