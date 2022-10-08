using Community.VisualStudio.Toolkit;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SharedSrc.Interfaces;
using static SQLSynSugarAndValidation.Resources.Constants;

namespace SQLSynSugarAndValidation
{
	internal partial class OptionsProvider
	{
		// See attributes in package class
		[ComVisible( true )]
		public class ExtensionOptionsProv : BaseOptionPage<ExtensionOptions> { }
	}

	public class ExtensionOptions : BaseOptionModel<ExtensionOptions>, IExtensionOptions
	{
		private static SQLSynSugarAndValidationPackage _package;

		[Category( OptionsPageCategoryName + " Logging" )]
		[DisplayName( "Diagnostic Output" )]
		[Description( "Enables Output Pane diagnostics" )]
		[DefaultValue( false )]
		public bool DiagnosticOutput { get; set; } = false;


		[Category( OptionsPageCategoryName + " Options" )]
		[DisplayName( "Show Parsed" )]
		[Description( "Show the parsed input query" )]
		[DefaultValue( true )]
		public bool ShowParsed { get; set; } = true;

		[Category( OptionsPageCategoryName + " Options" )]
		[DisplayName( "Schema Path" )]
		[Description( "Fully qualified path to serialized schema" )]
		public string SchemaFolder { get; set; }

		public void SetPackage( ToolkitPackage package )
		{
			_package = (SQLSynSugarAndValidationPackage)package;
		}
		public override string ToString()
		{
			return $"Diag {DiagnosticOutput}";
		}

	}
}
