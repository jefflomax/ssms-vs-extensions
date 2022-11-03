using SourceControlDLShared2.Options;
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
			ProviderBookmarksType = BookmarkTypeEnum.All;
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

	}
}
