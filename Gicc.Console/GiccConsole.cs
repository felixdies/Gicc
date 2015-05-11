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
          Clone();
          break;
        case "pull":
          Pull();
          break;
        case "push":
          Push();
          break;
        case "tree": case "tr":
          if (args.Length < 2)
          {
            WriteLine(Usage.Tree);
            return;
          }
          ViewCCVersionTree(args[1]);
          break;
        case "label": case "lb":
          if (args.Length < 3)
          {
            WriteLine(Usage.Label);
            return;
          }
          MakeCCLabel(args[1], args[2]);
          break;
        default:
          WriteLine(Usage.Main);
          return;
      }
		}

    static void Clone()
    {
    }

    static void Pull()
    {
    }

    static void Push()
    {
    }

    static void ViewCCVersionTree(string branchName)
    {
      string gitPath = Path.Combine(Environment.CurrentDirectory, ".git");
      bool gitInitialized = Directory.Exists(gitPath);

      if (!gitInitialized)
      {
        Directory.CreateDirectory(Path.Combine(gitPath, "gicc"));
        Gicc.SetConfig(Environment.CurrentDirectory, branchName, Environment.CurrentDirectory);
      }

      List<string> BranchFileList = ClearCase.FindAllFilesInBranch(Environment.CurrentDirectory, branchName);
      BranchFileList.ForEach(filePath => ClearCase.ViewVersionTree(filePath));

      if (!gitInitialized)
        Directory.Delete(gitPath);
    }

    static void MakeCCLabel(string branchName, string label)
    {
      List<string> targetExtension = new List<string>(new string[] {".aspx", ".ascx", ".js"});
      string gitPath = Path.Combine(Environment.CurrentDirectory, ".git");
      bool gitInitialized = Directory.Exists(gitPath);

      if (!gitInitialized)
      {
        Directory.CreateDirectory(Path.Combine(gitPath, "gicc"));
        Gicc.SetConfig(Environment.CurrentDirectory, branchName, Environment.CurrentDirectory);
      }

      List<string> branchFileList = ClearCase.FindAllFilesInBranch(Environment.CurrentDirectory, branchName);
      
      foreach (string filePath in branchFileList)
      {
        if(targetExtension.Contains(Path.GetExtension(filePath)))
          ClearCase.LabelLatestMain(filePath, label);
      }

      if (!gitInitialized)
        Directory.Delete(gitPath);
    }

    static void WriteLine(string value)
    {
      System.Console.WriteLine(value);
    }
	}
}
