using Community.VisualStudio.Toolkit;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SharedSrc.Interfaces;
using static SQLPaste.Resources.Constants;
using System.Text;

namespace SQLPaste
{
	internal partial class OptionsProvider
	{
		// See attributes in package class
		[ComVisible( true )]
		public class ExtensionOptionsProv : BaseOptionPage<ExtensionOptions> { }
	}

	public class ExtensionOptions : BaseOptionModel<ExtensionOptions>, IExtensionOptions
	{
		[Category( PasteSpecial + " Options" )]
		[DisplayName( PasteAs + " CSV Horizontal" )]
		[Description( "Controls if Paste As CSV emits linfeeds" )]
		[DefaultValue( true )]
		public bool CSVPasteDirection { get; set; } = true;

		[Category( PasteSpecial + " Options" )]
		[DisplayName( PasteAs + " Values Horizontal" )]
		[Description( "Controls if Paste As Values emits linfeeds" )]
		[DefaultValue( true )]
		public bool ValuesPasteDirection { get; set; } = true;

		[Category( PasteSpecial + " Options" )]
		[DisplayName( "Always Quote Integers" )]
		[Description( "Controls if Paste forces '' around integers" )]
		[DefaultValue( false )]
		public bool AlwaysQuoteIntegers { get; set; } = false;


		[Category( PasteSpecial + " Logging" )]
		[DisplayName( "Diagnostic Output" )]
		[Description( "Enables Output Pane diagnostics" )]
		[DefaultValue( false )]
		public bool DiagnosticOutput { get; set; } = false;


		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append( $"CSV {Direction( CSVPasteDirection )}" );
			sb.Append( $" VALUES {Direction( ValuesPasteDirection )}" );
			sb.Append( $" Quote Integers {YesNo( AlwaysQuoteIntegers )}" );
			sb.Append( $" Diagnostics {YesNo(DiagnosticOutput)}" );
			return sb.ToString();
		}

		private string Direction( bool b )
		{
			return b ? "Horizontal" : "Vertical";
		}

		private string YesNo( bool b )
		{
			return b ? "Yes" : "No";
		}
	}
}
