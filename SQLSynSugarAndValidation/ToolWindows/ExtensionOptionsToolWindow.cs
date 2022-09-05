using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace SQLSynSugarAndValidation.ToolWindows
{
	public class ExtensionOptionsToolWindow
		: BaseToolWindow<ExtensionOptionsToolWindow>
	{
		public override string GetTitle(int toolWindowId)
			=> "VS 2019/SSMS 18 Extension © 2022 Jeff Lomax";

		public override Type PaneType => typeof( Pane );

		public override Task<FrameworkElement> CreateAsync
		(
			int toolWindowId,
			CancellationToken cancellationToken
		)
		{
			var state = new ExtensionOptionsState();
			state.LispExpressionTree = 
				@"{\rtf1\ansi (\b Testing\b0 1 2 (\b Add\b0 1 + 2)) }";

			var package = GetPackage();

			var packageState = package.GetOptionsState();
			//state.CSVPasteDirection = packageState.CSVPasteDirection;
			//state.ValuesPasteDirection = packageState.ValuesPasteDirection;

			ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
			{
				await package.ToOutputPaneAsync( $"CreateAsync State {packageState}" );
			} );

			var control = new ExtensionOptionsToolWindowControl();
			control.DataContext = state;
			package.SetToolWindowControl( control );

			return Task.FromResult<FrameworkElement>
			(
				control
			);
		}

		private SQLSynSugarAndValidationPackage GetPackage()
		{
			return Package as SQLSynSugarAndValidationPackage;
		}

		[Guid( "d4fc0c0d-4345-4a5c-a532-59adb4e0a189" )]
		internal class Pane : ToolWindowPane 
		{
			public Pane()
			{
				BitmapImageMoniker = KnownMonikers.StatusInformation;
			}
		}
	}
}
