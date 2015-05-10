using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc
{
	class GiccException : Exception
	{
		public GiccException()
		{
		}

		public GiccException(string message) : base(message)
		{
		}
	}
}
