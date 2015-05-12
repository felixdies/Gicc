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
	public class GiccTestBase
	{
		protected string VOB_PATH = ConfigurationManager.AppSettings["VobPath"];
		protected string CC_TEST_PATH = ConfigurationManager.AppSettings["CCTestPath"];
		protected string CC_SYMBOLIC_LINK_PATH = ConfigurationManager.AppSettings["CCSymbolicLinkPath"];
		protected string BRANCH_NAME = ConfigurationManager.AppSettings["BranchName"];
		protected string REPO_PATH = ConfigurationManager.AppSettings["RepoPath"];

		[SetUp]
		public void initBase()
		{
			string GICC_PATH = Path.Combine(REPO_PATH, ".git/gicc");

			if (!Directory.Exists(GICC_PATH))
				Directory.CreateDirectory(GICC_PATH);

			SetCwd(REPO_PATH);

			FileEx.BackUp(Path.Combine(GICC_PATH, "config"));

			SetConfig(CC_TEST_PATH, BRANCH_NAME, REPO_PATH);
		}

		protected static void SetCwd(string path)
		{
			Environment.CurrentDirectory = System.IO.Path.GetFullPath(path);
		}

		protected static void SetConfig(string vobPath, string branchName, string repoPath)
		{
			List<string> config = new List<string>(new string[]{
				@"vob = " + vobPath + "\n"
				, @"branch = " + branchName + "\n"
				, @"repository = " + repoPath + "\n"}
				);
			IOHandler.WriteConfig(config);
		}

		[Test]
		public void AllConfigTest()
		{
			Assert.AreEqual(CC_TEST_PATH, IOHandler.VobPath);
			Assert.AreEqual(BRANCH_NAME, IOHandler.BranchName);
			Assert.AreEqual(REPO_PATH, IOHandler.RepoPath);
		}

		[TearDown]
		public void ClearUpBase()
		{
			// delete config
			FileEx.Restore(Path.Combine(REPO_PATH, ".git/gicc/config"));
		}
	}
}
