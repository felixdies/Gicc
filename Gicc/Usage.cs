using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc
{
	public class Usage
	{
		public static string Main
		{ get { return ResourceHandler.GetResource("usage_main.txt"); } }

		public static string Clone
		{ get { return ResourceHandler.GetResource("usage_clone.txt"); } }

		public static string Pull
		{ get { return ResourceHandler.GetResource("usage_pull.txt"); } }

		public static string Push
		{ get { return ResourceHandler.GetResource("usage_push.txt"); } }
	}
}
