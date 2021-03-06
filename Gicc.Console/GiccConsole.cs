﻿using System;
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
          Clone(args);
          WriteLine("Clone success!");
          break;

        case "pull":
          if (args.Length == 2)
          {
            new GiccCore(Environment.CurrentDirectory).Pull();
          }
          else
          {
            WriteLine(Resource.USAGE_PULL);
          }
          WriteLine("Pull success!");
          break;

        case "push":
          if (args.Length == 2)
          {
            new GiccCore(Environment.CurrentDirectory).PushWorkingBranch();
          }
          else
          {
            WriteLine(Resource.USAGE_PUSH);
          }
          WriteLine("Push success!");
          break;

        case "merge":
          if (args.Length == 2)
          {
            new GiccCore(Environment.CurrentDirectory).MergeWorkingBranchIntoMaster();
          }
          else
          {
            //WriteLine(Resource.USAGE_MERGE);
          }
          WriteLine("Merge success!");
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

    private static void Clone(string[] args)
    {
      if (args.Length < 4)
      {
        WriteLine(Resource.USAGE_CLONE);
        return;
      }

      string absVobPath = string.Empty;
      if (!Path.IsPathRooted(args[3]))
      {
        WriteLine("입력한 CC VOB 이 절대 경로가 맞는 지 확인 해 주세요.");
        return;
      }
      else
      {
        absVobPath = args[3];
      }

      string branchName = args[2];

      string absRepoPath = Path.IsPathRooted(args[3]) ? args[3] : Path.Combine(Environment.CurrentDirectory, args[3]);

      new GiccCore(Environment.CurrentDirectory, absVobPath, branchName, absRepoPath).Clone();
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
