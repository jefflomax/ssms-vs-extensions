using SharedSrc.Helpers;
using SourceControlDeepLinks.Options;

namespace SourceControlDeepLinks.Helpers
{
	public static class GithubProviderHelper
	{
		public static ProviderInfo GetDefault( AppSettingsHelper appSettingsHelper )
		{
			var domain = appSettingsHelper.GetString( "GithubDomain" );
			var scm = appSettingsHelper.GetString( "GithubScm" );
			var baseUrl = appSettingsHelper.GetString( "GithubBase" );
			var pi = new ProviderInfo();
			pi.Set( domain, baseUrl, scm, "", false );
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
			// https://github.com/jefflomax/
			var domain = providerInfo.Domain;
			var bitbucketBaseFormat = providerInfo.BaseUrl;
			var repoBase = string.Format( bitbucketBaseFormat, domain );

			// blob
			var prefix = providerInfo.ProjectPrefix;

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

			// Find the Repository name
			var endRepoName = remoteRepoUrl.LastIndexOf( ".git" );
			var startRepoName = remoteRepoUrl.LastIndexOf( '/', endRepoName - 1 );
			var repoName = remoteRepoUrl.Substring
			(
				startRepoName + 1,
				endRepoName - startRepoName - 2
			);

			// Get the relative file path within the repo
			var filePathInRepo = filePath.Substring( repoRoot.Length + 1 );

			// BB Urls cannot just be run thru [System.Web.HTTPUtility]::UrlEncode or [System.Runtime] [Uri]::EscapeDataString they do not
			// support + for space or %2F for /
			var filePathFragment = filePathInRepo
				.Replace( '\\', '/' )
				.Replace( " ", "%20" );

			var lines = string.IsNullOrEmpty( bookmarkedLines )
				? string.Empty
				: $"#{bookmarkedLines}";

			// Build the BB Source URL
			var deepLink =
				$"{repoBase}/{repoName}/{branch}/{prefix}/{filePathFragment}{lines}";

			return new ProviderLinkInfo( deepLink, filePathInRepo );
		}
	}
}
