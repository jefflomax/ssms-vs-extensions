﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedSrc.Helpers;
using SourceControlDeepLinks.Options;

namespace SourceControlDeepLinks.Helpers
{
	public static class BitbucketProviderHelper
	{
		public static ProviderInfo GetDefault( AppSettingsHelper appSettingsHelper )
		{
			var domain = appSettingsHelper.GetString( "BitbucketDomain" );
			var scm = appSettingsHelper.GetString( "SCM" );
			var baseUrl = appSettingsHelper.GetString( "BitbucketBase" );
			var pi = new ProviderInfo();
			pi.Set( domain, baseUrl, scm, "", false );
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
			//var scm = appSettingsHelper.GetString("SCM");
			//var domain = appSettingsHelper.GetString("BitbucketDomain");
			//var bitbucketBaseFormat = appSettingsHelper.GetString("BitbucketBase");
			var scm = providerInfo.ProjectPrefix;
			var domain = providerInfo.Domain;
			var bitbucketBaseFormat = providerInfo.BaseUrl;

			var bitbucketBase = string.Format(bitbucketBaseFormat, domain);

			// Find the BB PROJECT which follows /scm/
			var projectIndexStart = remoteRepoUrl.IndexOf( scm ) + 5;
			var projectIndexEnd = remoteRepoUrl.IndexOf( "/", projectIndexStart );
			var project = remoteRepoUrl
				.Substring( projectIndexStart, projectIndexEnd - projectIndexStart )
				.ToUpper();

			// Find the Repository name
			var endOfRepoNameIndex = remoteRepoUrl.IndexOf( ".git", projectIndexEnd + 1 );
			var repoName = remoteRepoUrl.Substring( projectIndexEnd+1, endOfRepoNameIndex - projectIndexEnd - 1 );

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
			var deepLink = $"{bitbucketBase}/{project}/repos/{repoName}/browse/{filePathFragment}{lines}";

			return new ProviderLinkInfo( deepLink, filePathInRepo );
		}
	}
}