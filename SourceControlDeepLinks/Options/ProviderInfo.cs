using System;

namespace SourceControlDeepLinks.Options
{
	[Serializable]
	public class ProviderInfo
	{
		public ProviderInfo()
		{
			Domain = string.Empty;
			BaseUrl = string.Empty;
			ProjectPrefix = string.Empty;
			DefaultBranch = string.Empty;
			UseDefaultBranch = false;
		}

		public void Set
		(
			string domain,
			string baseUrl,
			string prefix,
			string defaultBranch,
			bool useDefaultBranch
		)
		{
			Domain = domain;
			BaseUrl = baseUrl;
			ProjectPrefix = prefix;
			DefaultBranch = defaultBranch;
			UseDefaultBranch = useDefaultBranch;
		}
		public void Set( ProviderInfo from )
		{
			Domain = from.Domain;
			BaseUrl = from.BaseUrl;
			ProjectPrefix = from.ProjectPrefix;
			DefaultBranch = from.DefaultBranch;
			UseDefaultBranch = from.UseDefaultBranch;
		}


		public string Domain { get; private set; }
		public string BaseUrl { get; private set; }
		public string ProjectPrefix { get; private set; }
		public string DefaultBranch { get; private set; }
		public bool UseDefaultBranch { get; private set; }

		public string Serialize(string key)
		{
			return $"{key};{Domain},{BaseUrl},{ProjectPrefix},{DefaultBranch},{UseDefaultBranch}";
		}
		public static ProviderInfo Deserialize( string s )
		{
			var pi = new ProviderInfo();
			var fields = s.Split( new char[]{','}, StringSplitOptions.None );
			var l = fields.Length;
			if( l > 0 )
			{
				pi.Domain = fields[ 0 ];
			}
			if( l > 1 )
			{
				pi.BaseUrl = fields[ 1 ];
			}
			if( l > 2 )
			{
				pi.ProjectPrefix = fields[ 2 ];
			}
			if( l > 3 )
			{
				pi.DefaultBranch = fields[ 3 ];
			}
			if( l > 4 )
			{
				Boolean.TryParse( fields[ 4 ], out var b );
				pi.UseDefaultBranch = b;
			}
			return pi;
		}
	}
}
