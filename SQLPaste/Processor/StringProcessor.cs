using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// TODO: This code considers ',' and possibly some other punctuation/symbol as Alpha

namespace SQLPaste.Processor
{
	public enum Direction { Horizontal, Vertical }
	public enum DelimiterType { Csv, Values }

	public class StringProcessor
	{
		const char tab = '\t';
		const char carriageReturn = '\r';
		const char lineFeed = '\n';
		const char openParen = '(';
		const char closeParen = ')';
		const char minus = '-';
		const char space = ' ';
		const string NULL = "NULL";
		const string escapedChars = "'";
		const string fieldDelimeter = ",";
		int lines = 0;

		bool isAlpha = true;
		bool isAlphaNumeric = true;
		bool isDate = true;
		int dateSeperators = 0;
		bool isDecimal = true;
		bool isGuid = true;
		bool isInteger = true;
		bool hasEncounteredNewline = false;
		int newLineCharacters = 0;
		bool isNull = true;
		bool finalLineProcessed = false;

		int columnLength = 0;
		int maxColumnLength = -1;
		int currentLineType = -1;
		int extraEscapedChars = 0;
		FormatSettings fs = null;
		List<char> types = new List<char>();
		List<LineType> lineTypes = new List<LineType>();

		public bool IsQuoted( char type, FormatSettings fs )
		{
			// This does not work for nullable (lowercase template)
			// columns - so is it worth doing at all?
			if( type == 'I' && fs.QuoteIntegers )
			{
				return true;
			}
			// Alpha, Number, Guid, Time
			return "ANGT".IndexOf( type ) != -1;
		}

		private bool QuotedHelper( char lowerType, FormatSettings fs )
		{
			if( lowerType == 'i' && fs.QuoteIntegers )
			{
				return true;
			}
			// Alpha, Number, Guid, Time
			return "angt".IndexOf( lowerType ) != -1;
		}

		private bool AlphaNumericCoersableQuotedHelper( char lowerType, FormatSettings fs )
		{
			if( lowerType == 'i' && fs.QuoteIntegers )
			{
				return false;
			}
			// Int coercable to N may need extra chars IFF coerced
			return lowerType == 'i';
		}

