using SharedSrc.Helpers;
using SourceControlDeepLinks.Options;

namespace SourceControlDeepLinks.Helpers
{
	public class ProviderHelper
	{
		AppSettingsHelper _appSettings;
		public ProviderHelper
		(
			SourceControlDeepLinksPackage package
		)
		{
			_appSettings = new AppSettingsHelper( package );
		}

		public ProviderInfo GetProviderDefaults
		(
			SourceProvider sourceProvider
		)
		{
			switch( sourceProvider )
			{
				case SourceProvider.BitbucketServer:
					return BitbucketHelper.GetDefault( _appSettings );

				case SourceProvider.GitHub:
					return GithubProviderHelper.GetDefault( _appSettings );
			}
			return new ProviderInfo();
		}

		public ProviderLinkInfo GetDeepLink
		(
			SourceProvider sourceProvider,
			ProviderInfo providerInfo,
			string remoteOrigin,
			string repoRoot,
			string activeFilePath,
			string bookmarkedLines,
			string currentBranch
		)
		{
			switch( sourceProvider )
			{
				case SourceProvider.GitHub:
					return GithubProviderHelper.GetDeepLink
					(
						providerInfo,
						remoteOrigin,
						repoRoot,
						activeFilePath,
						bookmarkedLines,
						currentBranch
					);
			}

			return null;
		}
	}
}
