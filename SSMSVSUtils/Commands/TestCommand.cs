using System;
using System.Collections.Generic;
using System.Text;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel.Design;
using SharedSrc.Commands;
using SharedSrc.Interfaces;

namespace SSMSVSUtils.Commands
{
	[Command( PackageIds.SomeTestCommand )]
	public class TestCommand
		: CommandShared<TestCommand, SSMSVSUtilsPackage, ExtensionOptions>,
			ICommandShared<SSMSVSUtilsPackage>
	{
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var package = GetPackage;
			await package.ToOutputPaneAsync( "Test Command" );
		}
	}
}
