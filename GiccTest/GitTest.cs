using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Configuration;

using NUnit.Framework;
using Gicc;

namespace Gicc.Test
{
	[TestFixture]
	public class GitTest : GiccTestBase
	{
		[Test]
		public void InitTest()
		{
			FileEx.DeleteIfExists(Path.Combine(REPO_PATH, ".git"));

			string expectedStart = "Initialized empty Git repository ";
			string actual = new Git(GitInfo).Init();

			Assert.True(actual.StartsWith(expectedStart));
		}

		[Test]
		public void HelpTest()
		{
			string expectedStart = "usage: git ";
			string actual = new Git(GitInfo).Help();

			Assert.True(actual.StartsWith(expectedStart));
		}

		[Test]
		public void LastGiccPullTest()
		{
			//setup
			CreateGitTestMockUp();
			//setup

			Assert.AreEqual(new DateTime(2015, 5, 1, 1, 5, 0), new Git(GitMockupInfo).LastGiccPull);
		}

		[Test]
		public void GetUntrackedFileTest()
		{
			List<string> expected = new List<string>(new string[] { "b", "c" });
			List<string> actual = new Git(GitInfo).UntrackedFileList;

			Assert.That(actual, Is.EquivalentTo(expected));
		}

		[Test]
		public void Diff()
		{
			List<string> expected = new List<string>(new string[] { "a" });
			List<string> actual = new Git(GitInfo).ModifiedFileList;

			Assert.That(actual, Is.EquivalentTo(expected));
		}

		[TestCase(@".builds", true)]
		[TestCase(@"0.suo", true)]
		[TestCase(@"0.sln.docstates", true)]
		[TestCase(@"0.ide\0", true)]
		[TestCase(@"0.sln.ide\0", true)]
		[TestCase(@"debug\0", true)]
		[TestCase(@"Debug\0", true)]
		[TestCase(@"x86\0", true)]
		[TestCase(@"testresult\0", true)]
		[TestCase(@"testresult1\0", true)]
		[TestCase(@"testResult\0", true)]
		[TestCase(@"Testresult1\0", true)]
		[TestCase(@"file", false)]
		[TestCase(@"dir\0", false)]
		public void IsGitIgnoredTest(string path, bool result)
		{
			//setup
			new Git(GitInfo).Init();
			File.WriteAllText(Path.Combine(REPO_PATH, ".gitignore"), ResourceHandler.GetResource("gitignore.txt"));
			//setup

			Assert.AreEqual(new Git(GitInfo).IsIgnored(path), result);
		}
	}
}
