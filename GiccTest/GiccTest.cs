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
    public void WriteConfigTest()
    {
    }

    [Test]
    public void PullTest()
    {
      Gicc gicc = new Gicc();

      DateTime since = DateTime.Parse("2015-05-04 15:41:34");   // 마지막 push 시점으로 가정.
      DateTime until = DateTime.Now;

      gicc.Pull(since, until);
    }

    [Test]
    public void CheckCheckedOutFileIsNotExistTest()
    {
      string checkoutFile = "main.txt";

      new Gicc().CheckCheckedOutFileIsNotExist();

      ClearCase.Checkout(checkoutFile);

      try
      {
        new Gicc().CheckCheckedOutFileIsNotExist();
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
