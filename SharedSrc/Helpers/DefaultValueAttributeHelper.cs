using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SharedSrc.Helpers
{
	public static class DefaultValueAttributeHelper
	{
		// The [DefaultValue( )] attribute is not supported by BaseOptionModel<T>
		// To assure it is in sync and not have duplicate defaults, set by
		// reflection here
		public static void InitializeDefaultProperties(object objWithProperties, string last = "")
		{
			var propertyInfos = objWithProperties.GetType().GetProperties();
			foreach( var propertyInfo in propertyInfos.Where( p => p.Name != last) )
			{
				SetDefaultValue( propertyInfo, objWithProperties );
			}
			if( last.Length == 0 )
			{
				return;
			}
			var lastPropInfo = propertyInfos.FirstOrDefault( p => p.Name == last );
			if( lastPropInfo == null )
			{
				return;
			}
			SetDefaultValue( lastPropInfo, objWithProperties );
		}

		private static void SetDefaultValue( PropertyInfo pi, object o )
		{
			var d = pi.GetCustomAttribute<DefaultValueAttribute>();
			if( d != null )
			{
				pi.SetValue( o, d.Value );
			}
		}
	}
}
