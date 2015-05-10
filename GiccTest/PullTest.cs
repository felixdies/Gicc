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
	public class PullTest : GiccTestBase
	{
		[Test]
		public void ExecuteTest()
		{
			DateTime since = DateTime.Parse("2015-05-04 15:41:34");   // 마지막 push 시점으로 가정.
			DateTime until = DateTime.Now;

			Pull.Execute(since, until);
		}

		[Test]
		public void CheckCheckedOutFileIsNotExistTest()
		{
			string checkoutFile = "main.txt";

			Pull.CheckCheckedOutFileIsNotExist();

			ClearCase.Checkout(checkoutFile);

			try
			{
				Pull.CheckCheckedOutFileIsNotExist();
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
			Pull.CheckAllSymbolicLinksAreMounted();

			ClearCase.UMount(vobTag);
			try
			{
				Pull.CheckAllSymbolicLinksAreMounted();
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
