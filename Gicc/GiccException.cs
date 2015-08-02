using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc
{
  public class GiccException : Exception
  {
    public GiccException()
      : base()
    {
    }

    public GiccException(string message)
      : base(message)
    {
    }
  }
}
