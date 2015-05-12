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
      FileEx.DeleteIfExists(Path.Combine(REPO_PATH));

      throw new NotImplementedException();
			// todo : initialize git repository
    }

    [Test]
    public void StashTest()
    {

    }

    [TearDown]
    public void CleanUp()
    {
    }
  }
}
