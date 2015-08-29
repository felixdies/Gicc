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
    [TestCase(@"0.sln.ide", true)]
    [TestCase(@"debug", true)]
    [TestCase(@"Debug", true)]
    [TestCase(@"x86", true)]
    [TestCase(@"testresult", true)]
    [TestCase(@"testresult1", true)]
    [TestCase(@"sub/testResult", true)]
    [TestCase(@"sub\Testresult1", true)]
    [TestCase(@"0.ide", true)]
    [TestCase(@"dir", false)]
    [TestCase(@"packages/build", false)]
    [TestCase(@"0/packages/build", false)]
    [TestCase(@"0/1/packages/build", false)]
    public void IsIgnoredDirTest(string path, bool expectedResult)
    {
      string gitignore = Resource.GetResource("gitignore.txt");
      string[] ignoreArr = gitignore.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

      GitIgnore gitIgnore = new GitIgnore(ignoreArr);

      Assert.AreEqual(expectedResult, gitIgnore.IsIgnoredDir(path));
    }

    [TestCase(@".builds", true)]
    [TestCase(@"0.suo", true)]
    [TestCase(@"sub\0.suo", true)]
    [TestCase(@"sub/0.suo", true)]
    [TestCase(@"0.sln.docstates", true)]
    [TestCase(@"testresult\0", true)]
    [TestCase(@"testresult1/0", true)]
    [TestCase(@"sub/testResult/0", true)]
    [TestCase(@"sub\Testresult1\0", true)]
		[TestCase(@"file", false)]
    public void IsIgnoredFileTest(string path, bool expectedResult)
    {
      string gitignore = Resource.GetResource("gitignore.txt");
      string[] ignoreArr = gitignore.Split(new string[] { "\r\n" },StringSplitOptions.RemoveEmptyEntries);

      GitIgnore gitIgnore = new GitIgnore(ignoreArr);

      Assert.AreEqual(expectedResult, gitIgnore.IsIgnoredFile(path));
    }

    [TestCase("", "", typeof(ArgumentException))]
    [TestCase(@"*.suo", @"(.+[\\/])*[^\\/]*\.suo", null)]
    public void GlobPatternToRegexTest(string globPatt, string expectedRegex, Type expectedException)
    {
      if (expectedException != null)
      {
        Assert.Catch(expectedException, () => GitIgnore.GlobPatternToRegex(globPatt));
        return;
      }

      Regex actualRegex = GitIgnore.GlobPatternToRegex(globPatt);
      Assert.AreEqual(expectedRegex, actualRegex.ToString());
    }
  }
}
