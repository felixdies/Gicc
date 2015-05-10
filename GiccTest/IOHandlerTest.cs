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
	public class IOHandlerTest : GiccTestBase
	{
		[Test]
		public void ReadEmptyCCoutTest()
		{
			File.WriteAllText(IOHandler.CCoutPath, "");
			
			List<string> expected = new List<string>();
			List<string> actual = IOHandler.ReadCCout();

			Assert.That(actual, Is.EquivalentTo(expected));
		}

		[Test]
		public void ReadCCoutTest()
		{
			// setup
			FileEx.BackUp(IOHandler.CCoutPath);

			File.WriteAllLines(IOHandler.CCoutPath, new string[] { "a", "b", "c" });

			List<string> expected = new List<string>(new string[] { "a", "b", "c" });
			Assert.That(IOHandler.ReadCCout(), Is.EquivalentTo(expected));

			// teardown
			FileEx.Restore(IOHandler.CCoutPath);
		}

		[Test]
		public void WriteConfigTest()
		{
			// setup
			FileEx.BackUp(Path.Combine(REPO_PATH, @".git/gicc/config"));

			IOHandler.WriteConfig(new List<string>(new string[] { "a", "b", "c" }));
			List<string> expected = new List<string>(new string[] { "a", "b", "c" });
			Assert.That(IOHandler.ReadConfig(), Is.EquivalentTo(expected));

			IOHandler.WriteConfig(new List<string>(new string[] { "a\nb\r\nc" }));
			expected = new List<string>(new string[] { "a", "b", "c" });
			Assert.That(IOHandler.ReadConfig(), Is.EquivalentTo(expected));

			// teardown
			FileEx.Restore(Path.Combine(REPO_PATH, @".git/gicc/config"));
		}

		[Test]
		public void GetConfigTest()
		{
			// setup
            FileEx.BackUp(Path.Combine(REPO_PATH, @".git/gicc/config"));

            File.WriteAllLines(Path.Combine(REPO_PATH, @".git/gicc/config"), new string[] { "a", "b", "c" });

			List<string> expected = new List<string>(new string[] { "a", "b", "c" });
			Assert.That(IOHandler.ReadConfig(), Is.EquivalentTo(expected));

			// teardown
            FileEx.Restore(Path.Combine(REPO_PATH, @".git/gicc/config"));
		}

		[Test]
		public void GetGitPathTest()
		{
			Assert.AreEqual(REPO_PATH, IOHandler.RepoPath);
		}
	}
}
