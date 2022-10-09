namespace SharedSrc.Interfaces
{
	public interface IExtensionOptions : IBaseExtensionOptions
	{
		bool DiagnosticOutput { get; set; }
		bool CSVPasteDirection { get; set; }
		bool ValuesPasteDirection { get; set; }
		bool AlwaysQuoteIntegers { get; set; }
	}
}
