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

namespace SourceControlDeepLinks
{
	internal partial class OptionsProvider2
	{
		// See attributes in package class
		[ComVisible( true )]
		public class Provider2OptionsProv : BaseOptionPage<Provider2Options> 
		{
		}
	}


	public class Provider2Options : ProviderBase<Provider2Options>
	{
		private readonly bool _debug;
		private static bool _initChecked = false;

		// The .Instance property in this class is meant to be used
		// on the UI thread only

		public Provider2Options()
		{
			_debug = true;
			if( !_initChecked )
			{
				InitFromAppConfig( '2', typeof( Provider2Options ), nameof( Provider2Enabled ) );
				_initChecked = true;
			}
		}

		[Category( SourceLink + OptionsInfo )]
		[DisplayName( "Enabled" )]
		[Description( "Enable/Disable provider" )]
		[DefaultValue( true )]
		public bool Provider2Enabled { get { return Enabled; } set { Enabled = value; } }

		[Category( SourceLink + OptionsInfo )]
		[DisplayName( "Friendly Name" )]
		[Description( "Provider Name (optional)" )]
		public string ProviderFriendlyName { get { return FriendlyName; } set { FriendlyName = value; } }

		[Category( SourceLink + Options2PageName )]
		[DisplayName( "Origin Match" )]
		[Description( "Determines if this provider is used" )]
		public string Provider2OriginMatch { get { return OriginMatch; } set { OriginMatch = value; } }

		[Category( SourceLink + Options2PageName )]
		[DisplayName( "Origin Regex" )]
		[Description( "Extract named captures like domain, profile, repo, from origin URL to insert in Source Link Template" )]
		public string Provider2OriginRegex { get { return OriginRegex; } set { OriginRegex = value; } }

		[Category( SourceLink + Options2PageName )]
		[DisplayName( "Source Link Template" )]
		[Description( "URL template to source, space delimit ' file ' ' branch ' and capture name" )]
		public string Provider2SourceLinkTemplate
		{
			get { return SourceLinkTemplate; }
			set { SourceLinkTemplate = value; }
		}

		[Category( SourceLink + Options2PageName )]
		[DisplayName( "Default Branch" )]
		[Description( "Default branch (main, master, develop...)" )]
		public string Provider2DefaultBranch
		{
			get { return DefaultBranch; }
			set { DefaultBranch = value; }
		}

		[Category( SourceLink + Options2PageName )]
		[DisplayName( "Use Default Branch" )]
		[Description( "Use default instead of current branch" )]
		[DefaultValue(false)]
		public bool Provider2UseDefaultBranch
		{
			get { return UseDefaultBranch; }
			set { UseDefaultBranch = value; }
		}

		[Category( SourceLink + Options2PageName )]
		[DisplayName( "Bookmark Format" )]
		[Description( "Supported bookmarks 1  1,2,3... 1-3 1,3" )]
		[DefaultValue( BookmarkTypeEnum.All )]
		[TypeConverter( typeof( EnumConverter ) )]
		public BookmarkTypeEnum Provider2BookmarksType
		{
			get { return BookmarkType; }
			set { BookmarkType = value; }
		}

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
