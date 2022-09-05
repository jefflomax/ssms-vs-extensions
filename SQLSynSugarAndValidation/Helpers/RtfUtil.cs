using System;
using System.Text;

namespace SQLSynSugarAndValidation.Helpers
{
	public static class RtfUtil
	{
		private static readonly string _prefix = @"{\rtf1\ansi ";
		// In RTF, font size is in half points, supposed to default to 24
		private static readonly string _fontSize = @"\fs{0} ";
		private static readonly string _colorTable = @"{\colortbl;\red0\green0\blue0;\red16\green208\blue16;}";
		private static readonly string _suffix = @" }";
		private static readonly string _boldOn = @"\b ";
		private static readonly string _boldOff = @"\b0 ";

		private static readonly string[] _keywords =
		{
			"declare_typeof",
			"index_join",
			"index_join_on",
			"select_matching",
			"select_matching_parm"
		};

		public static string GetEmpty( string message )
		{
			return $"{_prefix}{message}{_suffix}";
		}

		public static string FormatParseTree
		(
			string lisp,
			int fontSize = 16
		)
		{
			if( lisp == null )
			{
				return $"{_prefix} No data {_suffix}";
			}

			var sb = new StringBuilder( lisp.Length * 3 );
			sb.Append( _prefix );
			sb.AppendFormat( _fontSize, fontSize );
			sb.Append( _colorTable );
			var beginBold = false;
			var endBold = false;
			var newKeyword = false;
			var index = 0;
			foreach( var ch in lisp )
			{
				if( beginBold )
				{
					sb.Append( _boldOn );
					beginBold = false;
					endBold = true;
					if( IsNewKeyword( lisp, ch, index) )
					{
						newKeyword = true;
						sb.Append( @"{\cf2 " );
					}
				}
				if( endBold &&
					ch != '_' &&
					! char.IsLetterOrDigit( ch ) )
				{
					if( newKeyword )
					{
						newKeyword = false;
						// end color
						sb.Append( @" }" );
					}
					sb.Append( _boldOff );
					endBold = false;
				}

				if( ch == '(' )
				{
					beginBold = true;
				}
				sb.Append( ch );
				index++;
			}
			sb.Append( _suffix );
			return sb.ToString();
		}

		private static bool IsNewKeyword( string lisp, char ch, int index )
		{
			if( "dis".IndexOf(ch) == -1 )
			{
				return false;
			}
			int charsRemaining = lisp.Length - index;
			foreach( var keyword in _keywords )
			{
				var l = keyword.Length;
				if( ch != keyword[0] || l > charsRemaining )
				{
					continue;
				}
				if( lisp.Substring(index,l) == keyword )
				{
					return true;
				}
			}
			return false;
		}
	}
}
