using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using NUnit.Framework;
using Gicc;

namespace Gicc.Test
{
	[TestFixture]
	public class GiccTest : GiccTestBase
	{
		[Test]
		public void WriteAndParseConfigTest()
		{
			Environment.CurrentDirectory = REPO_PATH;

			// write configs
			new Gicc(REPO_PATH, CC_TEST_PATH, BRANCH_NAME, REPO_PATH).WriteConfig();

			// parse configs
			Gicc gicc = new Gicc(Environment.CurrentDirectory, true);

			Assert.AreEqual(CC_TEST_PATH, gicc.VobPath);
			Assert.AreEqual(BRANCH_NAME, gicc.BranchName);
			Assert.AreEqual(REPO_PATH, gicc.RepoPath);
		}

		[Test]
		public void CopyAndCommitTest()
		{
			CreateGitTestMockUp();
			// setup

			Gicc gicc = new Gicc(REPO_PATH, CC_TEST_PATH, BRANCH_NAME, REPO_PATH);
			Git git = new Git(GitInfo);

			gicc.CopyAndCommit(CCMockupHistoryList, git.LastGiccPull, DateTime.Now);
			throw new NotImplementedException();
		}

		[Test]
		public void GetCommitPointsTest()
		{
			DateTime commitPoint = new DateTime(2015, 4, 1, 1, 0, 0);

			List<DateTime> expected = new List<DateTime>();
			List<DateTime> actual = new Gicc(REPO_PATH, false).GetCommitPoints(CCMockupHistoryList);

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

		[TestCase(@"test.aspx@@/main/5", true)]
		[TestCase(@"test.ascx@@/main/5", true)]
		[TestCase(@"test.js@@/main/5", true)]
		[TestCase(@"test.sql@@/main/5", true)]
		[TestCase(@"test.cs@@/main/5", false)]
		[TestCase(@"test.aspx", true)]
		[TestCase(@"test.ascx", true)]
		[TestCase(@"test.js", true)]
		[TestCase(@"test.sql", true)]
		[TestCase(@"test.cs", false)]
		public void IsTargetExtensionTest(string path, bool result)
		{
			Gicc gicc = new Gicc(REPO_PATH, false);

			Assert.AreEqual(result, gicc.IsLabelingTargetExtension(path));
		}
	}
}
