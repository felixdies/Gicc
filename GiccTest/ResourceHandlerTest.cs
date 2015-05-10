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
	public class ResourceHandlerTest : GiccTestBase
	{
		[Test]
		public void GetResourceTest()
		{
			// clone
			string expected = "usage : gicc clone <CC VOB> <CC branch name> <git 저장소 이름>";
			string actual = ResourceHandler.GetResource("usage_clone.txt");

			Assert.AreEqual(expected, actual);
		}
	}
}
