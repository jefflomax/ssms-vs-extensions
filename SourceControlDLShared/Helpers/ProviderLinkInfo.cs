namespace SourceControlDeepLinks.Helpers
{
	public class ProviderLinkInfo
	{
		public ProviderLinkInfo
		(
			string deepLink,
			string pathInRepo
		)
		{
			DeepLink = deepLink;
			PathInRepo = pathInRepo;
		}
		public string DeepLink { get; }
		public string PathInRepo { get; }
	}
}
