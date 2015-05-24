using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Gicc;

namespace Gicc.Test
{
  [TestFixture]
  public class GiccTest : GiccTestBase
  {
		[Test]
		public void PullTest()
		{
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

      new Gicc().CheckAnyFileIsNotCheckedOut();

      ClearCase.Checkout(checkoutFile);

      try
      {
        new Gicc().CheckAnyFileIsNotCheckedOut();
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

      ClearCase.Mount(vobTag);
      new Gicc().CheckAllSymbolicLinksAreMounted();

      ClearCase.UMount(vobTag);
      try
      {
        new Gicc().CheckAllSymbolicLinksAreMounted();
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
