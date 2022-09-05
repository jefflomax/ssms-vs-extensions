using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLSynSugarAndValidation.ToolWindows
{
	public class ValidationNode
	{
		public ValidationNode( string name )
		{
			Name = name;
			Data = new ObservableCollection<ValidationNode>();
		}
		public string Name { get; set; }
		public ObservableCollection<ValidationNode> Data { get; set; }
	}
}
