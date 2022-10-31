using SharedSrc.Helpers;
using SourceControlDeepLinks.Options;
using SourceControlDLShared.Helpers;
using SourceControlDLSharedNoDep.Helpers;

namespace SourceControlDeepLinks.Helpers
{
	public class ProviderFactory
	{
#if false
		AppSettingsHelper _appSettings;
		public ProviderFactory()
		{
			_appSettings = new AppSettingsHelper();
		}

		public ProviderInfo GetProviderDefaults
		(
			SourceProvider sourceProvider
		)
		{
			switch( sourceProvider )
			{
				case SourceProvider.BitbucketServer:
					return BitbucketProviderHelper.GetDefault( _appSettings );

				case SourceProvider.GitHub:
					return GithubProviderHelper.GetDefault( _appSettings );

			}
			return new ProviderInfo();
		}
#endif

		public ProviderLinkInfo GetDeepLink
		(
			ProviderInfo providerInfo,
			string remoteOriginUrl,
			string repoRoot,
			string activeFilePath,
			string bookmarkedLines,
			string currentBranch
		)
		{
			var captures = ProviderHelper.ResolveRegex
			(
				providerInfo.OriginRegex,
				remoteOriginUrl
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
			var filePathInRepo = activeFilePath.Substring( repoRoot.Length + 1 );
			// BB Urls cannot just be run thru [System.Web.HTTPUtility]::UrlEncode or [System.Runtime] [Uri]::EscapeDataString they do not
			// support + for space or %2F for /
			var filePathFragment = filePathInRepo
				.Replace( '\\', '/' )
				.Replace( " ", "%20" );

			var lines = string.IsNullOrEmpty( bookmarkedLines )
				? string.Empty
				: $"#{bookmarkedLines}";

			// Build the Bitbucket Deep Link Source URL
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
	}
}
