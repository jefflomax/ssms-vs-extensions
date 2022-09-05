using System;

namespace SQLPaste.Processor
{
	public class FormatSettings : FormatBase
	{
		private readonly char[] crlf = { '\r', '\n' };
		const char tab = '\t';
		public FormatSettings
		(
			Direction direction,
			DelimiterType delimiterType,
			bool isMatrix,
			bool quoteIntegers
		) : base( direction, delimiterType, quoteIntegers )
		{
			Matrix = isMatrix;
		}

		public FormatSettings
		(
			FormatBase formatBase,
			bool isMatrix
		) : base( formatBase )
		{
			Matrix = isMatrix;
		}

		public FormatSettings
		(
			Direction direction,
			DelimiterType delimiterType,
			string clipboardText,
			bool quoteIntegers
		) : base( direction, delimiterType, quoteIntegers )
		{
			// We need to know if the data is a "matrix" before
			// it's processed, because line endings will vary.
			bool isMatrix = false;

			var firstLineEnd = clipboardText.IndexOfAny( crlf );
			if( firstLineEnd != -1 )
			{
				var firstLine = clipboardText
					.AsSpan()
					.Slice( 0, firstLineEnd );
				int tabs = 0;
				foreach( var ch in firstLine )
				{
					if( ch == tab )
					{
						tabs++;
					}
				}
				if( tabs > 0 )
				{
					isMatrix = true;
				}
			}
			Matrix = isMatrix;
		}

		public bool Matrix { get; }

		public int GetLineEndLength( bool isFinalLine )
		{
			if( isFinalLine && Matrix )
			{
				return Environment.NewLine.Length;
			}

			if( isFinalLine )
			{
				return 0;
			}

			if( CSV && Matrix )
			{
				// CSV matrix always line breaks
				return Environment.NewLine.Length;
			}

			return (Horizontal)
				? 1
				: 1 + Environment.NewLine.Length;
		}

		public string GetLineEnd( bool isFinalLine )
		{
			if( isFinalLine && Matrix )
			{
				return Environment.NewLine;
			}

			if( CSV && Matrix )
			{
				// CSV matrix always line breaks
				return Environment.NewLine;
			}

			return (Horizontal)
				? ","
				: $",{Environment.NewLine}";
		}

	}
}
