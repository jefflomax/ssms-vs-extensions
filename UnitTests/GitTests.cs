using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SourceControlDeepLinks.Helpers;

namespace UnitTests
{
	public class GitTests
	{
		// Local file system dependent tests

		[Test, Explicit]
		public async Task GetRepositoryRootAsync()
		{
			var gh = new GitHelper( "", bypassGit: true );
			var repo = @"C:\programs\vs2019\VSExt\VSExtEssential\VSExtEssential\Options\";
			var git = await gh.GetRepositoryRootAsync( repo );
			Assert.NotNull( git );
			Assert.AreEqual( @"C:\programs\vs2019\VSExt\VSExtEssential", git );
		}


		[Test, Explicit]
		public async Task GetRemoteOriginUrlAsync()
		{
			var gh = new GitHelper( "", bypassGit: true );
			var repo = @"C:\programs\vs2019\VSExt\VSExtEssential\VSExtEssential\Options\";
			var remote = await gh.GetRemoteOriginUrlAsync( repo );
			Assert.AreEqual( @"https://github.com/jefflomax/VSExtEssential.git", remote );
		}
	}
}
