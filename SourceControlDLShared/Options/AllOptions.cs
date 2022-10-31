using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Community.VisualStudio.Toolkit;
using SourceControlDeepLinks;

namespace SourceControlDeepLinks.Options
{
	public class AllOptions
	{
		public ExtensionOptions General { get; }
		public Provider1Options Provider1 { get; }
		public Provider2Options Provider2 { get; }
		public Provider3Options Provider3 { get; }

		public AllOptions
		(
			ExtensionOptions general,
			Provider1Options p1,
			Provider2Options p2,
			Provider3Options p3
		) 
		{
			General = general;
			Provider1 = p1;
			Provider2 = p2;
			Provider3 = p3;
		}

		public ProviderInfo GetMatchingProvider( string originUrl )
		{
			bool ProviderMatch<T>
			(
				ProviderBase<T> provider,
				out ProviderInfo providerInfo
			) where T : BaseOptionModel<T>, new()
			{
				providerInfo = null;
				if( provider == null )
				{
					return false;
				}
				if( !provider.Enabled )
				{
					return false;
				}
				var regex = new Regex( provider.OriginMatch );
				var match = regex.IsMatch( originUrl );
				if( match )
				{
					providerInfo = new ProviderInfo();
					providerInfo.Set
					(
						provider.OriginRegex,
						provider.SourceLinkTemplate,
						provider.DefaultBranch,
						provider.UseDefaultBranch,
						provider.BookmarkType
					);
				}
				return match;

			}

			if( ProviderMatch<Provider1Options>( Provider1, out var providerInfo1 ) )
			{
				return providerInfo1;
			}

			if( ProviderMatch<Provider2Options>( Provider2, out var providerInfo2 ) )
			{
				return providerInfo2;
			}

			if( ProviderMatch<Provider3Options>( Provider3, out var providerInfo3 ) )
			{
				return providerInfo3;
			}

			return null;
		}

	}
}
