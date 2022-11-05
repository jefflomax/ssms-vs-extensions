namespace SourceControlDeepLinks.Resources
{
	public static partial class Constants
	{
		public const string ExtensionOutputPane = "Source Control Deep Links";
		public const string OptionsPageCategoryName = "Source Control Deep Links";
		public const string OptionsPageName = "General";
		public const string OptionsInfo = "Information";
		public const string Options1PageName = "Provider 1";
		public const string Options2PageName = "Provider 2";
		public const string Options3PageName = "Provider 3";
		public const string OptionsName = "Options";
		public const string SourceLink = "Source Link ";

		public const string PEnabled = "Enabled";
		public const string PEnabledDesc = "Enable/Disable provider";
		public const string PFriendly = "Friendly Name";
		public const string PFriendlyDesc = "Provider Name (optional, used in diagnostics)";
		public const string POriginMatch = "Origin Match";
		public const string POriginMatchDesc = "If string is Regex match of origin URL, this provider is used";
		public const string POriginRegex = "Origin Regex";
		public const string POriginRegexDesc = "Extract named RegEx captures like domain, profile, repo from origin URL to substitute into Source Link Template";
		public const string PSourceLinkTemplate = "Source Link Template";
		public const string PSourceLinkTemplateDesc = "URL template to source, SPACE delimit ' file ' ' branch ' and capture names";
		public const string PDefaultBranch = "Default Branch";
		public const string PDefaultBranchDesc = "Default branch (main, master, develop...)";
		public const string PUseDefaultBranch = "Use Default Branch";
		public const string PUseDefaultBranchDesc = "Use default instead of current branch";
		public const string PBookmarkType = "Bookmark Format";
		public const string PBookmarkTypeDesc = "Supported formats: Single=#L1  All=#1,2,3... FirstDashLast=#L1-L3 FirstCommaLast=#1,3";

	}
}