		public (List<LineType> lineTypes, int lines, int maxColumnLength)
			Process
			(
				string s,
				Direction direction = Direction.Horizontal,
				DelimiterType delimeterType = DelimiterType.Csv,
				bool quoteIntegerColumns = false
			)
		{
			void SetColumnType()
			{
				if( columnLength == 0 )
				{
					types.Add( 'E' );
				}
				else if( isGuid && isAlphaNumeric && columnLength == 36 )
				{
					types.Add( 'G' );
				}
				else if( isDate && dateSeperators > 1 )
				{
					types.Add( 'T' );
				}
				else if( isInteger )
				{
					types.Add( 'I' );
				}
				else if( isDecimal )
				{
					types.Add( 'D' );
				}
				else if( isAlpha && columnLength == 4 && isNull )
				{
					types.Add( 'X' );
				}
				else if( isAlpha )
				{
					types.Add( 'A' );
				}
				else if( isAlphaNumeric )
				{
					types.Add( 'N' );
				}
			}

			bool ApplyColumnTypeLimitations( char ch )
			{
				if( Char.IsDigit( ch ) )
				{
					isAlpha = false;
					columnLength++;
					return true;
				}

				if( ch == '.' ) // localize for , or . for decimal
				{
					isAlpha = false;
					isInteger = false;
					isGuid = false;
					columnLength++;
					return true;
				}

				if( Char.IsLetter( ch ) )
				{
					if( isNull )
					{
						switch( columnLength )
						{
							case 0:
								if( ch != 'N' )
								{
									isNull = false;
								}
								break;
							case 1:
								if( ch != 'U' )
								{
									isNull = false;
								}
								break;
							case 2:
							case 3:
								if( ch != 'L' )
								{
									isNull = false;
								}
								break;
							default:
								isNull = false;
								break;
						}
					}
					isInteger = false;
					isDate = false; // day AM/PM
					isDecimal = false;
					columnLength++;
					return true;
				}

				if( Char.IsPunctuation( ch ) )
				{
					// . handled prior
					if( ch == minus )
					{
						if( columnLength == 0 )
						{
							isDate = false;
						}
						else
						{
							isInteger = false;
							isDecimal = false;
						}
					}

					var dashOrColon = ch == minus || ch == ':';
					if( dashOrColon )
					{
						dateSeperators++;
					}
					else
					{
						isDate = false;
					}

					if( ch != minus )
					{
						isGuid = false;
					}
				}

				if( Char.IsSymbol( ch ) )
				{
					isDate = false;
					isGuid = false;
					isInteger = false;
					isDecimal = false;
				}

				if( Char.IsWhiteSpace( ch ) )
				{
					isGuid = false;
					isInteger = false;
					isDecimal = false;
					if( ch != space )
					{
						isDate = false;
					}
				}

				return false;
			}

			void ColumnEnd()
			{
				SetColumnType();
				if( columnLength > maxColumnLength )
				{
					maxColumnLength = columnLength;
				}
				Reset();
			}

			void ColumnOrLineEnd( bool lastLine )
			{
				void AddLineEndExtraCharacters
				(
					FormatSettings formatSettings,
					int newLineChars,
					LineType lineToMatch,
					bool isFinalLine
				)
				{
					if( newLineChars == 0 && !isFinalLine )
					{
						return;
					}
					var lineEndLength = formatSettings
						.GetLineEndLength( isFinalLine );
					if( newLineCharacters != lineEndLength )
					{
						lineToMatch.AddNewlineChars
						(
							lineEndLength - newLineCharacters
						);
					}
				}

				ColumnEnd();

				var newColumns = string.Join( "", types );
				if( lineTypes.Count == 0 )
				{
					var lineToMatch = new LineType( newColumns );
					lineTypes.Add( lineToMatch );
					currentLineType = 0;
					AddLineEndExtraCharacters
					(
						fs,
						newLineCharacters,
						lineToMatch,
						lastLine
					);
					AddNewLineQuotedExtraCharacters
					(
						lineToMatch,
						fs
					);
					lineToMatch.AddEscapedChars( extraEscapedChars );
				}
				else
				{
					var match = false;
					var lineToMatch = lineTypes[ currentLineType ];

					AddLineEndExtraCharacters
					(
						fs,
						newLineCharacters,
						lineToMatch,
						lastLine
					);

					// exact match?
					if( lineToMatch.ColumnTypeMatch( newColumns ) )
					{
						lineToMatch.IncrementLineCount();
						lineToMatch.AddEscapedChars( extraEscapedChars );
						AddNewLineQuotedExtraCharacters
						(
							lineToMatch,
							fs
						);

						match = true;
					}
					else
					{
						// TODO: don't think this works, no tests
						if( !lineToMatch.LengthMatch( newColumns.Length ) )
						{
							lineToMatch.AddEscapedChars( extraEscapedChars );
							extraEscapedChars = 0;
							lineTypes.Add( new LineType( newColumns ) );
							currentLineType++;
						}

						// can coerce?
						var columns = lineToMatch.ColumnTemplate();
						var canCoerceColumns = new BitArray( columns.Length, false );
						var canCoerce = true;
						var coercedColumn = false;
						for( var i = 0; i < columns.Length; i++ )
						{
							var newType = newColumns[ i ];
							var oldType = columns[ i ];

							if( oldType == newType )
							{
								canCoerceColumns.Set( i, true );
							}
							else
							{
								var newTypeLower = Char.ToLower( newType );
								var oldTypeLower = Char.ToLower( oldType );
								coercedColumn = false;

								// Convert between NOT NULL type and NULLed same type
								if( newTypeLower == Char.ToLower( oldType ) )
								{
									columns[ i ] = newTypeLower;
									coercedColumn = true;
								}
								// convert from NULL to type
								else if( oldType == 'X' && newType != 'X' )
								{
									columns[ i ] = newTypeLower;
									coercedColumn = true;
								}
								else if( newType == 'X' && oldType != 'X' )
								{
									columns[ i ] = oldTypeLower;
									coercedColumn = true;
								}

								// Coerce Alphabetic and Integer into alphaNumeric
								if
								(
									(newTypeLower == 'a' || newTypeLower == 'n') &&
									(oldTypeLower == 'i' || oldTypeLower == 'a' || oldTypeLower == 'n')
								)
								{
									columns[ i ] = Char.IsLower( newType ) || Char.IsLower( oldType )
										? 'n'
										: 'N';
									coercedColumn = true;
									lineToMatch.IncludeCoercableExtraChars( i );
								}
								else if( newTypeLower == 'i' &&
									(oldTypeLower == 'n' || oldTypeLower == 'a')
								)
								{
									columns[ i ] = Char.IsLower( newType ) || Char.IsLower( oldType )
										? 'n'
										: 'N';
									coercedColumn = true;
									lineToMatch.IncludeCoercableExtraChars( i );
								}

								canCoerceColumns.Set( i, coercedColumn );
							}
						}
						canCoerce = canCoerceColumns.Cast<bool>().Any( x => x );

						if( canCoerce )
						{
							// Could move all the qoted type counts here
							AddNewLineQuotedExtraCharacters
							(
								lineToMatch,
								fs
							);
							lineToMatch.SetTemplate( columns );
							lineToMatch.IncrementLineCount();
							lineToMatch.AddEscapedChars( extraEscapedChars );
							match = true;
						}
						else
						{
							lineToMatch.AddEscapedChars( extraEscapedChars );
							var newLineType = new LineType( newColumns );
							lineTypes.Add( newLineType );
							currentLineType++;
							AddNewLineQuotedExtraCharacters
							(
								newLineType,
								fs
							);
							match = true;
						}
					}

					if( !match )
					{
						lineToMatch.AddEscapedChars( extraEscapedChars );
						lineTypes.Add( new LineType( newColumns ) );
						currentLineType++;
					}
				}

				extraEscapedChars = 0;
				lines++;
				types.Clear();
			}

			void AddNewLineQuotedExtraCharacters
			(
				LineType lineType,
				FormatSettings fs
			)
			{
				var chars = lineType.ColumnTemplate();
				for( var i = 0; i < chars.Length; i++ )
				{
					var columnTypeLower = Char.ToLower( chars[ i ] );

					if( QuotedHelper( columnTypeLower, fs ) )
					{
						lineType.AddQuotedChars( 2 );
					}

					if( AlphaNumericCoersableQuotedHelper( columnTypeLower, fs ) )
					{
						lineType.AddCoercableExtraChars( i, 2 );
					}
				}
			}

			fs = new FormatSettings
			(
				direction,
				delimeterType,
				s,
				quoteIntegers: quoteIntegerColumns
			);

			foreach( var ch in s )
			{
				if( IsNewLine( ch ) )
				{
					// TODO: This should fail for \n\n
					newLineCharacters++;
					hasEncounteredNewline = true;
					finalLineProcessed = true;
					continue;
				}

				if( hasEncounteredNewline )
				{
					ColumnOrLineEnd( lastLine: false );
					hasEncounteredNewline = false;
					newLineCharacters = 0;
				}

				finalLineProcessed = false;
				if( ch == tab )
				{
					ColumnEnd();
					continue;
				}

				if( escapedChars.Contains( ch ) )
				{
					extraEscapedChars += 1;
				}

				if( ApplyColumnTypeLimitations( ch ) )
				{
					continue;
				}

				columnLength++;
			}// walk thru entire string

			if( !finalLineProcessed )
			{
				ColumnOrLineEnd( lastLine: true );
			}

			return (lineTypes, lines, maxColumnLength);
		}


