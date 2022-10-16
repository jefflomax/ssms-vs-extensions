using SharedSrc.Helpers;
using SourceControlDeepLinks.Options;
using SourceControlDLSharedNoDep.Helpers;

namespace SourceControlDeepLinks.Helpers
{
	public static class GithubProviderHelper
	{
		public static ProviderInfo GetDefault( AppSettingsHelper appSettingsHelper )
		{
			var originRegex = appSettingsHelper.GetString( "GithubOriginRegex" );
			var sourceLinkTemplate = appSettingsHelper.GetString( "GithubSourceLinkTemplate" );

			var pi = new ProviderInfo();
			pi.Set( originRegex, sourceLinkTemplate, "", false );
			return pi;
		}

		public static ProviderLinkInfo GetDeepLink
		(
			ProviderInfo providerInfo,
			string remoteRepoUrl,
			string repoRoot,
			string filePath,
			string bookmarkedLines,
			string currentBranch
		)
		{
			var captures = ProviderHelper.ResolveRegex
			(
				providerInfo.OriginRegex,
				remoteRepoUrl
			);

			var template = providerInfo.SourceLinkTemplate;

			var branch = "main";
			if( providerInfo.UseDefaultBranch &&
				!string.IsNullOrEmpty( providerInfo.DefaultBranch ) )
			{
				branch = providerInfo.DefaultBranch;
			}
			else if( !string.IsNullOrEmpty( currentBranch ) )
			{
				branch = currentBranch;
			}


			// Get the relative file path within the repo
			var filePathInRepo = filePath.Substring( repoRoot.Length + 1 );

			// BB Urls cannot just be run thru [System.Web.HTTPUtility]::UrlEncode or [System.Runtime] [Uri]::EscapeDataString they do not
			// support + for space or %2F for /
			var filePathFragment = filePathInRepo
				.Replace( '\\', '/' )
				.Replace( " ", "%20" );

			// Github seems to support only #LNN or #LNN-LNN
			var lines = string.IsNullOrEmpty( bookmarkedLines )
				? string.Empty
				: FirstBookmark(bookmarkedLines);

			var deepLink = ProviderHelper.TranslateUrl
			(
				template,
				branch,
				filePathFragment,
				lines,
				captures
			);

			return new ProviderLinkInfo( deepLink, filePathInRepo );
		}


		private static string FirstBookmark( string bookmarks )
		{
			var iComma = bookmarks.IndexOf( ',' );
			if( iComma == -1 )
			{
				return $"#L{bookmarks}";
			}
			return $"#L{bookmarks.Substring(0, iComma)}";
		}
	}
}
