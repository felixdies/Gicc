using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using NUnit.Framework;
using Gicc.Lib;

namespace Gicc.Test
{
  [TestFixture]
  public class GitIgnoreTest
  {
    [TestCase(@".builds", true)]
    [TestCase(@"0.suo", true)]
    [TestCase(@"sub\0.suo", true)]
    [TestCase(@"sub/0.suo", true)]
    [TestCase(@"0.sln.docstates", true)]
    [TestCase(@"0.ide\0", true)]
    [TestCase(@"0.sln.ide\0", true)]
    [TestCase(@"debug\0", true)]
    [TestCase(@"Debug\0", true)]
    [TestCase(@"x86\0", true)]
    [TestCase(@"testresult\0", true)]
    [TestCase(@"testresult1/0", true)]
    [TestCase(@"testResult\0", true)]
    [TestCase(@"Testresult1/0", true)]
		[TestCase(@"file", false)]
    [TestCase(@"dir\0", false)]
    public void IsIgnoredFileTest(string path, bool expectedResult)
    {
      string gitignore = Resource.GetResource("gitignore.txt");
      string[] ignoreArr = gitignore.Split(new string[] { "\r\n" },StringSplitOptions.RemoveEmptyEntries);

      GitIgnore gitIgnore = new GitIgnore(ignoreArr);

      Assert.AreEqual(expectedResult, gitIgnore.IsIgnoredFile(path));
    }

    [TestCase("", "", typeof(ArgumentException))]
    [TestCase("suo", ".*suo", null)]
    public void GlobPatternToRegexTest(string globPatt, string expectedRegex, Type expectedException)
    {
      if (expectedException != null)
      {
        Assert.Catch(expectedException, () => new GitIgnore().GlobPatternToRegex(globPatt));
        return;
      }

      Regex actualRegex = new GitIgnore().GlobPatternToRegex(globPatt);
      Assert.AreEqual(expectedRegex, actualRegex.ToString());
    }
  }
}
