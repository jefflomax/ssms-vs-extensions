using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell;
using SharedSrc.Interfaces;

namespace SharedSrc.Commands
{
	public class BasePackage<O> : ToolkitPackage
		where O: IExtensionOptions
	{
		protected static Guid _extensionOutputPane = Guid.Empty;
		protected static OutputWindowPane _outputWindowPane = null;
		protected O _state;
		protected string _extensionOutputPaneName;
		protected async Task<OutputWindowPane> SetupOutputPaneAsync
		(
			string outputPaneName
		)
		{
			if( _extensionOutputPane == Guid.Empty )
			{
				_outputWindowPane = await VS.Windows.CreateOutputWindowPaneAsync
				(
					outputPaneName,
					lazyCreate: false
				).ConfigureAwait( false );
				_extensionOutputPane = _outputWindowPane.Guid;
			}
			else if ( _outputWindowPane != null)
			{
				return _outputWindowPane;
			}
			{
				_outputWindowPane = await GetExtensionOutputWindowPaneAsync();
			}
			return _outputWindowPane;
		}

		private async Task<OutputWindowPane> GetExtensionOutputWindowPaneAsync()
		{
			if( _outputWindowPane != null )
			{
				return _outputWindowPane;
			}
			var outputWindowPane = await VS.Windows.GetOutputWindowPaneAsync
			(
				_extensionOutputPane
			);
			return outputWindowPane;
		}

		public async Task ToOutputPaneAsync
		(
			OutputWindowPane pane,
			string message
		)
		{
			if( pane == null )
			{
				return;
			}
			await pane?.WriteLineAsync( message );
		}

		public async Task ToOutputPaneAsync( bool show, string message )
		{
			if( show )
			{
				await ToOutputPaneAsync( message );
			}
		}

		public async Task ToOutputPaneAsync( string message )
		{
			var pane = await GetExtensionOutputWindowPaneAsync();
			await ToOutputPaneAsync( pane, message );
		}

		protected async Task GetOptionsFromVSSettingsAsync()
		{
			// read settings
			var options = await GetLiveSettingsInstanceAsync();
			_state = options;
		}

		protected async Task LogStartupInformationAsync
		(
			string name,
			string version
		)
		{
			if( _outputWindowPane == null || !_state.DiagnosticOutput )
			{
				return;
			}
			await _outputWindowPane
				.WriteLineAsync( $"{name} version {version} initialized {_state}" )
				.ConfigureAwait( false );
		}

		public virtual async Task<O> GetLiveSettingsInstanceAsync()
		{
			await Task.CompletedTask;
			return default(O);
		}

	}

}
