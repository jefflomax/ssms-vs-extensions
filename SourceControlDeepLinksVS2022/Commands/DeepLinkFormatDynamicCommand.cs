using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
//using Microsoft.VisualStudio.Shell;
//using Task = System.Threading.Tasks.Task;


namespace SourceControlDeepLinks.Commands
{
	[Command( PackageIds.DeepLinkDynamicCommand )]
	internal sealed class DeepLinkFormatDynamicCommand
		: BaseDynamicCommand<DeepLinkFormatDynamicCommand, string>
	{
		// CommandShared hard coded to BaseCommand, so not used here
		protected override void BeforeQueryStatus
		(
			OleMenuCommand menuItem,
			EventArgs e,
			string item
		)
		{
			var package = GetPackage;
			var state = package.GetOptionsState();
			var format = state.Format;
			var hasFormatString = state.FormatString.Length > 0;

			menuItem.Checked = format;
			menuItem.Enabled = hasFormatString;
		}

		protected override async Task ExecuteAsync
		(
			OleMenuCmdEventArgs e,
			string command
		)
		{
			var package = GetPackage;
			var state = package.GetOptionsState();
			if( state.DiagnosticOutput )
			{
				await package.ToOutputPaneAsync
				(
					$"Changing Format state from {state.Format} to {!state.Format}"
				);
			}
			state.Format = !state.Format;
		}

		protected override IReadOnlyList<string> GetItems()
		{
			return new List<string> { "Deep Link Formatted" };
		}

		private SourceControlDeepLinksVS2022Package GetPackage =>
			Package as SourceControlDeepLinksVS2022Package;
	}
}
