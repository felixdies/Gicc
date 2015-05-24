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
		protected string VOB_PATH = ConfigurationManager.AppSettings["VobPath"];
		protected string CC_TEST_PATH = ConfigurationManager.AppSettings["CCTestPath"];
		protected string CC_SYMBOLIC_LINK_PATH = ConfigurationManager.AppSettings["CCSymbolicLinkPath"];
		protected string BRANCH_NAME = ConfigurationManager.AppSettings["BranchName"];
		protected string REPO_PATH = ConfigurationManager.AppSettings["RepoPath"];

		public GitTest()
		{
			CreateGitTestMockUp();
		}

		public void CreateGitTestMockUp()
		{
			string MOCKUP_PATH = Path.Combine(Directory.GetParent(REPO_PATH).ToString(), "gicctest_mockup");
			
			if (Directory.Exists(MOCKUP_PATH))
				return;

			string cachedCWD = Environment.CurrentDirectory;

			Git git = new Git(MOCKUP_PATH);
			string nl = Environment.NewLine;
			string tt = "tt.txt";
			string subtt = "sub\\subtt.txt";
			string authorA = "A <A@A.A>";
			string authorB = "B <B@B.B>";
			DateTime commitTime = new DateTime(2015, 05, 01, 01, 0, 0);

			Directory.CreateDirectory(Path.Combine(MOCKUP_PATH, @".git/gicc"));

			Environment.CurrentDirectory = MOCKUP_PATH;
			SetConfig(CC_TEST_PATH, BRANCH_NAME, MOCKUP_PATH);

			git.Execute("init");
			File.WriteAllText(tt, string.Empty);
			Directory.CreateDirectory("sub");
			File.WriteAllText(subtt, string.Empty);
			git.AddCommit("first commit", "First <F@F.F>", commitTime.ToString());

			git.Checkout(BRANCH_NAME);
			File.WriteAllText(tt, "A1" + nl);
			File.WriteAllText(subtt, "A2" + nl);
			git.AddCommit("A1", authorA, commitTime.AddMinutes(1).ToString());

			git.Checkout("master");
			File.WriteAllText(tt, "B3" + nl);
			File.WriteAllText(subtt, "B4" + nl);
			git.AddCommit("B2", authorB, commitTime.AddMinutes(2).ToString());

			git.Checkout("master");
			File.WriteAllText(subtt, "A5" + nl);
			git.AddCommit("A3", authorA, commitTime.AddMinutes(3).ToString());

			git.Checkout(BRANCH_NAME);
			File.WriteAllText(tt, "A6" + nl);
			File.WriteAllText(subtt, "A7" + nl);
			File.WriteAllText(tt, "A8" + nl); // write again(cc history)
			git.AddCommit("A4", authorA, commitTime.AddMinutes(4).ToString());

			git.Checkout(BRANCH_NAME);
			File.WriteAllText(subtt, "B9" + nl);
			git.AddCommit("B5", authorB, commitTime.AddMinutes(5).ToString());

			git.Checkout("master");
			File.WriteAllText(tt, "A10" + nl);
			File.WriteAllText(subtt, "A11" + nl);
			git.AddCommit("A6", authorA, commitTime.AddMinutes(6).ToString());

			git.Checkout("master");
			File.Delete(tt);
			git.AddCommit("A7", authorA, commitTime.AddMinutes(7).ToString());

			git.Checkout("master");
			File.Delete(subtt);
			git.AddCommit("B8", authorB, commitTime.AddMinutes(8).ToString());

			Environment.CurrentDirectory = cachedCWD;
		}

		[Test]
		public void HelpTest()
		{
			string expectedStart = "usage: git ";
			string actual = new Git(REPO_PATH).Help();

			Assert.True(actual.StartsWith(expectedStart));
		}

		[Test]
		public void GetUntrackedFileTest()
		{
			List<string> expected = new List<string>(new string[] { "b", "c" });
			List<string> actual = new Git(REPO_PATH).GetUntrackedFileList();

			Assert.That(actual, Is.EquivalentTo(expected));
		}

		[Test]
		public void Diff()
		{
			List<string> expected = new List<string>(new string[] {
				"a"});
			List<string> actual = new Git(REPO_PATH).GetModifiedFileList();

			Assert.That(actual, Is.EquivalentTo(expected));
		}

		[TearDown]
		public void CleanUp()
		{
			//Environment.CurrentDirectory = Directory.GetDirectoryRoot(REPO_PATH);
			//FileEx.DeleteIfExists(Path.Combine(REPO_PATH));
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
		public void IsGitIgnoredTest(string path, bool result)
		{
			Assert.AreEqual(new Git(REPO_PATH).IsIgnored(path), result);
		}
	}
}
