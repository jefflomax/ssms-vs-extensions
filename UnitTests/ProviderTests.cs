using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SourceControlDLSharedNoDep.Helpers;

namespace UnitTests
{
	public class ProviderTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void TranslateUrlGit()
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
				["profile"] = "jefflomax",
				["repo"] = "ssms-vs-extensions"
			};
			Assert.That(captures, Is.EqualTo(expectedCaptures));

			var translatedUrl = ProviderHelper.TranslateUrl
			(
				template,
				branch,
				filePathFragment,
				lines,
				captures
			);

			var expected = @"https://github.com/jefflomax/ssms-vs-extensions/blob/main/SourceControlDeepLinksVS2022/VSCommandTable.cs";

			Assert.AreEqual( expected, translatedUrl );
		}
	}
}
