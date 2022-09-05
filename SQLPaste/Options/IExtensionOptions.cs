namespace SharedSrc.Interfaces
{
	public interface IExtensionOptions
	{
		bool DiagnosticOutput { get; set; }
		bool CSVPasteDirection { get; set; }
		public bool ValuesPasteDirection { get; set; }
		public bool AlwaysQuoteIntegers { get; set; }
	}
}
