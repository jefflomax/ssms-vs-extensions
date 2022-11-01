using System.ComponentModel;
using System.Runtime.InteropServices;
using SharedSrc.Interfaces;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using SharedSrc.Helpers;
using Community.VisualStudio.Toolkit;
using SourceControlDeepLinks.Helpers;
using SourceControlDeepLinks.Options;
using static SourceControlDeepLinks.Resources.Constants;
using Microsoft.VisualStudio.Shell;

namespace SourceControlDeepLinks.Options
{
	internal partial class OptionsProvider1
	{
		// See attributes in package class
		[ComVisible( true )]
		public class Provider1OptionsProv : BaseOptionPage<Provider1Options> 
		{
		}
	}


	public class Provider1Options : ProviderBase<Provider1Options>
	{
		private readonly bool _debug;
		private static bool _initChecked = false;

		// The .Instance property in this class is meant to be used
		// on the UI thread only

		public Provider1Options()
		{
			_debug = true;

			if( !_initChecked )
			{
				InitFromAppConfig( '1', typeof( Provider1Options ), nameof( Provider1Enabled ) );
				_initChecked = true;
			}
		}


		[Category( SourceLink + OptionsInfo )]
		[DisplayName( "Enabled" )]
		[Description( "Enable/Disable provider" )]
		[DefaultValue( true )]
		public bool Provider1Enabled { get { return Enabled; } set { Enabled = value; } }

		[Category( SourceLink + OptionsInfo )]
		[DisplayName( "Friendly Name" )]
		[Description( "Provider Name (optional)" )]
		public string ProviderFriendlyName { get { return FriendlyName; } set { FriendlyName = value; } }

		[Category( SourceLink + Options1PageName )]
		[DisplayName( "Origin Match" )]
		[Description( "Determines if this provider is used" )]
		public string Provider1OriginMatch { get { return OriginMatch; } set { OriginMatch = value; } }

		[Category( SourceLink + Options1PageName )]
		[DisplayName( "Origin Regex" )]
		[Description( "Extract named captures like domain, profile, repo, from origin URL to insert in Source Link Template" )]
		public string Provider1OriginRegex { get { return OriginRegex; } set { OriginRegex = value; } }

		[Category( SourceLink + Options1PageName )]
		[DisplayName( "Source Link Template" )]
		[Description( "URL template to source, space delimit ' file ' ' branch ' and capture name" )]
		public string Provider1SourceLinkTemplate
		{
			get { return SourceLinkTemplate; }
			set { SourceLinkTemplate = value; }
		}

		[Category( SourceLink + Options1PageName )]
		[DisplayName( "Default Branch" )]
		[Description( "Default branch (main, master, develop...)" )]
		public string Provider1DefaultBranch
		{ 
			get { return DefaultBranch; }
			set { DefaultBranch = value; }
		}

		[Category( SourceLink + Options1PageName )]
		[DisplayName( "Use Default Branch" )]
		[Description( "Use default instead of current branch" )]
		[DefaultValue(false)]
		public bool Provider1UseDefaultBranch 
		{
			get { return UseDefaultBranch; }
			set { UseDefaultBranch = value; }
		}

		[Category( SourceLink + Options1PageName )]
		[DisplayName( "Bookmark Format" )]
		[Description( "Supported bookmarks 1  1,2,3... 1-3 1,3" )]
		[DefaultValue( BookmarkTypeEnum.All )]
		[TypeConverter( typeof( EnumConverter ) )]
		public BookmarkTypeEnum Provider1BookmarksType 
		{
			get { return BookmarkType; }
			set { BookmarkType = value; }
		}

#if false
		protected override IEnumerable<PropertyInfo> GetOptionProperties()
		{
			var optionProperties = base.GetOptionProperties().ToList();

			return ForceSettingDictionaryFirst( optionProperties );
		}

		private IEnumerable<PropertyInfo> ForceSettingDictionaryFirst
		(
			List<PropertyInfo> baseProps
		)
		{
			var orderedSettings = baseProps
				.OrderBy
				(
					p =>
					{
						switch( p.Name )
						{
							case nameof( Provider1Enabled ):
								return 0;
							case nameof( Provider1OriginMatch ):
								return 1;
							case nameof( Provider1OriginRegex ):
								return 2;
							case nameof( Provider1SourceLinkTemplate ):
								return 3;
							case nameof( Provider1BookmarksType ):
								return 4;
							case nameof( Provider1UseDefaultBranch ):
								return 5;
						}
						return 6;
					}
				);
			return orderedSettings;
		}
#endif

		[Conditional( "DEBUG" )]
		private void Log( string message )
		{
			if( _debug )
			{
				Debug.WriteLine( $"{LogPrefix}{message}" );
			}
		}
	}
}
