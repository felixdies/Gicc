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
  public class ClearCaseTest : GiccTestBase
  {
    string VOB_TAG = System.Configuration.ConfigurationManager.AppSettings["VobTag"];

    /// <summary>
    /// main Branch 의 cc 실행 정보.
    /// cleartool 실행 경로는 언제나 cc 의 VOB path 이다.
    /// </summary>
    private ClearCaseConstructInfo MainCCInfo
    {
      get
      {
        return new ClearCaseConstructInfo()
        {
          VobPath = VOB_PATH,
          BranchName = "main",
          ExecutingPath = CC_TEST_PATH,
          OutPath = Path.Combine(GICC_PATH, "ccout"),
          LogPath = Path.Combine(GICC_PATH, "log")
        };
      }
    }

    /// <summary>
    /// 특정 Branch 의 cc 실행 정보.
    /// cleartool 실행 경로는 언제나 cc 의 VOB path 이다.
    /// </summary>
    private ClearCaseConstructInfo BranchCCInfo
    {
      get
      {
        return new ClearCaseConstructInfo()
        {
          VobPath = VOB_PATH,
          BranchName = BRANCH_NAME,
          ExecutingPath = CC_TEST_PATH,
          OutPath = Path.Combine(GICC_PATH, "ccout"),
          LogPath = Path.Combine(GICC_PATH, "log")
        };
      }
    }

    /// <summary>
    /// Git 과 상관 없이 CC 를 실행 할 때 사용하는 생성자 정보
    /// </summary>
    private ExecutorConstructInfo CCInfo
    {
      get
      {
        return new ExecutorConstructInfo()
        {
          BranchName = BRANCH_NAME,
          ExecutingPath = CC_TEST_PATH,
          OutPath = "giccout",
          LogPath = "gicclog"
        };
      }
    }

    [Test]
    public void PwdTest()
    {
      Assert.AreEqual(CC_TEST_PATH, new ClearCase(BranchCCInfo).Pwd());
    }

    [Test]
    public void FindAllSymbolicLinksTest()
    {
      string expected = CC_SYMBOLIC_LINK_PATH;
      string actual = new ClearCase(BranchCCInfo).FindAllSymbolicLinks()[0].SymbolicLink;

      Assert.AreEqual(expected, actual);
    }

    [Test]
    // CastCS() is tested as well.
    public void SetDefaultCSTest()
    {
      new ClearCase(BranchCCInfo).SetDefaultCS();

      List<string> expected = new List<string>(
        new string[] { "element * CHECKEDOUT", "element * /main/LATEST" }
        );
      List<string> actual = new ClearCase(MainCCInfo).CatCS();

      Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Test]
    // CastCS() is tested as well.
    public void SetBranchCSTest()
    {
      List<string> expected = new List<string>(new string[] {
				"element * CHECKEDOUT",
				"element -dir * /main/LATEST",
				"element -file * /main/" + BRANCH_NAME + @"/LATEST",
				"element -file * /main/LATEST -mkbranch " + BRANCH_NAME
			});

      new ClearCase(BranchCCInfo).SetBranchCS();

      Assert.That(new ClearCase(BranchCCInfo).CatCS(), Is.EquivalentTo(expected));

      // teardown
      new ClearCase(BranchCCInfo).SetDefaultCS();
    }

    [Test]
    public void CurrentViewTest()
    {
      Assert.AreEqual(ConfigurationManager.AppSettings["ViewName"], new ClearCase(BranchCCInfo).GetCurrentView());
    }

    [Test]
    public void LogInUserTest()
    {
      Assert.AreEqual(ConfigurationManager.AppSettings["LogInUser"], new ClearCase(BranchCCInfo).GetLogInUser());
    }

    [Test]
    public void LogInUserNameTest()
    {
      Assert.AreEqual(ConfigurationManager.AppSettings["LogInUserName"], new ClearCase(BranchCCInfo).GetLogInUserName());
    }

    [Test]
    public void CheckoutTest()
    {
      ClearCase cc = new ClearCase(BranchCCInfo);
      cc.SetBranchCS();

      List<string> targetFiles = new List<string>(new string[] { "main.txt", @".\sub\main.txt" });
      List<string> actual;

      cc.Checkout(targetFiles[0]);
      cc.Checkout(targetFiles[1]);

      actual = cc.LscheckoutInCurrentViewByLoginUser();

      Assert.That(actual, Is.EquivalentTo(targetFiles));

      cc.Uncheckout(targetFiles[0]);
      cc.Uncheckout(targetFiles[1]);

      actual = cc.LscheckoutInCurrentViewByLoginUser();

      Assert.IsTrue(actual.Count == 0);
    }

    [Test]
    public void CheckAllSymbolicLinksAreMountedTest()
    {
      ExecutorConstructInfo VobInfo = new ExecutorConstructInfo()
      {
        ExecutingPath = VOB_PATH,
        OutPath = "giccout",
        LogPath = "gicclog"
      };

      ExecutorConstructInfo ParentOfVobInfo = new ExecutorConstructInfo()
      {
        ExecutingPath = Path.GetDirectoryName(VOB_PATH),
        OutPath = "giccout",
        LogPath = "gicclog"
      };

      new ClearCase(ParentOfVobInfo).Mount(VOB_TAG);

      // Assert this method doesn't throw an exception
      new ClearCase(VobInfo).CheckAllSymbolicLinksAreMounted();

      new ClearCase(ParentOfVobInfo).UMount(VOB_TAG);
      try
      {
        new ClearCase(VobInfo).CheckAllSymbolicLinksAreMounted();
      }
      catch (GiccException ex)
      {
        Console.WriteLine(ex.Message);
        return;
      }

      Assert.Fail(VOB_TAG + " 가 unmount 되었으나 유효성 검사를 통과 하였습니다.");
    }

    [Test]
    public void CheckAnyFileIsNotCheckedOutTest()
    {
      string checkoutFile = "main.txt";

      ClearCase cc = new ClearCase(BranchCCInfo);

      cc.CheckCheckedoutFileIsNotExist();

      cc.Checkout(checkoutFile);

      try
      {
        cc.CheckCheckedoutFileIsNotExist();
      }
      catch (GiccException ex)
      {
        Console.WriteLine("success : caught checkedout file");
        Console.WriteLine(ex.Message);

        return;
      }
      finally
      {
        cc.Uncheckout(checkoutFile);
      }

      Assert.Fail("could not catch checkout file : " + checkoutFile);
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
      ClearCase cc = new ClearCase(BranchCCInfo);

      Assert.AreEqual(result, cc.IsLabelingTargetExtension(path));
    }
  }
}
