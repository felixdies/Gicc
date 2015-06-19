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
	public class ExecutorTest : GiccTestBase
	{
		[Test]
		/// Performance test - result : 0.5 sec per each process execution
		public void GetExecutedResultWithOutFIOTest()
		{
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			Console.WriteLine("start GetExecutedResultWithOutFIO()");
			sw.Start();

			for (int i = 0; i < 10; i++)
			{
				try
				{
					new Git(GitInfo).GetExecutedResultWithOutFIO("git");
				}
				catch { }
			}

			sw.Stop();
			Console.WriteLine("end GetExecutedResultWithOutFIO(). Elapsed time : " + sw.Elapsed);
		}
	}
}
