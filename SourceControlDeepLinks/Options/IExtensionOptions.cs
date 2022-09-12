namespace SharedSrc.Interfaces
{
	public interface IExtensionOptions : IBaseExtensionOptions
	{
		bool DiagnosticOutput { get; set; }
		string GitExecutable { get; set; }
		bool BypassGit { get; set; }
		bool Format { get; set; }
		string FormatString { get; set; }
		bool OutputToPane { get; set; }
		bool OutputToClipboard { get; set; }
	}
}
