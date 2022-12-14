using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.StringComparison;
using SharedSrc.Helpers;

namespace SourceControlDeepLinks.Helpers
{
	public class GitHelper
	{
		private readonly string _pathToGit;
		private readonly bool _bypassGit;
		private DirectoryInfo _gitFolder;
		public GitHelper(string pathToGit, bool bypassGit)
		{
			_pathToGit = pathToGit;
			_bypassGit = bypassGit;
			_gitFolder = null;
		}

		public async Task<string> GetRepositoryRootAsync
		(
			string workingDirectory
		)
		{
			if (!_bypassGit)
			{
				var arguments = "rev-parse --git-dir";
				var repoRoot = await RunGitCommandAsync( arguments, workingDirectory );
				return (repoRoot == ".git")
					? workingDirectory + '\\'
					: repoRoot.Substring( 0, repoRoot.Length - 4 );
			}

			await Task.CompletedTask;
			var gitRoot = GetGitFolder( workingDirectory );
			return (gitRoot?.Extension == ".git")
				? gitRoot.Parent.FullName
				: string.Empty;
		}

		public async Task<string> GetRemoteOriginUrlAsync(string workingDirectory)
		{
			if(!_bypassGit)
			{
				var arguments = "config --get remote.origin.url";
				return await RunGitCommandAsync( arguments, workingDirectory );
			}

			var gitRoot = GetGitFolder( workingDirectory );
			if( gitRoot == null )
			{
				return string.Empty;
			}
			var gitConfig = $@"{gitRoot.FullName}\config";
			using (var tr = File.OpenText( gitConfig ))
			{
				var findRemote = true;
				var readUrl = false;
				var line = await tr.ReadLineAsync();
				var regEx = new Regex( @"(?<remote>http.*\.git)" );
				while( line != null )
				{
					if( findRemote )
					{
						// [remote "origin"]
						if (line[0] == '[')
						{
							if (line.Length > 6 && line.Substring( 1, 6 ).Equals( "remote", CurrentCultureIgnoreCase ))
							{
								findRemote = false;
								readUrl = true;
							}
						}
					}

					if( readUrl )
					{
						// 	url = https://github.com/jefflomax/VSExtEssential.git
						var match = regEx.Match( line );
						if (match.Success)
						{
							var cap = regEx.GroupNumberFromName("remote");
							return match.Groups[cap].Value;
						}
					}

					line = await tr.ReadLineAsync();
				}
			}
			return string.Empty;
		}

		public async Task<string> GetCurrentBranchAsync
		(
			string workingDirectory,
			string defaultBranch = "main"
		)
		{
			var gitRoot = GetGitFolder( workingDirectory );

			// TODO: Git Command version

			// ref: refs/heads/master
			var branch = defaultBranch;
			var gitHead = $@"{gitRoot.FullName}\HEAD";
			using( var tr = File.OpenText( gitHead ) )
			{
				var line = await tr.ReadLineAsync();
				while( line != null )
				{
					if( line.Length < 2 )
					{
						continue;
					}
					var refIndex = line.IndexOf( "ref:", CurrentCultureIgnoreCase );
					if( refIndex != -1 )
					{
						var lastSlash = line.LastIndexOf( '/' );
						branch = line.Substring( lastSlash + 1 );
						return branch;
					}
					line = await tr.ReadLineAsync();
				}
			}
			await Task.CompletedTask;
			return branch;
		}


		private async Task<string> RunGitCommandAsync
		(
			string arguments,
			string workingDirectory
		)
		{
			var standardOutput = await ProcessHelper.RunProcessAsync
			(
				_pathToGit,
				arguments,
				workingDirectory
			);

			return standardOutput;
		}

		private DirectoryInfo GetGitFolder( string workingDirectory )
		{
			if( _gitFolder != null )
			{
				return _gitFolder;
			}

			var di = new DirectoryInfo( workingDirectory );
			while( di != null )
			{
				var gitDir = di.GetDirectories( ".git", SearchOption.TopDirectoryOnly );
				if ( gitDir.Length > 0 )
				{
					_gitFolder = gitDir[ 0 ];
					return _gitFolder;
				}
				di = di.Parent;
			}
			return null;
		}
	}
}

