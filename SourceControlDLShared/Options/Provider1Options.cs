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
		//private bool _initChecked = false;

		// The .Instance property in this class is meant to be used
		// on the UI thread only

		public Provider1Options()
		{
			_debug = true;

			DefaultValueAttributeHelper.InitializeDefaultProperties( this );

			//if( !_initChecked )
			//{
				InitFromAppConfig( '1', typeof( Provider1Options ), nameof( Provider1Enabled ) );
			//	_initChecked = true;
			//}
		}


		[Category( SourceLink + OptionsInfo )]
		[DisplayName( PEnabled )]
		[Description( PEnabledDesc )]
		[DefaultValue( true )]
		public bool Provider1Enabled { get { return Enabled; } set { Enabled = value; } }

		[Category( SourceLink + OptionsInfo )]
		[DisplayName( PFriendly )]
		[Description( PFriendlyDesc )]
		public string ProviderFriendlyName { get { return FriendlyName; } set { FriendlyName = value; } }

		[Category( SourceLink + Options1PageName )]
		[DisplayName( POriginMatch )]
		[Description( POriginMatchDesc )]
		public string Provider1OriginMatch { get { return OriginMatch; } set { OriginMatch = value; } }

		[Category( SourceLink + Options1PageName )]
		[DisplayName( POriginRegex )]
		[Description( POriginRegexDesc )]
		public string Provider1OriginRegex { get { return OriginRegex; } set { OriginRegex = value; } }

		[Category( SourceLink + Options1PageName )]
		[DisplayName( PSourceLinkTemplate )]
		[Description( PSourceLinkTemplateDesc )]
		public string Provider1SourceLinkTemplate
		{
			get { return SourceLinkTemplate; }
			set { SourceLinkTemplate = value; }
		}

		[Category( SourceLink + Options1PageName )]
		[DisplayName( PDefaultBranch )]
		[Description( PDefaultBranchDesc )]
		public string Provider1DefaultBranch
		{ 
			get { return DefaultBranch; }
			set { DefaultBranch = value; }
		}

		[Category( SourceLink + Options1PageName )]
		[DisplayName( PUseDefaultBranch )]
		[Description( PUseDefaultBranchDesc )]
		[DefaultValue(false)]
		public bool Provider1UseDefaultBranch 
		{
			get { return UseDefaultBranch; }
			set { UseDefaultBranch = value; }
		}

		[Category( SourceLink + Options1PageName )]
		[DisplayName( PBookmarkType )]
		[Description( PBookmarkTypeDesc )]
		[DefaultValue( BookmarkTypeEnum.All )]
		[TypeConverter( typeof( EnumConverter ) )]
		public BookmarkTypeEnum Provider1BookmarksType 
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
