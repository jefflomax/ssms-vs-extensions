using System.ComponentModel;
using System.Runtime.InteropServices;
using SharedSrc.Interfaces;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using SharedSrc.Helpers;
using Community.VisualStudio.Toolkit;
using SourceControlDeepLinks.Helpers;
using SourceControlDeepLinks.Options;
using static SourceControlDeepLinks.Resources.Constants;
using Microsoft.VisualStudio.Shell;
using SourceControlDLShared.Options;

namespace SourceControlDeepLinks
{
	// getting apply button
	// https://docs.microsoft.com/en-us/visualstudio/extensibility/creating-an-options-page?view=vs-2022

	internal partial class OptionsProvider
	{
		// See attributes in package class
		[ComVisible( true )]
		public class ExtensionOptionsProv : BaseOptionPage<ExtensionOptions> 
		{
		}

	}

	// TODO: Update Settings on Save

	public class ExtensionOptions
		: BaseOptionModel<ExtensionOptions>, IExtensionOptions
	{
		// Consider migrating to package
		private static int _priorProvider = 0;
		private readonly bool _debug;

		// The .Instance property in this class is meant to be used
		// on the UI thread only

		public ExtensionOptions()
		{
			Settings = new Dictionary<string, ProviderInfo>();
			SetFormToProviderFields(null); // Clear

			var settingsHelper = new SettingsHelper( typeof( ExtensionOptions ) );
			var propertyInSettings = nameof( ExtensionOptions.Provider );

			_ = ThreadHelper.JoinableTaskFactory.RunAsync( async () =>
			{
				// This is more than icky
				// The prior provider (Bitbucket, Github) is stored in a static,
				// special code reads hidden dictionary property "Settings", which has several
				// fields that change when the Provider is changed.
				// But before any VS Settings have been saved, we read defaults from
				// App.Config and DefaultValue properties.
				// As this can be called many times before settings are persisted,
				// if no "Provider" is in VS Settings, reset the priorProvider so it always
				// resets, otherwise the fields will be empty

				var propertyExists = await settingsHelper.PropertyExistsAsync( propertyInSettings );
				if( !propertyExists )
				{
					_priorProvider = 0;
					// DefaultValue attribute appears to ONLY work on boolean types
					DefaultValueAttributeHelper.InitializeDefaultProperties
					(
						this,
						nameof( Provider ) // assure this one initialized LAST
					);
				}
			} );

			_debug = true;
		}


		/// <summary>
		/// As provider changges, restore a set of form fields
		/// </summary>
		/// <param name="pi"></param>
		private void SetFormToProviderFields(ProviderInfo pi)
		{
			if( pi == null )
			{
				ProviderOriginRegex = string.Empty;
				ProviderSourceLinkTemplate = string.Empty;
				ProviderDefaultBranch = string.Empty;
				ProviderUseDefaultBranch = false;
				ProviderBookmarksType = BookmarkTypeEnum.All;
			}
			else
			{
				ProviderOriginRegex = pi.OriginRegex;
				ProviderSourceLinkTemplate = pi.SourceLinkTemplate;
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
		[Description("Recommended - manually locate .git folder and parse HEAD and config to bypass git \"trust developers\"")]
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
		[DefaultValue( SourceProvider.BitbucketServer )] // Neither 1 or SourceProvider.BitbucketServer worked
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
			var providerHelper = new ProviderFactory();
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
				ProviderOriginRegex,
				ProviderSourceLinkTemplate,
				ProviderDefaultBranch,
				ProviderUseDefaultBranch,
				ProviderBookmarksType
			);

			return providerInfo;
		}

		[Category( SourceLink + " Provider" )]
		[DisplayName( "Origin Regex" )]
		[Description( "Extract named captures like domain, profile, repo, from origin URL to insert in Source Link Template" )]
		public string ProviderOriginRegex { get; set; }

		[Category( SourceLink + " Provider" )]
		[DisplayName( "Source Link Template" )]
		[Description( "URL template to source, space delimit ' file ' ' branch ' and capture name" )]
		public string ProviderSourceLinkTemplate { get; set; }

		[Category( SourceLink + " Provider" )]
		[DisplayName( "Default Branch" )]
		[Description( "Default branch (main, master, develop...)" )]
		public string ProviderDefaultBranch { get; set; }

		[Category( SourceLink + " Provider" )]
		[DisplayName( "Use Default Branch" )]
		[Description( "Use default instead of current branch" )]
		[DefaultValue(false)]
		public bool ProviderUseDefaultBranch { get; set; }

		[Category( SourceLink + " Provider" )]
		[DisplayName( "Bookmark Format" )]
		[Description( "Supported bookmarks 1  1,2,3... 1-3 1,3" )]
		[DefaultValue( BookmarkTypeEnum.All )]
		[TypeConverter( typeof( EnumConverter ) )]
		public BookmarkTypeEnum ProviderBookmarksType { get; set; }

		[ Category("Output")]
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
						var (key, version) = KeyAndVersion( line, endOfKey );

						if( KeyInSourceProvider( key ) )
						{
							var serialized = line.Substring( endOfKey + 1 );
							var providerInfo = ProviderInfo.Deserialize
							(
								version,
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
							Log( $"Key {key} not found" );
						}
					}
					line = sr.ReadLine();
				}
			}

			return (object)settings;
		}

		private static (string Key, char Version) KeyAndVersion( string line, int endOfKey )
		{
			var key = line.Substring( 0, endOfKey );
			var version = '0';
			if( key[ 0 ] == 'V' )
			{
				version = key[ 1 ];
				key = key.Substring( 2 );
			}

			return (key, version);
		}

		protected override string SerializeValue
		(
			object value,
			System.Type type,
			string propertyName
		)
		{
			if( propertyName != nameof( Settings ) )
			{
				return base.SerializeValue( value, type, propertyName );
			}

			Log( $"SerializeValue {propertyName}" );

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

		/// <summary>
		/// Force the hidden "Settings" property to read first, so it's
		/// dictionary is available when the Provider is set
		/// </summary>
		/// <returns></returns>
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
				Debug.WriteLine( $"{LogPrefix}{message}" );
			}
		}
	}
}
