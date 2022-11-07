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
using SourceControlDLShared2.Options;

namespace SourceControlDeepLinks.Options
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
		//private static bool _initChecked = false;

		// The .Instance property in this class is meant to be used
		// on the UI thread only

		public Provider2Options()
		{
			_debug = true;

			DefaultValueAttributeHelper.InitializeDefaultProperties( this );

			//if( !_initChecked )
			//{
				InitFromAppConfig( '2', typeof( Provider2Options ), nameof( Provider2Enabled ) );
			//	_initChecked = true;
			//}
		}

		[Category( SourceLink + OptionsInfo )]
		[DisplayName( PEnabled )]
		[Description( PEnabledDesc )]
		[DefaultValue( true )]
		public bool Provider2Enabled { get { return Enabled; } set { Enabled = value; } }

		[Category( SourceLink + OptionsInfo )]
		[DisplayName( PFriendly )]
		[Description( PFriendlyDesc )]
		public string ProviderFriendlyName { get { return FriendlyName; } set { FriendlyName = value; } }

		[Category( SourceLink + Options2PageName )]
		[DisplayName( POriginMatch )]
		[Description( POriginMatchDesc )]
		public string Provider2OriginMatch { get { return OriginMatch; } set { OriginMatch = value; } }

		[Category( SourceLink + Options2PageName )]
		[DisplayName( POriginRegex )]
		[Description( POriginRegexDesc )]
		public string Provider2OriginRegex { get { return OriginRegex; } set { OriginRegex = value; } }

		[Category( SourceLink + Options2PageName )]
		[DisplayName( PSourceLinkTemplate )]
		[Description( PSourceLinkTemplateDesc )]
		public string Provider2SourceLinkTemplate
		{
			get { return SourceLinkTemplate; }
			set { SourceLinkTemplate = value; }
		}

		[Category( SourceLink + Options2PageName )]
		[DisplayName( PDefaultBranch )]
		[Description( PDefaultBranchDesc )]
		public string Provider2DefaultBranch
		{
			get { return DefaultBranch; }
			set { DefaultBranch = value; }
		}

		[Category( SourceLink + Options2PageName )]
		[DisplayName( PUseDefaultBranch )]
		[Description( PUseDefaultBranchDesc )]
		[DefaultValue(false)]
		public bool Provider2UseDefaultBranch
		{
			get { return UseDefaultBranch; }
			set { UseDefaultBranch = value; }
		}

		[Category( SourceLink + Options2PageName )]
		[DisplayName( PBookmarkType )]
		[Description( PBookmarkTypeDesc )]
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
