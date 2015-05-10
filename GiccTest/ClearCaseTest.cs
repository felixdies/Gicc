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
	public class ClearCaseTest : GiccTestBase
	{
		[Test]
		public void FindAllSymbolicLinksTest()
		{
			SetConfig(VOB_PATH, BRANCH_NAME, REPO_PATH);
			string expected = CC_SYMBOLIC_LINK_PATH;
			List<CCElementVersion> actual = ClearCase.FindAllSymbolicLinks();

			Assert.AreEqual(expected, actual[0].SymbolicLink);
		}

		[Test]
		public void GetConfigSpecTest()
		{
			// setup
			ClearCase.SetDefaultCS();

			List<string> expected = new List<string>(
				new string[] {"element * CHECKEDOUT","element * /main/LATEST"}
				);
			List<string> actual = ClearCase.CatCS();

			Assert.That(actual, Is.EquivalentTo(expected));
		}

		[Test]
		public void ConfigSpecTest()
		{
			List<string> expected = new List<string>(new string[] {
				"element * CHECKEDOUT"
				, "element -dir * /main/LATEST"
				, "element -file * /main/" + BRANCH_NAME + @"/LATEST"
				, "element -file * /main/LATEST -mkbranch " + BRANCH_NAME
			});

			ClearCase.SetCS(expected);

			Assert.That(ClearCase.CatCS(), Is.EquivalentTo(expected));

			// teardown
			ClearCase.SetDefaultCS();
		}

		[Test]
		public void SetBranchConfigSpecTest()
		{
			List<string> expected = new List<string>(new string[] {
				"element * CHECKEDOUT"
				, "element -dir * /main/LATEST"
				, "element -file * /main/" + BRANCH_NAME + @"/LATEST"
				, "element -file * /main/LATEST -mkbranch " + BRANCH_NAME
			});

			ClearCase.SetBranchCS(IOHandler.BranchName);

			Assert.That(ClearCase.CatCS(), Is.EquivalentTo(expected));

			// teardown
			ClearCase.SetDefaultCS();
		}

		[Test]
		public void CurrentViewTest()
		{
			Assert.AreEqual(ConfigurationManager.AppSettings["ViewName"], ClearCase.CurrentView);
		}

		[Test]
		public void LogInUserTest()
		{
			Assert.AreEqual(ConfigurationManager.AppSettings["LogInUser"], ClearCase.LogInUser);
		}
		
		[Test]
		public void LogInUserNameTest()
		{
            Assert.AreEqual(ConfigurationManager.AppSettings["LogInUserName"], ClearCase.LogInUserName);
		}

		[Test]
		public void CheckoutTest()
		{
			// setup
			List<string> storedConfigSpec = ClearCase.CatCS();
			ClearCase.SetBranchCS(IOHandler.BranchName);

			List<string> targetFiles = new List<string>(new string[] {"main.txt", @".\sub\main.txt"});
			List<string> actual;
			
			ClearCase.Checkout(targetFiles[0]);
			ClearCase.Checkout(targetFiles[1]);

			actual = ClearCase.LscheckoutInCurrentViewByLoginUser();

			Assert.That(actual, Is.EquivalentTo(targetFiles));

			ClearCase.Uncheckout(targetFiles[0]);
			ClearCase.Uncheckout(targetFiles[1]);

			actual = ClearCase.LscheckoutInCurrentViewByLoginUser();
			
			Assert.That(actual, Is.EquivalentTo(new List<string>()));

			// teardown
			ClearCase.SetCS(storedConfigSpec);
		}

		[Test]
		public void ExecuteTest()
		{
			ClearCase.Pwd();

			List<string> expected = new List<string>(new string[] { IOHandler.VobPath });
			List<string> actual = IOHandler.ReadCCout();

			Assert.That(actual, Is.EquivalentTo(expected));
		}
	}

	[TestFixture]
	public class CCFileVersionTest : GiccTestBase
	{
		[Test]
		public void ParseFmtTest()
		{
			CCElementVersion expected = new CCElementVersion();
			CCElementVersion actual = new CCElementVersion();

			expected.CreatedDate = DateTime.Parse("2015-05-04 15:57:38");
			expected.EventDescription = "checkout version";
			expected.CheckoutInfo = "reserved";
			expected.HostName = ConfigurationManager.AppSettings["HostName"];
			expected.ObjectKind = "version";
			expected.ElementName = "branch1.txt";
			expected.Version = @"\main\" + BRANCH_NAME + @"\CHECKEDOUT";
			expected.PredecessorVersion = @"\main\" + BRANCH_NAME + @"\4";
			expected.Operation = "checkout";
			expected.Type = "text_file";
			expected.OwnerLoginName = ConfigurationManager.AppSettings["LogInUserName"];
			expected.OwnerFullName = "Park";

            actual.ParseFileInfo(@"null|Attributes=|Comment=|CreatedDate=2015-05-04T15:57:38+09:00|EventDescription=checkout version|CheckoutInfo=reserved|HostName=D-SEL-00650801|IndentLevel=|Labels=|ObjectKind=version|ElementName=branch1.txt|Version=\main\" + BRANCH_NAME + @"\CHECKEDOUT|PredecessorVersion=\main\" + BRANCH_NAME
                + @"\4|Operation=checkout|Type=text_file|SymbolicLink=|OwnerLoginName=" + ConfigurationManager.AppSettings["LogInUserName"]
                + "|OwnerFullName=Park|HyperLink=");			
			AssertEx.PropertyValuesAreEquals(expected, actual);
			
			expected.CreatedDate = DateTime.Now;
			AssertEx.PropertyValuesAreNotEquals(expected, actual);
		}
	}
}
