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
		public void CopyAndCommitTest()
		{
			Git git = new Git(REPO_PATH);

			CreateGitTestMockUp();
			new Gicc().CopyAndCommit(CCHistoryList, git.LastGiccPull, DateTime.Now);
			throw new NotImplementedException();
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
		public void GetCommitPointsTest()
		{
			DateTime commitPoint = new DateTime(2015, 4, 1, 1, 0, 0);

			List<DateTime> expected = new List<DateTime>();
			List<DateTime> actual = new Gicc().GetCommitPoints(CCHistoryList);

			expected.Add(commitPoint.AddMinutes(0));
			expected.Add(commitPoint.AddMinutes(2));
			expected.Add(commitPoint.AddMinutes(4));
			expected.Add(commitPoint.AddMinutes(5));
			expected.Add(commitPoint.AddMinutes(8));
			expected.Add(commitPoint.AddMinutes(9));
			expected.Add(commitPoint.AddMinutes(11));

			Assert.AreEqual(expected, actual);
		}

		private List<CCElementVersion> CCHistoryList
		{
			get
			{
				List<CCElementVersion> ccHistoryList = new List<CCElementVersion>();
				DateTime checkinTime = new DateTime(2015, 4, 1, 1, 0, 0);

				// FindAllFilesInBranch() 함수를 먼저 호출하므로 history 는 file 단위로 나타남.
				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime,
					ElementName = "tt.txt",
					Version = @"\main\0",
					Operation = "mkelem",
					OwnerFullName = "Init"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime,
					ElementName = "tt.txt",
					Version = @"\main\1",
					Operation = "mkelem",
					OwnerFullName = "Init"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(1),
					ElementName = "tt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\0",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(1),
					ElementName = "tt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\1",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(3),
					ElementName = "tt.txt",
					Version = @"\main\2",
					Operation = "chekin",
					OwnerFullName = "B"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(6),
					ElementName = "tt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\2",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(8),
					ElementName = "tt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\3",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(10),
					ElementName = "tt.txt",
					Version = @"\main\3",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				/*
				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(12),
					ElementName = "tt.txt",
					Version = @"\main\3",
					Operation = "rmelem",
					OwnerFullName = "A"
				}
				);
				*/

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime,
					ElementName = @"sub\subtt.txt",
					Version = @"\main\0",
					Operation = "mkelem",
					OwnerFullName = "Init"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime,
					ElementName = @"sub\subtt.txt",
					Version = @"\main\1",
					Operation = "mkelem",
					OwnerFullName = "Init"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(2),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\0",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(2),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\1",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(4),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\2",
					Operation = "chekin",
					OwnerFullName = "B"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(5),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\3",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(7),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\2",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(9),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\3",
					Operation = "chekin",
					OwnerFullName = "B"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(11),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\4",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				/*
				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(13),
					ElementName = "tt.txt",
					Version = @"\main\4",
					Operation = "rmelem",
					OwnerFullName = "B"
				}
				);
				*/

				return ccHistoryList;
			}
		}

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

    [Test]
    public void CheckAllSymbolicLinksAreMountedTest()
    {
      // setup
      SetConfig(VOB_PATH, IOHandler.BranchName, IOHandler.RepoPath);

      string vobTag = System.Configuration.ConfigurationManager.AppSettings["VobTag"];
			
			new ClearCase(Path.GetDirectoryName(VOB_PATH)).Mount(vobTag);
			new ClearCase(VOB_PATH).CheckAllSymbolicLinksAreMounted();

			new ClearCase(Path.GetDirectoryName(VOB_PATH)).UMount(vobTag);
      try
      {
				new ClearCase(VOB_PATH).CheckAllSymbolicLinksAreMounted();
      }
      catch (GiccException ex)
      {
        Console.WriteLine(ex.Message);
        return;
      }

      Assert.Fail(vobTag + " 가 unmount 되었으나 유효성 검사를 통과 하였습니다.");
    }
  }
}
