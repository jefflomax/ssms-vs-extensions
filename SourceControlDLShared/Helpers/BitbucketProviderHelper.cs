using System;
using SharedSrc.Helpers;
using SourceControlDeepLinks.Options;
using SourceControlDLSharedNoDep.Helpers;

namespace SourceControlDeepLinks.Helpers
{
	public static class BitbucketProviderHelper
	{
		public static ProviderInfo GetDefault( AppSettingsHelper appSettingsHelper )
		{
			var originRegex = appSettingsHelper.GetString( "BitbuckerOriginRegex" );
			var sourceLinkTemplate = appSettingsHelper.GetString( "BitbuckerSourceLinkTemplate" );

			var pi = new ProviderInfo();
			pi.Set( originRegex, sourceLinkTemplate, "", false );
			return pi;
		}

		public static ProviderLinkInfo GetBitbucketDeepLink
		(
			ProviderInfo providerInfo,
			string remoteRepoUrl,
			string repoRoot,
			string filePath,
			string bookmarks
		)
		{
			var captures = ProviderHelper.ResolveRegex
			(
				providerInfo.OriginRegex,
				remoteRepoUrl
			);

			var template = providerInfo.SourceLinkTemplate;

			// Get the relative file path within the repo
			var filePathInRepo = filePath.Substring(repoRoot.Length + 1);
			// BB Urls cannot just be run thru [System.Web.HTTPUtility]::UrlEncode or [System.Runtime] [Uri]::EscapeDataString they do not
			// support + for space or %2F for /
			var filePathFragment = filePathInRepo
				.Replace( '\\', '/' )
				.Replace( " ", "%20" );

			var lines = string.IsNullOrEmpty(bookmarks)
				? string.Empty
				: $"#{bookmarks}";

			// Build the Bitbucket Deep Link Source URL
			var deepLink = ProviderHelper.TranslateUrl
			(
				template,
				"",
				filePathFragment,
				lines,
				captures
			);

			return new ProviderLinkInfo( deepLink, filePathInRepo );
		}
	}
}
