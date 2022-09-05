using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLSynSugarAndValidation.ToolWindows;

namespace SQLSynSugarAndValidation.Helpers
{
	public class ValidationDisplayUtils
	{
		public static void ValidationTree
		(
			antlr.Validation.Validations validations,
			ObservableCollection<ValidationNode> tree
		)
		{
			tree.Clear();

			AddCategory( tree, validations.Errors, "Error" );
			AddCategory( tree, validations.Warnings, "Warning" );
			AddCategory( tree, validations.Infos, "Information" );
		}

		private static void AddCategory
		(
			ObservableCollection<ValidationNode> tree,
			List<antlr.Validation.Validation> category,
			string categoryName
		)
		{
			if( ! category.Any() )
			{
				return;
			}

			var node = new ValidationNode( categoryName );
			tree.Add( node );
			foreach( var v in category )
			{
				var type = v.Type;
				var dataNode = new ValidationNode( $"{type} {v.Message}" );
				node.Data.Add( dataNode );
			}
		}
	}
}
