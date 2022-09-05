using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SharedSrc.Helpers
{
	public enum SSMSIntellisense { NotFound, Enabled, Disabled }
	public static class RegistryHelper
	{
		//public const string SSMSOptionsX = @"SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell\SqlEditor";
		public const string PrivateSettingsCollectionName = "ApplicationPrivateSettings";
		public const string SqlEditorCollectionName = "SqlEditor";
		public const string RadLangSvcSqlLang = @"RadLangSvc\SqlLanguagePreferences";
		public const string SSMSOptions = @"SOFTWARE\Microsoft\SQL Server Management Studio\18.0_IsoShell\ApplicationPrivateSettings\RadLangSvc\SqlLanguagePreferences";
		public const string IntellisenseEnabled = "EnableIntellisense";
		//public const string EnableIntellisense = "EnableIntellisense";

		public static SSMSIntellisense GetStringBooleanValue(string path)
		{
			var registryKey = GetHKCUSubKey(SSMSOptions);
			var value = registryKey.GetValue( path );

			return ProcessRegistryString( value );
		}

		public static SSMSIntellisense Toggle(SSMSIntellisense current)
		{
			return (current == SSMSIntellisense.Enabled)
				? SSMSIntellisense.Disabled
				: SSMSIntellisense.Enabled;
		}

		public static bool ToBoolean(SSMSIntellisense current)
		{
			return current == SSMSIntellisense.Enabled;
		}

		public static SSMSIntellisense ProcessRegistryString( object registryValue )
		{
			var str = registryValue as string;
			if( str == null )
			{
				return SSMSIntellisense.NotFound;
			}
			int lastSplat = str.LastIndexOf( '*' );
			if( lastSplat == -1)
			{
				return SSMSIntellisense.NotFound;
			}
			ReadOnlySpan<char> s = str.AsSpan();
			var trueFalse = s.Slice( lastSplat + 1 );

			if( SpanEquals( trueFalse, Boolean.TrueString ))
			{
				return SSMSIntellisense.Enabled;
			}
			else if (SpanEquals( trueFalse, Boolean.FalseString ))
			{
				return SSMSIntellisense.Disabled;
			}

			return SSMSIntellisense.NotFound;
		}

		private static bool SpanEquals( ReadOnlySpan<char> value, string s )
		{
			return value.CompareTo( s.AsSpan(), StringComparison.CurrentCultureIgnoreCase ) == 0;
		}

		public static void SetStringValueFromBool(string path, SSMSIntellisense ssmsIntellisense)
		{
			var intellisenseSetting = Serialize( ssmsIntellisense );

			var hive = Registry.CurrentUser;
			var openRegistryKeyForWrite = hive.CreateSubKey( SSMSOptions );
			try
			{
				openRegistryKeyForWrite.SetValue( path, intellisenseSetting );
			}
			catch( Exception e )
			{
				var message = e.Message;
				System.Diagnostics.Debug.WriteLine( $"ERROR {message}" );
			}

		}

		public static string Serialize(SSMSIntellisense ssmsIntellisense)
		{
			// 1*System.Boolean*True
			// 1*System.Boolean*False

			var value = ToBoolean( ssmsIntellisense );

			var valueStr = value.ToString();
			var intellisenseSetting = $"1*System.Boolean*{valueStr}";
			return intellisenseSetting;
		}

#if false
		private static void Phooey(string k, string v)
		{
			var user = Environment.UserDomainName + "\\" + Environment.UserName;
			RegistryAccessRule rule = new RegistryAccessRule
			(
				user,
				RegistryRights.FullControl,
				AccessControlType.Allow
			);
			RegistrySecurity security = new RegistrySecurity();
			security.AddAccessRule( rule );
			var key = Registry.Users.OpenSubKey
			(
				SSMSOptions,
				RegistryKeyPermissionCheck.ReadWriteSubTree,
				RegistryRights.FullControl
			);
			key.SetAccessControl( security );
			try
			{
				key.SetValue( k, v );
			}
			catch( Exception e)
			{
				var message = e.Message;
				System.Diagnostics.Debug.WriteLine( $"ERROR {message}" );
			}
		}
#endif

		private static RegistryKey GetHKCUSubKey(string subKey)
		{
			var hive = Registry.CurrentUser;
			var registryKey = hive.OpenSubKey( subKey );
			return registryKey;
		}
	}
}
