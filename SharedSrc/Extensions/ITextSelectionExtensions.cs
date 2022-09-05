using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedSrc.Extensions
{
	public static class ITextSelectionExtensions
	{
		public static (int start, int end) GetStartEnd(this ITextSelection selection )
		{
			return (
				selection.Start.Position.Position,
				selection.End.Position.Position
			);
		}
	}
}
