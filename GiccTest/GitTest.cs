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

			Git gitMockup = new Git(GitMockupInfo);
			gitMockup.TagPull();

			Assert.AreEqual(new DateTime(2015, 5, 1, 1, 8, 0), gitMockup.GetLastGiccPull());
		}

		[Test]
		public void UntrackedFileListTest()
		{
			CreateGitTestMockUp();
			File.Create(Path.Combine(REPO_MOCKUP_PATH, "untracked1")).Close();
			File.Create(Path.Combine(REPO_MOCKUP_PATH, "untracked2")).Close();
			//setup

			List<string> expected = new string[] { "untracked1", "untracked2" }.ToList();
			List<string> actual = new Git(GitMockupInfo).GetUntrackedFileList();

			Assert.That(actual, Is.EquivalentTo(expected));

			//cleanup
			File.Delete(Path.Combine(REPO_MOCKUP_PATH, "untracked1"));
			File.Delete(Path.Combine(REPO_MOCKUP_PATH, "untracked2"));
		}

		[Test]
		public void ModifiedFileListTest()
		{
			string TT_PATH = Path.Combine(REPO_MOCKUP_PATH, "tt.txt");
			
			CreateGitTestMockUp();
			string stored = File.ReadAllText(TT_PATH);
			File.WriteAllText(TT_PATH, "modified");
			//setup

			List<string> expected = new string[] { "tt.txt" }.ToList();
			List<string> actual = new Git(GitMockupInfo).GetModifiedFileList();

			Assert.That(actual, Is.EquivalentTo(expected));

			//cleanup
			File.WriteAllText(TT_PATH, stored);
		}

		[Test]
		public void IsIgnoredListTest()
		{
			//setup
			Git git = new Git(GitInfo);
			git.Init();
      File.WriteAllText(Path.Combine(REPO_PATH, ".gitignore"), Resource.GetResource("gitignore.txt"));
			//setup

			Dictionary<string, bool> ignoredList = new Dictionary<string, bool>()
			{
				{@".builds", true},
				{@"0.suo", true},
				{@"0.sln.docstates", true},
				{@"0.ide\0", true},
				{@"0.sln.ide\0", true},
				{@"debug\0", true},
				{@"Debug\0", true},
				{@"x86\0", true},
				{@"testresult\0", true},
				{@"testresult1\0", true},
				{@"testResult\0", true},
				{@"Testresult1\0", true},
				{@"file", false},
				{@"dir\0", false}
			};

			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

			Console.WriteLine("start IsGitIgnoredTest()");
			sw.Start();

			List<string> fileNameList = new List<string>();
			List<bool> isIgnoredList = new List<bool>();

			foreach (KeyValuePair<string, bool> pair in ignoredList)
			{
				fileNameList.Add(pair.Key);
				isIgnoredList.Add(pair.Value);
			}

			Assert.That(git.IsIgnoredList(fileNameList), Is.EquivalentTo(isIgnoredList));

			sw.Stop();
			Console.WriteLine("end IsGitIgnoredTest(). Elapsed time : " + sw.Elapsed);
		}
	}
}
