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

	// GENERAL Options page

	internal partial class OptionsProvider
	{
		// See attributes in package class
		[ComVisible( true )]
		public class ExtensionOptionsProv : BaseOptionPage<ExtensionOptions> 
		{
		}
	}

	public class ExtensionOptions
		: BaseOptionModel<ExtensionOptions>, IExtensionOptions
	{
		private readonly bool _debug;

		// The .Instance property in this class is meant to be used
		// on the UI thread only

		public ExtensionOptions()
		{
			DefaultValueAttributeHelper.InitializeDefaultProperties( this );

			_debug = true;
		}


		[Category( SourceLink + OptionsName )]
		[DisplayName( "Diagnostic Options" )]
		[Description( "Extra logging to output pane" )]
		[DefaultValue(false)]
		public bool DiagnosticOutput { get; set; }

		[Category( SourceLink + OptionsName )]
		[DisplayName("Git Executable")]
		[Description("Fully qualified path to git.exe")]
		[DefaultValue( @"C:\Program Files\Git\cmd\git.exe" )]
		public string GitExecutable { get; set; }

		[Category( SourceLink + OptionsName )]
		[DisplayName("Bypass Git")]
		[Description("Recommended - manually locate .git folder and parse HEAD and config to bypass git \"trust developers\"")]
		[DefaultValue(true)]
		public bool BypassGit { get; set; }

		[Category( SourceLink + OptionsName )]
		[DisplayName("Format")]
		[Description("Format for Slack or other markdown link")]
		[DefaultValue(false)]
		public bool Format { get; set; }

		[Category( SourceLink + OptionsName )]
		[DisplayName("Format String")]
		[Description("{0} url {1} path {2} file {3} extension")]
		[DefaultValue( "<{0}|{2}{3}>" )]
		public string FormatString { get; set; }
		[Category("Output")]
		[DisplayName("Output Pane")]
		[Description("View, Output, select \"show output from\": " + ExtensionOutputPane)]
		public bool OutputToPane { get; set; } = true;

		[Category("Output")]
		[DisplayName("Clipboard")]
		[Description("Place URL in clipboard")]
		public bool OutputToClipboard { get; set; } = true;

		[Category( "Project Details" )]
		[DisplayName( SourceLink + "©2022 Jeff Lomax" )]
		[Description( "Source Control Deep Links is open source with the Apache License 2.0" )]
		public string ProjectInfo { get; } = "https://github.com/jefflomax/ssms-vs-extensions/tree/main/SourceControlDeepLinksVS2022";


		public override string ToString()
		{
			var sb = new StringBuilder();
			if( BypassGit )
			{
				sb.Append("Bypass GIT exe enabled");
			}
			else
			{
				sb.Append("GIT {GitExecutable}");
			}

			if( OutputToClipboard || OutputToPane )
			{
				sb.Append("; Output:");
				if (OutputToClipboard)
				{
					sb.Append(" Clipboard");
				}
				if (OutputToPane)
				{
					sb.Append(" OutputWindowPane");
				}
			}

			if( Format )
			{
				sb.Append($"; Format: {FormatString}");
			}

			return sb.ToString();	
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
