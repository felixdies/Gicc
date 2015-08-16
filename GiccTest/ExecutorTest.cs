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
	public class ExecutorTest : GiccTestBase
	{
		[Test]
		/// Performance test - result : 0.5 sec per each process execution
		public void GetExecutedResultListWithoutFIOTest()
		{
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			Console.WriteLine("start GetExecutedResultWithOutFIO()");
			sw.Start();

			Git git = new Git(GitInfo);

			List<string> argList = new string[] { string.Empty, "help" }.ToList();
			List<string> expected = new string[] { "usage", "usage" }.ToList();
			List<string> actual = git.GetExecutedResultListWithoutFIO(argList);

			sw.Stop();

			for (int i = 0; i < expected.Count; i++)
				Assert.That(actual[i].StartsWith(expected[i]));

			Console.WriteLine("end GetExecutedResultWithOutFIO(). Elapsed time : " + sw.Elapsed);
		}
	}
}
