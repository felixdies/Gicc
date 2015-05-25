using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Gicc;

namespace Gicc.Console
{
	public class GiccConsole
	{
		static void Main(string[] args)
		{
      if (args.Length == 0)
      {
        WriteLine(Usage.Main);
        return;
      }

      switch (args[0].ToLower())
      {
        case "clone":
          new Gicc(Environment.CurrentDirectory, args[1], args[2], args[3]).Clone();
          break;

        case "pull":
					new Gicc(Environment.CurrentDirectory, true).Pull();
          break;

        case "push":
					new Gicc(Environment.CurrentDirectory, true).Push();
          break;

        case "tree": case "tr":
          if (args.Length < 2)
          {
            WriteLine(Usage.Tree);
            return;
          }
					new Gicc(Environment.CurrentDirectory, false).ViewCCVersionTrees(args[1]);
          break;

        case "label": case "lb":
          if (args.Length < 3)
          {
            WriteLine(Usage.Label);
            return;
          }
          new Gicc(Environment.CurrentDirectory, false).MakeCCLabel(args[1], args[2]);
          break;

        default:
          WriteLine(Usage.Main);
          return;
      }
		}

    static void WriteLine(string value)
    {
      System.Console.WriteLine(value);
    }
	}
}
