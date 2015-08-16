using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gicc.Lib;

namespace Gicc.Console
{
  /// <summary>
  /// 
  /// </summary>
  public class GiccConsole
  {
    private static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        WriteLine(Resource.USAGE_MAIN);
        return;
      }

      ClearCase cc;

      switch (args[0].ToLower())
      {
        case "clone":
          new GiccCore(Environment.CurrentDirectory, args[1], args[2], args[3]).Clone();
          break;

        case "pull":
          new GiccCore(Environment.CurrentDirectory).Pull();
          break;

        case "push":
          new GiccCore(Environment.CurrentDirectory).Push();
          break;

        case "list":
          if (args.Length < 2)
          {
            WriteLine(Resource.USAGE_LIST);
            return;
          }

          cc = new ClearCase(CreateCCInfo(args[1], Environment.CurrentDirectory));
          cc.FindAllFilesInBranch().ForEach(file => WriteLine(file));
          break;

        case "tree":
          if (args.Length < 2)
          {
            WriteLine(Resource.USAGE_TREE);
            return;
          }

          cc = new ClearCase(CreateCCInfo(args[1], Environment.CurrentDirectory));
          cc.FindAllFilesInBranch().ForEach(filePath => cc.ViewVersionTree(filePath));
          break;

        case "label":
          Label(args);
          break;

        case "cs":
          ConfigSpec(args);
          break;

        default:
          WriteLine(Resource.USAGE_MAIN);
          return;
      }
    }

    private static void Label(string[] args)
    {
      if (args.Length < 4)
      {
        WriteLine(Resource.USAGE_LABEL);
        return;
      }

      string labeledBranch;

      switch (args[1].ToLower())
      {
        case "-main":
        case "-m":
          labeledBranch = "main";
          break;

        case "-branch":
        case "-b":
          labeledBranch = "main\\" + args[2];
          break;

        default:
          WriteLine(Resource.USAGE_LABEL);
          return;
      }

      ClearCase cc = new ClearCase(CreateCCInfo(labeledBranch, Environment.CurrentDirectory));
      cc.LabelLastElements(labeledBranch, args[3]);
    }

    private static void ConfigSpec(string[] args)
    {
      switch (args.Length)
      {
        case 2:
          new ClearCase(CreateCCInfo(string.Empty, Environment.CurrentDirectory)).CatCS()
            .ForEach(line => WriteLine(line));
          break;
        /*
        case 3:
          new GiccCore(Environment.CurrentDirectory, args[2]).SetBranchCS();
          break;
        */
        default:
          WriteLine(Resource.USAGE_CS);
          return;
      }
    }

    /// <summary>
    /// Git 과 상관 없이 CC 를 실행 할 때 사용하는 생성자 정보
    /// </summary>
    private static ClearCaseConstructInfo CreateCCInfo(string branchName, string executingPath)
    {
      return new ClearCaseConstructInfo()
      {
        BranchName = branchName,
        ExecutingPath = executingPath,
        OutPath = Path.Combine(executingPath, "giccout.txt"),
        LogPath = Path.Combine(executingPath, "gicclog.txt")
      };
    }

    private static void WriteLine(string value)
    {
      System.Console.WriteLine(value);
    }
  }
}
