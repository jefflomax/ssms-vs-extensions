using System;
using System.Collections.Generic;
using NUnit.Framework;
using SourceControlDLShared2.Helpers;

namespace UnitTests
{
	public class ProviderTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void TranslateUrlGithub()
		{
			// From App.Config
			var originRegex = @"^.*//github\.com/(?<profile>[\w.]*)/(?<repo>[\w-.]*)\.git";
			// from .git/config [remote "origin"]
			var originUrl = @"https://github.com/jefflomax/ssms-vs-extensions.git";

			var template = @"https://github.com/ profile / repo /blob/ branch / file";
			var branch = "main";
			var filePathFragment = @"SourceControlDeepLinksVS2022/VSCommandTable.cs";
			var lines = "";

			var captures = ProviderHelper.ResolveRegex( originRegex, originUrl );

			Assert.AreEqual( 2, captures.Count );
			var expectedCaptures = new Dictionary<string, string>
			{
				[ "profile" ] = "jefflomax",
				[ "repo" ] = "ssms-vs-extensions"
			};
			Assert.That( captures, Is.EqualTo( expectedCaptures ) );

			var translatedUrl = ProviderHelper.TranslateUrl
			(
				template,
				branch,
				filePathFragment,
				captures
			);

			var expected = @"https://github.com/jefflomax/ssms-vs-extensions/blob/main/SourceControlDeepLinksVS2022/VSCommandTable.cs";

			Assert.AreEqual( expected, translatedUrl );
		}

		[Test]
		public void TranslateUrlBitbucketServer()
		{
			// From App.Config
			var originRegex = @"^.*//code\.(?<domain>[\w.]*)/scm/(?<project>[\w]*)/(?<repo>[\w.]*)\.git";
			// from .git/config [remote "origin"]
			var originUrl = @"https://code.yourdomain.com/scm/TC/yourdomain.yourrepo.git";

			var template = @"https://code. domain /scm/ project /repos/ repo /browse/ file";
			var branch = "";
			var filePathFragment = @"TestFolder/Test.cs";
			var lines = "";

			var captures = ProviderHelper.ResolveRegex( originRegex, originUrl );

			Assert.AreEqual( 3, captures.Count );
			var expectedCaptures = new Dictionary<string, string>
			{
				[ "domain" ] = "yourdomain.com",
				[ "project" ] = "TC",
				[ "repo" ] = "yourdomain.yourrepo"
			};
			Assert.That( captures, Is.EqualTo( expectedCaptures ) );

			var translatedUrl = ProviderHelper.TranslateUrl
			(
				template,
				branch,
				filePathFragment,
				captures
			);

			var expected = @"https://code.yourdomain.com/scm/TC/repos/yourdomain.yourrepo/browse/TestFolder/Test.cs";

			Assert.AreEqual( expected, translatedUrl );

		}
	}
}
