using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using SharedSrc.Interfaces;

namespace SharedSrc.Commands
{
	public class CommandShared<C, P, O> : BaseCommand<C>
		where C : class, ICommandShared<P>, new()
		where P : BasePackage<O>, IBasePackage<O>
		where O : class, IExtensionOptions, new()
	{
		public async Task<(bool, DocumentView docView)> GetActiveDocumentViewAsync()
		{
			var docView = await VS.Documents.GetActiveDocumentViewAsync();

			return (docView?.TextView != null, docView);
		}

		public O GetOptions()
		{
			return GetPackage.GetOptionsState();
		}

		public async Task OutputToExtensionWindowPaneAsync
		(
			string message
		)
		{
			await GetPackage.ToOutputPaneAsync( message );
		}

		public P GetPackage => Package as P;

	}
}
