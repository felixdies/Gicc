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
  public class GitTest : GiccTestBase
  {
    [SetUp]
    public void Init()
    {
      //FileEx.DeleteIfExists(Path.Combine(REPO_PATH));

      //throw new NotImplementedException();
			// todo : initialize git repository
    }

    [Test]
    public void HelpTest()
    {
			string expectedStart = "usage: git ";
			string actual = new Git(REPO_PATH).Help();

			Assert.True(actual.StartsWith(expectedStart));
    }

		[Test]
		public void GetUntrackedFileTest()
		{
			List<string> expected = new List<string>(new string[] { "b", "c" });
			List<string> actual = new Git(REPO_PATH).GetUntrackedFileList();

			Assert.That(actual, Is.EquivalentTo(expected));
		}

		[Test]
		public void Diff()
		{
			List<string> expected = new List<string>(new string[] {
				"a"});
			List<string> actual = new Git(REPO_PATH).GetModifiedFileList();

			Assert.That(actual, Is.EquivalentTo(expected));
		}

    [TearDown]
    public void CleanUp()
    {
    }
  }
}
