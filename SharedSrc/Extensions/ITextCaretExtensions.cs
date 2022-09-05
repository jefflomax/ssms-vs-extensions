using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedSrc.Extensions
{
	public static class ITextCaretExtensions
	{
		public static int GetBufferPosition(this ITextCaret iTextCaret )
		{
			return iTextCaret
				.Position
				.BufferPosition
				.Position;
		}
	}
}
