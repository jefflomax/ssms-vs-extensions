using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;

namespace SharedSrc.Interfaces
{
	public interface ICommandShared<P>
	{
		P GetPackage { get; }
		Task OutputToExtensionWindowPaneAsync( string message );
		Task<(bool, DocumentView docView)> GetActiveDocumentViewAsync();
		// S GetOptions()
	}
}
