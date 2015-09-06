using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Configuration;

using NUnit.Framework;
using Gicc.Lib;

namespace Gicc.Test
{
	[TestFixture]
	public class GitTest : GiccTestBase
	{
    [Test]
    public void GetLastPushOrPullTest()
    {
      string lastPushOrPull = new Git(GitInfo).GetLastPP();
      Assert.AreEqual("6acda1797f37b0fae29839d6ccd0ecd1d48a18ae", lastPushOrPull);
    }

    [Test]
    public void GetCommittedFilesAfterLastPP()
    {
      Dictionary<string, FileChangeType> expectedFileList = new Dictionary<string, FileChangeType>()
      {
        {"a.txt", FileChangeType.Delete},
        {"b.txt", FileChangeType.Modification},
        {"c.txt", FileChangeType.Creation},
        {"d.txt", FileChangeType.Creation},
        {"e\\e.txt", FileChangeType.Creation}
      };
      Dictionary<string, FileChangeType> actualFileList = new Git(GitInfo).GetCommittedFilesAfterLastPP();

      Assert.That(actualFileList, Is.EquivalentTo(expectedFileList));
    }

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
			// setup
			CreateGitTestMockUp();
			// setup

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
			// setup

			List<string> expected = new string[] { "untracked1", "untracked2" }.ToList();
			List<string> actual = new Git(GitMockupInfo).GetUntrackedFileList();

			Assert.That(actual, Is.EquivalentTo(expected));

			// cleanup
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
			// setup

			List<string> expected = new string[] { "tt.txt" }.ToList();
			List<string> actual = new Git(GitMockupInfo).GetModifiedFileList();

			Assert.That(actual, Is.EquivalentTo(expected));

			// cleanup
			File.WriteAllText(TT_PATH, stored);
		}
	}
}
