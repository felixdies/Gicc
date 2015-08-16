using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public void FnMatch(string path, bool expectedResult)
    {
      string[] ignoreArr = Resource.GetResource(Resource.GetResource("gitignore.txt")).Split('\n');

      GitIgnore gitIgnore = new GitIgnore(ignoreArr);

      Assert.AreEqual(expectedResult, gitIgnore.IsIgnoredFile(path));
    }
  }
}
