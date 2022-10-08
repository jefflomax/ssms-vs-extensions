using Community.VisualStudio.Toolkit;
using System.Windows;
using System.Windows.Controls;

namespace SQLSynSugarAndValidation.ToolWindows
{
	public partial class ExtensionOptionsToolWindowControl : UserControl
	{
		public ExtensionOptionsToolWindowControl()
		{
			//DataContext = state;
			InitializeComponent();
			Init();
		}

		private void Init()
		{
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Init();
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			VS.MessageBox.Show( "Button clicked" );
		}
	}
}