
namespace SQLPaste.Processor
{
	public class LineType
	{
		private int lineCount;
		private int extraNewlineChars;
		private int extraQuotedChars;
		private int extraEscapedChars;
		private int extraCoercedChars;
		private int[] coercableExtraChars = null;
		private string lineTemplate;

		public LineType( string template )
		{
			lineCount = 1;
			lineTemplate = template;
			extraEscapedChars = 0;
			extraNewlineChars = 0;
			extraQuotedChars = 0;
			extraCoercedChars = 0;
			coercableExtraChars = new int[ template.Length ];
		}

		public bool ColumnTypeMatch( string template )
		{
			return lineTemplate == template;
		}

		public bool LengthMatch( int length )
		{
			return lineTemplate.Length == length;
		}

		public void AddEscapedChars( int chars )
		{
			extraEscapedChars += chars;
		}

		public void AddQuotedChars( int chars )
		{
			extraQuotedChars += chars;
		}

		public void AddNewlineChars( int chars )
		{
			extraNewlineChars += chars;
		}

		public void AddCoercableExtraChars( int column, int chars )
		{
			coercableExtraChars[ column ] += chars;
		}

		public void IncludeCoercableExtraChars( int column )
		{
			extraCoercedChars += coercableExtraChars[ column ];
			coercableExtraChars[ column ] = 0;
		}

		public void ClearAllCoercableExtraCharsColumns()
		{
			for( var i = 0; i < coercableExtraChars.Length; i++ )
			{
				coercableExtraChars[ i ] = 0;
			}
		}

		/// <summary>
		/// Get extra chars needed due to CR/LF additions, quotes around fields,
		/// quotes to escape a char within a field, and quotes around integers
		/// coerced to alphanumeric
		/// </summary>
		/// <returns></returns>
		public int AllExtraChars()
		{
			return extraQuotedChars + extraNewlineChars +
				extraEscapedChars + extraCoercedChars;
		}

		public string GetTemplate()
		{
			return lineTemplate;
		}

		public int GetLines()
		{
			return lineCount;
		}

		public void IncrementLineCount()
		{
			lineCount++;
		}

		public char[] ColumnTemplate()
		{
			return lineTemplate.ToCharArray();
		}

		public void SetTemplate( char[] charArray )
		{
			lineTemplate = new string( charArray );
		}

#if DEBUG
		public (int extraEscapedChars,
			int extraNewlineChars,
			int extraQuotedChars,
			int extraCoercedChars) GetInternalsForUnitTests()
		{
			return (extraEscapedChars, extraNewlineChars, extraQuotedChars, extraCoercedChars);
		}
#endif
	}
}
