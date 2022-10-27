using SharedSrc.Helpers;
using SourceControlDeepLinks.Options;
using SourceControlDLShared.Helpers;

namespace SourceControlDeepLinks.Helpers
{
	public class ProviderFactory
	{
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

				case SourceProvider.Other:
					return OtherProviderHelper.GetDefault(_appSettings );

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

				case SourceProvider.BitbucketServer:
					return BitbucketProviderHelper.GetBitbucketDeepLink
					(
						providerInfo,
						remoteOrigin,
						repoRoot,
						activeFilePath,
						bookmarkedLines
					);

				case SourceProvider.Other:
					return OtherProviderHelper.GetOtherDeepLink
					(
						providerInfo,
						remoteOrigin,
						repoRoot,
						activeFilePath,
						bookmarkedLines
					);
			}

			return null;
		}
	}
}
