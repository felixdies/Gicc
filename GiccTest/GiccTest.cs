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

		/*
		[Test]
		public void CopyAndCommitTest()
		{
			Git git = new Git(REPO_PATH);

			CreateGitTestMockUp();
			new Gicc().CopyAndCommit(CCHistoryList, git.LastGiccPull, DateTime.Now);
			throw new NotImplementedException();
		}
		*/

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

		/*
    [Test]
    public void CheckAnyFileIsNotCheckedOutTest()
    {
      string checkoutFile = "main.txt";

      new Gicc().CheckCheckedoutFileIsNotExist();

      ClearCase.Checkout(checkoutFile);

      try
      {
        new Gicc().CheckCheckedoutFileIsNotExist();
      }
      catch (GiccException ex)
      {
        Console.WriteLine("success : caught checkedout file");
        Console.WriteLine(ex.Message);

        return;
      }
      finally
      {
        ClearCase.Uncheckout(checkoutFile);
      }

      Assert.Fail("could not catch checkout file : " + checkoutFile);
    }
		 */
	}
}
