using Community.VisualStudio.Toolkit;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SharedSrc.Interfaces;
using static SourceControlDeepLinks.Resources.Constants;
using System.Text;
using SourceControlDeepLinks.Options;

namespace SourceControlDeepLinks
{
	internal partial class OptionsProvider
	{
		// See attributes in package class
		[ComVisible( true )]
		public class ExtensionOptionsProv : BaseOptionPage<ExtensionOptions> { }
	}

	public class ExtensionOptions : BaseOptionModel<ExtensionOptions>, IExtensionOptions
	{
		public bool DiagnosticOutput { get; set; } = false;

		[Category(BitBucketLink + " Options")]
		[DisplayName("Git Executable")]
		[Description("Fully qualified path to git.exe")]
		public string GitExecutable { get; set; } = @"C:\Program Files\Git\cmd\git.exe";

		[Category(BitBucketLink + " Options")]
		[DisplayName("Bypass Git")]
		[Description("Manually retrieve git root and remote url")]
		public bool BypassGit { get; set; } = true;

		[Category(BitBucketLink + " Options")]
		[DisplayName("Format")]
		[Description("Format for Slack or other markdown link")]
		public bool Format { get; set; } = false;

		[Category(BitBucketLink + " Options")]
		[DisplayName("Format String")]
		[Description("{0} url {1} path {2} file {3} extension")]
		public string FormatString { get; set; } = "<{0}|{2}{3}>";

		[Category( BitBucketLink + " Options" )]
		[DisplayName( "Provider" )]
		[Description( "Source Provider" )]
		[DefaultValue( SourceProvider.BitbucketServer )]
		[TypeConverter( typeof( EnumConverter ) )]
		public SourceProvider Provider { get; set; } = SourceProvider.BitbucketServer;


		[ Category("Output")]
		[DisplayName("Output Pane")]
		[Description("View, Output, select \"show output from\": " + ExtensionOutputPane)]
		public bool OutputToPane { get; set; } = true;

		[Category("Output")]
		[DisplayName("Clipboard")]
		[Description("Place URL in clipboard")]
		public bool OutputToClipboard { get; set; } = true;

		public override string ToString() {
			var sb = new StringBuilder();
			if( BypassGit)
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

			sb.Append( $"; Provider: {Provider}" );

			return sb.ToString();
			
		}
}
}
