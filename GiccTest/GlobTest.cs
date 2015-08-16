using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using IronRuby.Builtins;

namespace Gicc.Test
{
  [TestFixture]
  public class GlobTest
  {
    [TestCase("build/", @".builds", true)]
    [TestCase("*.suo", @"0.suo", true)]
    [TestCase("*.sln.docstates", @"0.sln.docstates", true)]
    [TestCase("*.ide/", @"0.ide\0", true)]
    [TestCase("*.ide/", @"0.sln.ide\0", true)]
    [TestCase("[Dd]ebug/", @"debug\0", true)]
    [TestCase("[Dd]ebug/", @"Debug\0", true)]
    [TestCase("x86/", @"x86\0", true)]
    [TestCase("[Tt]est[Rr]esult*/", @"testresult\0", true)]
    [TestCase("[Tt]est[Rr]esult*/", @"testresult1\0", true)]
    [TestCase("[Tt]est[Rr]esult*/", @"testResult\0", true)]
    [TestCase("[Tt]est[Rr]esult*/", @"Testresult1\0", true)]
		[TestCase("", @"file", false)]
    [TestCase("", @"dir\0", false)]
    public void FnMatch(string pattern, string path, bool expectedResult)
    {
      Assert.AreEqual(expectedResult, Glob.FnMatch(pattern, path, 0));
    }
  }
}
