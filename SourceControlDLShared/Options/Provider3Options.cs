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
	internal partial class OptionsProvider3
	{
		// See attributes in package class
		[ComVisible( true )]
		public class Provider3OptionsProv : BaseOptionPage<Provider3Options> 
		{
		}
	}

	public class Provider3Options : ProviderBase<Provider3Options>
	{
		private readonly bool _debug;

		// The .Instance property in this class is meant to be used
		// on the UI thread only

		public Provider3Options()
		{
			_debug = true;
			DefaultValueAttributeHelper.InitializeDefaultProperties( this );
		}

		[Category( SourceLink + OptionsInfo )]
		[DisplayName( PEnabled )]
		[Description( PEnabledDesc )]
		[DefaultValue( false )]
		public bool Provider3Enabled { get { return Enabled; } set { Enabled = value; } }

		[Category( SourceLink + OptionsInfo )]
		[DisplayName( PFriendly )]
		[Description( PFriendlyDesc )]
		public string ProviderFriendlyName { get { return FriendlyName; } set { FriendlyName = value; } }

		[Category( SourceLink + Options3PageName )]
		[DisplayName( POriginMatch )]
		[Description( POriginMatchDesc )]
		public string Provider3OriginMatch { get { return OriginMatch; } set { OriginMatch = value; } }

		[Category( SourceLink + Options3PageName )]
		[DisplayName( POriginRegex )]
		[Description( POriginRegexDesc )]
		public string Provider3OriginRegex { get { return OriginRegex; } set { OriginRegex = value; } }

		[Category( SourceLink + Options3PageName )]
		[DisplayName( PSourceLinkTemplate )]
		[Description( PSourceLinkTemplateDesc )]
		public string Provider3SourceLinkTemplate
		{
			get { return SourceLinkTemplate; }
			set { SourceLinkTemplate = value; }
		}

		[Category( SourceLink + Options3PageName )]
		[DisplayName( PDefaultBranch )]
		[Description( PDefaultBranchDesc )]
		public string Provider3DefaultBranch
		{
			get { return DefaultBranch; }
			set { DefaultBranch = value; }
		}

		[Category( SourceLink + Options3PageName )]
		[DisplayName( PUseDefaultBranch )]
		[Description( PUseDefaultBranchDesc )]
		[DefaultValue(false)]
		public bool Provider3UseDefaultBranch
		{
			get { return UseDefaultBranch; }
			set { UseDefaultBranch = value; }
		}

		[Category( SourceLink + Options3PageName )]
		[DisplayName( PBookmarkType )]
		[Description( PBookmarkTypeDesc )]
		[DefaultValue( BookmarkTypeEnum.All )]
		[TypeConverter( typeof( EnumConverter ) )]
		public BookmarkTypeEnum Provider3BookmarksType
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
