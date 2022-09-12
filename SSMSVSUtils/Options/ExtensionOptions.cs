using Community.VisualStudio.Toolkit;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SharedSrc.Interfaces;
using static SSMSVSUtils.Resources.Constants;

namespace SSMSVSUtils
{
	internal partial class OptionsProvider
	{
		// See attributes in package class
		[ComVisible( true )]
		public class ExtensionOptionsProv : BaseOptionPage<ExtensionOptions> { }
	}

	public class ExtensionOptions : BaseOptionModel<ExtensionOptions>, IExtensionOptions
	{
		private SSMSVSUtilsPackage _package;

		[ Category( OptionsPageCategoryName + " Logging" )]
		[DisplayName( "Diagnostic Output" )]
		[Description( "Enables Output Pane diagnostics" )]
		[DefaultValue( false )]
		public bool DiagnosticOutput { get; set; } = false;

		public void SetPackage( ToolkitPackage package )
		{
			_package = (SSMSVSUtilsPackage)package;
		}
	}
}
