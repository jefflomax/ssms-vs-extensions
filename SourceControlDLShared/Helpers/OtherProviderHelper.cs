using SharedSrc.Helpers;
using SourceControlDeepLinks.Helpers;
using SourceControlDeepLinks.Options;
using SourceControlDLShared.Options;
using SourceControlDLSharedNoDep.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControlDLShared.Helpers
{
	public static class OtherProviderHelper
	{
		public static ProviderInfo GetDefault( AppSettingsHelper appSettingsHelper )
		{
			var pi = new ProviderInfo();
			var sourceLinkTemplate = @"https://YOUR_PROVIDER_DOMAIN/ profile / repo /blob/ branch / file";
			pi.Set( "", sourceLinkTemplate, "", false, BookmarkTypeEnum.All );
			return pi;
		}

		public static ProviderLinkInfo GetOtherDeepLink
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
			var filePathInRepo = filePath.Substring( repoRoot.Length + 1 );
			// BB Urls cannot just be run thru [System.Web.HTTPUtility]::UrlEncode or [System.Runtime] [Uri]::EscapeDataString they do not
			// support + for space or %2F for /
			var filePathFragment = filePathInRepo
				.Replace( '\\', '/' )
				.Replace( " ", "%20" );

			var lines = string.IsNullOrEmpty( bookmarks )
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
