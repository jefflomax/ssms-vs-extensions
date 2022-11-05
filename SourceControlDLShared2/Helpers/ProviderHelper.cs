using SourceControlDLShared2.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace SourceControlDLShared2.Helpers
{
	public static class ProviderHelper
	{
		public static Dictionary<string, string> ResolveRegex
		(
			string originRegex,
			string originUrl
		)
		{
			var captures = new Dictionary<string, string>();
			if( string.IsNullOrEmpty(originRegex) )
			{
				return captures;
			}
			var regex = new Regex( originRegex );

			var match = regex.Match(originUrl);
			if( match.Success )
			{
				var groups = match.Groups;

				foreach( var groupName in regex.GetGroupNames() )
				{
					var group = groups[groupName];
					if( group.Name.Length > 2 && group.Value.Length > 0 )
					{
						captures.Add( groupName, group.Value );
					}
				}
			}

			return captures;
		}

		public static string TranslateUrl
		(
			string template,
			string branch,
			string filePathFragment,
			Dictionary<string, string> captures
		)
		{
			int iSpace;
			var startIndex = 0;
			var sb = new StringBuilder( template.Length );
			var space = ' ';

			while( startIndex < template.Length )
			{
				iSpace = template.IndexOf( space, startIndex );
				if( iSpace != -1 )
				{
					sb.Append( template.Substring( startIndex, iSpace - startIndex ) );

					startIndex += iSpace - startIndex + 1;
					var endIndex = template.IndexOf( space, startIndex );
					if( endIndex == -1 )
					{
						endIndex = template.Length;
					}

					var substitution = template.Substring( iSpace + 1, endIndex - startIndex );

					// From Code
					if( substitution == "file" )
					{
						sb.Append( filePathFragment );
					}
					else if( substitution == "branch" )
					{
						sb.Append( branch );
					}
					// From Capture Group
					else if( captures.TryGetValue( substitution, out var capture ) )
					{
						sb.Append( capture );
					}

					startIndex = endIndex + 1;
				}
				else
				{
					sb.Append( template.Substring( startIndex ) );
					startIndex = template.Length;
				}
			}

			return sb.ToString();
		}

		public static string AddBookmarks
		(
			BookmarkTypeEnum bookmarkType,
			string deepLink,
			string lines
		)
		{
			if( lines.Length == 0 )
			{
				return deepLink;
			}

			var bookmarks = lines;
			var iComma = bookmarks.IndexOf( comma );
			if( bookmarkType == BookmarkTypeEnum.Single )
			{
				bookmarks = FirstBookmark( bookmarks, iComma, "L" );
			}
			else if( bookmarkType == BookmarkTypeEnum.FirstDashLast)
			{
				bookmarks = FirstAndLastBookmark( bookmarks, iComma, "L", "-" );
			}
			else if( bookmarkType == BookmarkTypeEnum.FirstCommaLast)
			{
				bookmarks = FirstAndLastBookmark( bookmarks, iComma, "", "," );
			}
			return deepLink + '#' + bookmarks;
		}

		private const char comma = ',';
		private static string FirstBookmark
		(
			string bookmarks,
			int iComma,
			string prefix
		)
		{
			if( iComma == -1 )
			{
				return $"{prefix}{bookmarks}";
			}
			return $"{prefix}{bookmarks.Substring( 0, iComma )}";
		}

		private static string FirstAndLastBookmark
		(
			string bookmarks,
			int iComma,
			string prefix,
			string delimiter
		)
		{
			if( iComma == -1 )
			{
				return FirstBookmark( bookmarks, iComma, prefix );
			}
			var iLastComma = bookmarks.LastIndexOf( comma );
			return $"{FirstBookmark(bookmarks, iComma, prefix )}{delimiter}{prefix}{bookmarks.Substring(iLastComma + 1)}";
		}
	}
}
