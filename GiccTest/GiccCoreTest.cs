using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using NUnit.Framework;
using Gicc.Lib;

namespace Gicc.Test
{
	[TestFixture]
	public class GiccCoreTest : GiccTestBase
	{
    [Test]
    public void PushTest()
    {
      new GiccCore(REPO_PATH, CC_TEST_PATH, BRANCH_NAME, REPO_PATH).Push();
    }
		
		[Test]
		public void WriteAndParseConfigTest()
		{
			Environment.CurrentDirectory = REPO_PATH;

			// write configs
      new GiccCore(REPO_PATH, CC_TEST_PATH, BRANCH_NAME, REPO_PATH).WriteConfig();

			// parse configs
			GiccCore gicc = new GiccCore(Environment.CurrentDirectory);

			Assert.AreEqual(CC_TEST_PATH, gicc.VobPath);
			Assert.AreEqual(BRANCH_NAME, gicc.BranchName);
			Assert.AreEqual(REPO_PATH, gicc.RepoPath);
		}

    [Test]
    public void MakeRelativeTest()
    {
      GiccCore gicc = new GiccCore(REPO_PATH, CC_TEST_PATH, BRANCH_NAME, REPO_PATH);
      string path = Path.Combine(REPO_PATH, "test");
      Assert.AreEqual("test", gicc.MakeRelative(path));
      
      path = Path.Combine(CC_TEST_PATH, "test");
      Assert.AreEqual("test", gicc.MakeRelative(path));

      path = "test";
      Assert.AreEqual("test", gicc.MakeRelative(path));
    }

		[Test]
		public void CopyAndCommitTest()
		{
			CreateGitTestMockUp();
			// setup

			GiccCore gicc = new GiccCore(REPO_PATH, CC_TEST_PATH, BRANCH_NAME, REPO_PATH);
			Git git = new Git(GitInfo);

			gicc.CopyAndCommit(CCMockupHistoryList, git.GetLastGiccPullDate(), DateTime.Now);
			throw new NotImplementedException();
		}

		[Test]
		public void GetCommitPointsTest()
		{
			DateTime commitPoint = new DateTime(2015, 4, 1, 1, 0, 0);

			List<DateTime> expected = new List<DateTime>();
			List<DateTime> actual = new GiccCore(REPO_PATH).GetCommitPoints(CCMockupHistoryList);

			expected.Add(commitPoint.AddMinutes(0));
			expected.Add(commitPoint.AddMinutes(2));
			expected.Add(commitPoint.AddMinutes(4));
			expected.Add(commitPoint.AddMinutes(5));
			expected.Add(commitPoint.AddMinutes(8));
			expected.Add(commitPoint.AddMinutes(9));
			expected.Add(commitPoint.AddMinutes(11));
			expected.Add(commitPoint.AddMinutes(12));
			expected.Add(commitPoint.AddMinutes(13));

			Assert.AreEqual(expected, actual);
		}
	}
}
