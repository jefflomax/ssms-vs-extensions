using System;
using System.Collections.Generic;
using System.Text;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Task = System.Threading.Tasks.Task;
using SharedSrc.Commands;
using SQLSynSugarAndValidation;
using SQLSynSugarAndValidation.ToolWindows;
using SharedSrc.Interfaces;

namespace SQLSynSugarAndValidation.Commands
{
	[Command( PackageIds.ToolWindowCommand )]
	internal sealed class ToolWindowCommand
		: CommandShared<ToolWindowCommand, SQLSynSugarAndValidationPackage, ExtensionOptions>,
			ICommandShared<SQLSynSugarAndValidationPackage>
	{
		protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			return ExtensionOptionsToolWindow.ShowAsync();
		}
	}
}