		/// <summary>
		/// Get +/- count from clipboard text to formatted buffer
		/// Because of quoting columns, newlines, and columns with
		/// nulls the variation can't be computed, processing
		/// has to store the deltas.  Worse, a column could be alpha
		/// down to the very last row being NULL, so a running count
		/// is best
		/// </summary>
		/// <param name="formatSettings"></param>
		/// <returns></returns>
		public int GetExtraChars( FormatSettings formatSettings )
		{
			var extraChars = 0;
			foreach( var lineType in lineTypes )
			{
				var template = lineType.GetTemplate();
				var linesInTemplate = lineType.GetLines();

				extraChars += lineType.AllExtraChars();
				if( formatSettings.Values )
				{
					extraChars += linesInTemplate * 2; // ( ) around entries
				}
			}
			return extraChars;
		}

		public string GetFormattedText
		(
			string clipboardText,
			Direction direction,
			DelimiterType delimiterType = DelimiterType.Csv,
			bool quoteIntegers = false
		)
		{
			ReadOnlySpan<char> escapedCharsSpan = escapedChars.AsSpan();

			var roText = clipboardText.AsSpan();
			var roSlice = roText.Slice( 0 );

			bool isFirstColumn;
			bool isLineEnd;
			bool isLastLineNoCRLF = false;

			// is 1,x or x,1
			int maxRows = lineTypes.Sum( lt => lt.GetLines() );
			int maxColumns = lineTypes.Max( lt => lt.GetTemplate().Length );

			// could be local but sep call for ExtraChars needs
			fs = new FormatSettings
			(
				direction,
				delimiterType,
				maxColumns > 1 && maxRows > 1,
				quoteIntegers
			);

			string EscapedString
			(
				ReadOnlySpan<char> s,
				int start,
				int length,
				ReadOnlySpan<char> escapedCharsSpn
			)
			{
				var spn = s.Slice( start, length );
				var iEscapedChar = spn.IndexOfAny( escapedCharsSpn );
				if( iEscapedChar == -1 )
				{
					return spn.ToString();
				}
				var strWithCharsToEscape = spn.ToString();
				return strWithCharsToEscape.Replace( "'", "''" );
			}

			extraEscapedChars = GetExtraChars( fs );

			var expectedTotalBufferLength = clipboardText.Length + extraEscapedChars;

			var tb = new TextBuffer( expectedTotalBufferLength );

			foreach( var lineType in lineTypes )
			{
				var template = lineType.GetTemplate();
				ReadOnlySpan<char> lineEndChar;
				for( var l = 0; l < lineType.GetLines(); l++ )
				{
					var iLineEnd = roSlice.IndexOfAny( carriageReturn, lineFeed );
					if( iLineEnd == -1 )
					{
						isLastLineNoCRLF = true;
						iLineEnd = roSlice.Length;
						lineEndChar = roSlice.Slice( 0 );
					}
					else
					{
						lineEndChar = roSlice.Slice( iLineEnd );
					}

					isFirstColumn = true;
					isLineEnd = false;

					// Output each template
					if( fs.Values )
					{
						tb.Add( openParen );
					}
					foreach( var ch in template )
					{
						if( !isFirstColumn )
						{
							tb.Add( fieldDelimeter );
						}
						isFirstColumn = false;

						var iTab = roSlice.IndexOf( tab );
						var noTabFound = iTab == -1;
						if( iTab > iLineEnd || noTabFound )
						{
							isLineEnd = true;
							iTab = iLineEnd;
						}

						switch( ch )
						{
							case 'E':
								//								if( noTabFound )
								//								{
								//sb.Append( ',' );
								//								}
								break;
							// No Quotes, NULL same as any other output
							case 'I' when !fs.QuoteIntegers:
							case 'D':
							case 'd':
								tb.Add( roSlice.Slice( 0, iTab ) );
								break;
							// Quotes - but no internal escapes
							case 'G':
							case 'T':
								tb.Add( $"'{roSlice.Slice( 0, iTab ).ToString()}'" );
								break;
							case 'A':
							case 'I' when fs.QuoteIntegers:
							case 'N': // AlphaNumeric
								tb.Add( $"'{EscapedString( roSlice, 0, iTab, escapedCharsSpan )}'" );
								break;
							case 'X':
								tb.Add( NULL );
								break;
							case 'a':
							case 'g':
							case 'n':
							case 't':
								var nullOrQuoted = roSlice.Slice( 0, iTab ).ToString();
								if( nullOrQuoted == NULL )
								{
									tb.Add( NULL );
								}
								else
								{
									tb.Add( $"'{EscapedString( roSlice, 0, iTab, escapedCharsSpan )}'" );
								}
								break;
							case 'i':
								var nullOrUnquoted = roSlice.Slice( 0, iTab ).ToString();
								if( nullOrUnquoted == NULL )
								{
									tb.Add( NULL );
								}
								else
								{
									if( fs.QuoteIntegers )
									{
										tb.Add( $"'{nullOrUnquoted}'" );
									}
									else
									{
										tb.Add( nullOrUnquoted );
									}
								}
								break;
							case 'e':
								var nullOrEmpty = roSlice.Slice( 0, iTab ).ToString();
								if( nullOrEmpty == NULL )
								{
									tb.Add( NULL );
								}
								if( noTabFound )
								{
									tb.Add( fieldDelimeter );
								}
								break;
						}
						if( !(isLastLineNoCRLF && isLineEnd) )
						{
							roSlice = roSlice.Slice( iTab + 1 );
						}
						iLineEnd -= (iTab + 1);
					}

					if( fs.Values )
					{
						tb.Add( closeParen );
					}

					if( !isLastLineNoCRLF )
					{
						// If line end is CR LF pair
						// and the char past EOL is the opposite
						// bump forward
						// No empty line handling yet
						var ch = roSlice[ 0 ];
						if( ch == carriageReturn || ch == lineFeed )
						{
							if( lineEndChar[ 0 ] != ch )
							{
								roSlice = roSlice.Slice( 1 );
							}
						}

						tb.Add( GetLineEnd( fs, false ) );
					}
					else
					{
						// Final line w/o linefeed
						isLastLineNoCRLF = false;
						if( fs.Matrix )
						{
							tb.Add( Environment.NewLine );
						}
					}
				}
			}

			return tb.Result();
		}

		private string GetLineEnd( FormatSettings fs, bool isFinalLine )
		{
			return fs.GetLineEnd( isFinalLine );
		}

		private static bool IsNewLine( char ch )
		{
			return ch == lineFeed || ch == carriageReturn;
		}

		private void Reset()
		{
			isAlpha = true;
			isAlphaNumeric = true;
			isDate = true;
			dateSeperators = 0;
			isDecimal = true;
			isGuid = true;
			isInteger = true;
			isNull = true;
			columnLength = 0;
		}

		public override string ToString()
		{
			return $"LineTypes {lineTypes.Count} {string.Join( ",", lineTypes.Select( lt => lt.GetTemplate() ) )} Lines {lines}";
		}
	}
}
