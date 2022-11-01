using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SharedSrc.Helpers;
using SourceControlDeepLinks.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControlDeepLinks.Options
{
	public class ProviderBase<T> : BaseOptionModel<T> where T : BaseOptionModel<T>, new()
	{
		public bool Enabled;
		public string FriendlyName;
		public string OriginMatch;
		public string OriginRegex;
		public string SourceLinkTemplate;
		public string DefaultBranch;
		public bool UseDefaultBranch;
		public BookmarkTypeEnum BookmarkType;

		protected void InitFromAppConfig
		(
			char provider,
			Type collectionType,
			string propertyName
		)
		{
			var settingsHelper = new SettingsHelper( collectionType );
			Check( provider, collectionType, settingsHelper, propertyName );
		}

		private void Check
		(
			char provider,
			Type collectionType,
			SettingsHelper settingsHelper,
			string propertyName
		)
		{
			_ = ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
			{
				var propertyExists = await settingsHelper.PropertyExistsAsync( collectionType, propertyName );
				if( !propertyExists )
				{
					var appSettings = new AppSettingsHelper();
					var prefix = "P" + provider;
					var origin = prefix + "OriginRegex";
					var template = prefix + "SourceLinkTemplate";
					var bookmarks = prefix + "BookmarksType";
					var name = prefix + "FriendlyName";
					var match = prefix + "Match";
					FriendlyName = appSettings.GetString( name );
					OriginRegex = appSettings.GetString( origin );
					OriginMatch = appSettings.GetString( match );
					SourceLinkTemplate = appSettings.GetString( template );
					var bookmarkEnum = appSettings.GetString( bookmarks );
					BookmarkType = ProviderInfo.GetBookmarkType( bookmarkEnum );
				}
			} );
		}
	}
}
