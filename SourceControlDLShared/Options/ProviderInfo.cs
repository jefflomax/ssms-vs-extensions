using SourceControlDLShared.Options;
using System;

namespace SourceControlDeepLinks.Options
{
	[Serializable]
	public class ProviderInfo
	{
		public ProviderInfo()
		{
			OriginRegex = string.Empty;
			SourceLinkTemplate = string.Empty;
			DefaultBranch = string.Empty;
			UseDefaultBranch = false;
		}

		public void Set
		(
			string originRegex,
			string sourceLinkTemplate,
			string defaultBranch,
			bool useDefaultBranch,
			BookmarkTypeEnum providerBookmarksType
		)
		{
			OriginRegex = originRegex;
			SourceLinkTemplate = sourceLinkTemplate;
			DefaultBranch = defaultBranch;
			UseDefaultBranch = useDefaultBranch;
			ProviderBookmarksType = providerBookmarksType;
		}
		public void Set( ProviderInfo from )
		{
			OriginRegex = from.OriginRegex;
			SourceLinkTemplate = from.SourceLinkTemplate;
			DefaultBranch = from.DefaultBranch;
			UseDefaultBranch = from.UseDefaultBranch;
			ProviderBookmarksType = from.ProviderBookmarksType;
		}


		public string OriginRegex { get; private set; }
		public string SourceLinkTemplate { get; private set; }
		public string DefaultBranch { get; private set; }
		public bool UseDefaultBranch { get; private set; }
		public BookmarkTypeEnum ProviderBookmarksType { get; private set; }

		public static BookmarkTypeEnum GetBookmarkType( string enumName )
		{
			var type = BookmarkTypeEnum.All;
			if( Enum.TryParse<BookmarkTypeEnum>( enumName, out var bookmarkType ) )
			{
				type = bookmarkType;
			}
			return type;
		}

		public string Serialize(string key)
		{
			// TODO: encode ',' to protect RegEx
			return $"V1{key};{OriginRegex},{SourceLinkTemplate},{DefaultBranch},{UseDefaultBranch},{ProviderBookmarksType}";
		}
		public static ProviderInfo Deserialize( char version, string s )
		{
			var pi = new ProviderInfo();
			pi.ProviderBookmarksType = BookmarkTypeEnum.All;

			var fields = s.Split( new char[]{','}, StringSplitOptions.None );

			if( version == '0' )
			{
				return Version0(pi, fields);
			}
			return Version1( pi, fields );
		}

		private static ProviderInfo Version1( ProviderInfo pi, string[] fields )
		{
			var l = fields.Length;

			if( l > 0 )
			{
				pi.OriginRegex = fields[0];
			}
			if( l > 1 )
			{
				pi.SourceLinkTemplate = fields[ 1 ];
			}
			if( l > 2 )
			{
				pi.DefaultBranch = fields[ 2 ];
			}
			if( l > 3 )
			{
				Boolean.TryParse( fields[ 3 ], out var b );
				pi.UseDefaultBranch = b;
			}
			if( l > 4 )
			{
				pi.ProviderBookmarksType = GetBookmarkType( fields[4] );
			}
			return pi;
		}

		private static ProviderInfo Version0( ProviderInfo pi, string[] fields )
		{
			var l = fields.Length;

			if( l > 0 )
			{
			}
			if( l > 1 )
			{
			}
			if( l > 2 )
			{
			}
			if( l > 3 )
			{
				pi.DefaultBranch = fields[ 3 ];
			}
			if( l > 4 )
			{
				Boolean.TryParse( fields[ 4 ], out var b );
				pi.UseDefaultBranch = b;
			}
			return pi;
		}
	}
}
