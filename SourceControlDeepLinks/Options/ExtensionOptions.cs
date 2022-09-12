using Community.VisualStudio.Toolkit;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SharedSrc.Interfaces;
using static SourceControlDeepLinks.Resources.Constants;
using System.Text;
using SourceControlDeepLinks.Options;
using System.Collections.Generic;
using SourceControlDeepLinks.Helpers;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace SourceControlDeepLinks
{
	// getting apply button
	// https://docs.microsoft.com/en-us/visualstudio/extensibility/creating-an-options-page?view=vs-2022
	internal partial class OptionsProvider
	{
		// See attributes in package class
		[ComVisible( true )]
		public class ExtensionOptionsProv : BaseOptionPage<ExtensionOptions> { }

	}

	// TODO: Update Settings on Save

	public class ExtensionOptions
		: BaseOptionModel<ExtensionOptions>, IExtensionOptions
	{
		// Consider migrating to package
		private static int _priorProvider;
		private static SourceControlDeepLinksPackage _package;
		private readonly string _sdl = "SourceControlDeepLinks ";
		private readonly bool _debug;

		// The .Instance property in this class is meant to be used
		// on the UI thread only

		public ExtensionOptions()
		{
			Settings = new Dictionary<string, ProviderInfo>();
			SetFormToProviderFields(null);
			_debug = true;
		}

		public void SetPackage( ToolkitPackage package )
		{
			_package = (SourceControlDeepLinksPackage)package;
			Log( "Setting package on Extension Options" );
		}

		private void SetFormToProviderFields(ProviderInfo pi)
		{
			if( pi == null )
			{
				ProviderDomain = string.Empty;
				ProviderBaseUrl = string.Empty;
				ProviderProjectPrefix = string.Empty;
				ProviderDefaultBranch = string.Empty;
				ProviderUseDefaultBranch = false;
			}
			else
			{
				ProviderDomain = pi.Domain;
				ProviderBaseUrl = pi.BaseUrl;
				ProviderProjectPrefix = pi.ProjectPrefix;
				ProviderDefaultBranch = pi.DefaultBranch;
				ProviderUseDefaultBranch = pi.UseDefaultBranch;
			}
		}

		// 'ForceSettingDictionaryFirst' reorders so this hydrates first
		// TODO: Consider TypeConverter instead
		private Dictionary<string, ProviderInfo> _settings;
		[Browsable( false )]
		public Dictionary<string, ProviderInfo> Settings
		{
			get { return _settings; }
			set { _settings = value; }
		}

		[Category( SourceLink + " Options" )]
		[DisplayName( "Diagnostic Options" )]
		[Description( "Extra logging to output pane" )]
		[DefaultValue(false)]
		public bool DiagnosticOutput { get; set; }

		[Category(SourceLink + " Options")]
		[DisplayName("Git Executable")]
		[Description("Fully qualified path to git.exe")]
		[DefaultValue( @"C:\Program Files\Git\cmd\git.exe" )]
		public string GitExecutable { get; set; }

		[Category(SourceLink + " Options")]
		[DisplayName("Bypass Git")]
		[Description("Recommended - manually process .git to bypass git \"trust developers\"")]
		[DefaultValue(true)]
		public bool BypassGit { get; set; }

		[Category(SourceLink + " Options")]
		[DisplayName("Format")]
		[Description("Format for Slack or other markdown link")]
		[DefaultValue(false)]
		public bool Format { get; set; }

		[Category(SourceLink + " Options")]
		[DisplayName("Format String")]
		[Description("{0} url {1} path {2} file {3} extension")]
		[DefaultValue( "<{0}|{2}{3}>" )]
		public string FormatString { get; set; }

		private SourceProvider _sourceProvider;
		[Category( SourceLink + " Provider" )]
		[DisplayName( "Provider" )]
		[Description( "Source Provider" )]
		[DefaultValue( SourceProvider.BitbucketServer )]
		[TypeConverter( typeof( EnumConverter ) )]
		public SourceProvider Provider
		{
			get
			{
				return _sourceProvider;
			}
			set
			{
				var iValue = (int)value;
				var key = value.ToString();

				Log
				(
					$"Provider '{key}' Prior {_priorProvider} "+
					$"Settings {KeysAsList<string,ProviderInfo>(Settings.Keys)}"
				);
				if( iValue != 0 )
				{
					if( iValue != _priorProvider )
					{
						if( _priorProvider != 0 )
						{
							// Save prior provider config
							var prior = (SourceProvider)_priorProvider;
							Log( $"Save {prior.ToString()}" );

							var priorKey = prior.ToString();
							if( Settings.TryGetValue( priorKey, out var priorEntry ) )
							{
								Log( $"BeforeUpdate {priorKey} {priorEntry.Serialize( priorKey )}" );
								SetProviderInfoFromForm( priorEntry );
								Log( $"AfterUpdate {priorKey} {priorEntry.Serialize( priorKey )}" );
							}
							else
							{
								var newEntry = GetProviderInfoFromForm();
								Log( $"Add NEW {priorKey} {newEntry.Serialize( priorKey )}" );
								Settings.Add( priorKey, newEntry );
							}
						}

						if( Settings.TryGetValue( key, out var entry ) )
						{
							Log( $"LOAD {key} {entry.Serialize( key )}" );
							SetFormToProviderFields( entry );
						}
						else
						{
							var providerDefaults = GetProviderDefaultsFromAppConfig( value );
							Log( $"LOADNEW {key} {providerDefaults.Serialize( key )}" );

							SetFormToProviderFields( providerDefaults );
						}
						_priorProvider = iValue;
					}
					_sourceProvider = value;
				}
			}
		}

		private ProviderInfo GetProviderDefaultsFromAppConfig
		(
			SourceProvider currentProvider
		)
		{
			var providerHelper = new ProviderHelper( _package );
			return providerHelper.GetProviderDefaults( currentProvider );
		}

		public ProviderInfo GetProviderInfoFromForm()
		{
			var providerInfo = new ProviderInfo();
			return SetProviderInfoFromForm( providerInfo );
		}
		private ProviderInfo SetProviderInfoFromForm
		(
			ProviderInfo providerInfo
		)
		{
			providerInfo.Set
			(
				ProviderDomain,
				ProviderBaseUrl,
				ProviderProjectPrefix,
				ProviderDefaultBranch,
				ProviderUseDefaultBranch
			);

			return providerInfo;
		}

		[ Category( SourceLink + " Provider" )]
		[DisplayName( "Domain" )]
		[Description( "The domain name for the source url" )]
		public string ProviderDomain { get; set; }

		[Category( SourceLink + " Provider" )]
		[DisplayName( "Project Prefix" )]
		[Description( "Used to locate the project" )]
		public string ProviderProjectPrefix { get; set; }

		[ Category( SourceLink + " Provider" )]
		[DisplayName( "Base Url" )]
		[Description( "Source Url start '{0}' is domain" )]
		public string ProviderBaseUrl { get; set; }

		[Category( SourceLink + " Provider" )]
		[DisplayName( "Default Branch" )]
		[Description( "Default branch (main, master, develop...)" )]
		public string ProviderDefaultBranch { get; set; }

		[Category( SourceLink + " Provider" )]
		[DisplayName( "Use Default Branch" )]
		[Description( "Use default instead of current branch" )]
		[DefaultValue(false)]
		public bool ProviderUseDefaultBranch { get; set; }

		[Category("Output")]
		[DisplayName("Output Pane")]
		[Description("View, Output, select \"show output from\": " + ExtensionOutputPane)]
		public bool OutputToPane { get; set; } = true;

		[Category("Output")]
		[DisplayName("Clipboard")]
		[Description("Place URL in clipboard")]
		public bool OutputToClipboard { get; set; } = true;


		public override string ToString()
		{
			var sb = new StringBuilder();
			if( BypassGit )
			{
				sb.Append("Bypass GIT exe enabled");
			}
			else
			{
				sb.Append("GIT {GitExecutable}");
			}

			if( OutputToClipboard || OutputToPane )
			{
				sb.Append("; Output:");
				if (OutputToClipboard)
				{
					sb.Append(" Clipboard");
				}
				if (OutputToPane)
				{
					sb.Append(" OutputWindowPane");
				}
			}

			if( Format )
			{
				sb.Append($"; Format: {FormatString}");
			}

			sb.Append( $"; Provider: {Provider}" );

			return sb.ToString();
			
		}

		/* We can't auto-serialize a dictionary ??? */
		protected override object DeserializeValue
		(
			string serializedData,
			System.Type type,
			string propertyName
		)
		{
			Log( $"DeserializeValue {propertyName}" );

			if( propertyName != nameof( Settings ) )
			{
				return base.DeserializeValue( serializedData, type, propertyName );
			}

			var settings = new Dictionary<string, ProviderInfo>();
			if( string.IsNullOrEmpty( serializedData ) )
			{
				return (object)settings;
			}

			Log( $"Deserialize {serializedData}" );

			using( var sr = new StringReader( serializedData ) )
			{
				string line = sr.ReadLine();
				while( line != null )
				{
					var endOfKey = line.IndexOf( ';' );
					if( endOfKey != -1 )
					{
						var key = line.Substring( 0, endOfKey );
						if( KeyInSourceProvider( key ) )
						{
							var serialized = line.Substring( endOfKey + 1 );
							var providerInfo = ProviderInfo.Deserialize
							(
								serialized
							);

							Log
							(
								$"Add {key} {providerInfo.Serialize( key )}"
							);

							settings.Add( key, providerInfo );
						}
						else
						{
							Log( $"{_sdl} Key {key} not found" );
						}
					}
					line = sr.ReadLine();
				}
			}

			return (object)settings;
		}

		protected override string SerializeValue
		(
			object value,
			System.Type type,
			string propertyName
		)
		{
			Log( $"SerializeValue {propertyName}" );

			if( propertyName != nameof( Settings ) )
			{
				return base.SerializeValue( value, type, propertyName );
			}

			var sb = new StringBuilder();
			foreach(var key in Settings
				.Keys
				.Where( k => KeyInSourceProvider(k) ) )
			{
				var providerInfo = Settings[ key ];
				sb.AppendLine( providerInfo.Serialize( key ) );
			}
			var s = sb.ToString();
			Log( $"serialize {s}" );
			return s;
		}

		protected override IEnumerable<PropertyInfo> GetOptionProperties()
		{
			Log( "GetOptionProperties" );

			var optionProperties = base.GetOptionProperties().ToList();

			return ForceSettingDictionaryFirst( optionProperties );
		}

		private IEnumerable<PropertyInfo> ForceSettingDictionaryFirst
		(
			List<PropertyInfo> baseProps
		)
		{
			var orderedSettings = baseProps
				.OrderBy
				(
					p => (p.Name == nameof( Settings )) ? 0 : 1
				);
			return orderedSettings;
		}

		private string KeysAsList<K,V>
		(
			Dictionary<K,V>.KeyCollection keys
		)
		{
			var keyNames = string.Join( ",", keys.Select( n => n ) );
			return keyNames;
		}

		/// <summary>
		/// Help remove wrong/no longer supported serialized valies
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private bool KeyInSourceProvider( string key )
		{
			var isInEnum =
				key == SourceProvider.BitbucketServer.ToString() ||
				key == SourceProvider.GitHub.ToString();
			return isInEnum;
		}

		[Conditional( "DEBUG" )]
		private void Log( string message )
		{
			if( _debug )
			{
				Debug.WriteLine( $"{_sdl}{message}" );
			}
		}
	}
}
