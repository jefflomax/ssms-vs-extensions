using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLSynSugarAndValidation.Helpers;

namespace SQLSynSugarAndValidation.ToolWindows
{
	public class ExtensionOptionsState
	{
		private bool _debug;
		private bool _initialized;
		private ObservableCollection<ValidationNode> _items;
		private string _lisp;
		private string _rewritten;
		private string _original;

		public ExtensionOptionsState()
		{
			_debug = false;
			ShowLispParseTree = true;
			_original = string.Empty;
			_rewritten = string.Empty;
			_items = new ObservableCollection<ValidationNode>();
		}

		public void BuildValidationTree()
		{
			_items = new ObservableCollection<ValidationNode>();
		}

		public ExtensionOptionsState( ExtensionOptionsState state )
			: this
			(
				state,
				state.LispExpressionTree,
				state.Rewritten,
				state.Original
			)
		{
		}

		public ExtensionOptionsState
		(
			ExtensionOptionsState state,
			string lispExpressionTree,
			string rewritten,
			string original
		)
		{
			ShowLispParseTree = state.ShowLispParseTree;
			LispExpressionTree = lispExpressionTree;
			Rewritten = rewritten;
			Original = original;

			_items = state._items;
		}

		public bool ShowLispParseTree { get; set; }

		public ObservableCollection<ValidationNode> Items
		{
			get { return _items; }
		}

		public string LispExpressionTree
		{
			get { return _lisp; }
			set { _lisp = value; }
		}

		public string Original
		{
			get { return _original; }
			set { _original = value; }
		}

		public string Rewritten
		{
			get { return _rewritten; }
			set { _rewritten = value; }
		}

		public bool Initialized { get { return _initialized; } }

		public void FromOptions( ExtensionOptions options )
		{
		}

		private void Debug(string s )
		{
#if DEBUG
			if( _debug )
			{
				System.Diagnostics.Debug.WriteLine( s );
			}
#endif
		}

		public override string ToString()
		{
			return $"Init {_initialized}";
		}
	}
}
