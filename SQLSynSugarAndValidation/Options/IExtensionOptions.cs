namespace SharedSrc.Interfaces
{
	public interface IExtensionOptions : IBaseExtensionOptions
	{
		bool DiagnosticOutput { get; }
		bool ShowParsed { get; }
	}
}
