﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc.Lib
{
  public class ClearCase : Executor
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearCase" /> class.
    /// Used when execute "Clone", "Pull", or "Push".
    /// </summary>
    /// <param name="constructInfo"></param>
    public ClearCase(ClearCaseConstructInfo constructInfo)
      : base(constructInfo)
    {
      this.VobPath = constructInfo.VobPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearCase" /> class.
    /// Used when execute "Label" or "Tree".
    /// </summary>
    /// <param name="constructInfo"></param>
    public ClearCase(ExecutorConstructInfo constructInfo)
      : base(constructInfo)
    {
    }

    public string VobPath { get; set; }

    protected override string Command
    {
      get
      {
        ProcessStartInfo proInfo = new ProcessStartInfo()
        {
          WorkingDirectory = ExecutingPath,
          FileName = "rcleartool",
          CreateNoWindow = true,
          UseShellExecute = false,
        };

        try
        {
          Process.Start(proInfo); // check if the user uses CCRC
        }
        catch
        {
          return "cleartool";
        }

        return "rcleartool";
      }
    }

    private string Fmt
    {
      get
      {
        return "'"
        + "Attributes=%a"
        + "|Comment=%Nc"
        + "|CreatedDate=%d"
        + "|EventDescription=%e"
        + "|CheckoutInfo=%Rf"
        + "|HostName=%h"
        + "|IndentLevel=%i"
        + "|Labels=%l"
        + "|ObjectKind=%m"
        + "|ElementName=%En"
        + "|Version=%Vn"
        + "|PredecessorVersion=%PVn"
        + "|Operation=%o"
        + "|Type=%[type]p"
        + "|SymbolicLink=%[slink_text]p"
        + "|OwnerLoginName=%[owner]p"
        + "|OwnerFullName=%[owner]Fp"
        + "|HyperLink=%[hlink]p"
        + "\n'";
      }
    }

    public void ViewVersionTree(string filePath)
    {
      Execute("lsvtree -graphical " + filePath + "\\LATEST", false);
    }

    public void LabelLastElements(string labeledBranch, string label)
    {
      // todo: validate label

      FindAllFilesInBranch()
        .Where(filePath => IsLabelingTargetExtension(filePath)).ToList()
        .ForEach(filePath => LabelLastElement(filePath, labeledBranch, label));
    }

    public List<string> CatCS()
    {
      return GetExecutedResultList("catcs");
    }

    public List<string> FindAllFilesInBranch()
    {
      string args = string.Empty;

      if (!string.IsNullOrWhiteSpace(BranchName))
      {
        args += " -branch 'brtype(" + BranchName + ")'";
      }

      return GetExecutedResultList("find . " + args + " -print");
    }

    public List<string> FindAllFilesInBranch(DateTime since, DateTime until)
    {
      string args = string.Empty;

      if (!string.IsNullOrWhiteSpace(BranchName))
      {
        args += " -branch 'brtype(" + BranchName + ")'";
      }

      args += " -version '{created_since(" + since.AddSeconds(1).ToString() + ") && !created_since(" + until.AddSeconds(1).ToString() + ")}'";

      return GetExecutedResultList("find . " + args + " -print");
    }

    internal void CheckCheckedoutFileIsNotExist()
    {
      List<string> checkedoutFileList = LscheckoutInCurrentView();
      if (checkedoutFileList.Count > 0)
      {
        string message = "체크아웃 된 파일이 있습니다." + Environment.NewLine
          + string.Join(Environment.NewLine, checkedoutFileList);
        throw new GiccException(message);
      }
    }

    internal void CheckAllSymbolicLinksAreMounted()
    {
      List<CCElementVersion> slinkList = FindAllSymbolicLinks();

      foreach (CCElementVersion link in slinkList)
      {
        if (!System.IO.Directory.Exists(link.SymbolicLink))
        {
          throw new GiccException(link.SymbolicLink + " VOB 이 mount 되지 않았습니다.");
        }
      }
    }

    private void SetCS(string[] configSpec)
    {
      File.WriteAllLines(OutPath, configSpec);
      Execute("setcs " + OutPath);
    }

    internal void SetBranchCS()
    {
      string[] branchCS = new string[] 
      {
        "element * CHECKEDOUT",
        "element -dir * /main/LATEST",
        "element -file * /main/" + BranchName + "/LATEST",
        "element -file * /main/LATEST -mkbranch " + BranchName
      };

      SetCS(branchCS);
    }

    internal void SetBranchCS(DateTime time)
    {
      string[] branchCS = new string[] 
      {
        "time " + time.ToString(),
        "element * CHECKEDOUT",
        "element -dir * /main/LATEST",
        "element -file * /main/" + BranchName + "/LATEST",
        "element -file * /main/LATEST -mkbranch " + BranchName,
        "end time"
      };

      SetCS(branchCS);
    }

    internal void SetMainCS(DateTime time)
    {
      string[] mainCS = new string[] 
      {
        "time " + time.ToString(),
        "element * CHECKEDOUT",
        "element * /main/LATEST",
        "end time"
      };

      SetCS(mainCS);
    }

    internal void SetDefaultCS()
    {
      Execute("setcs -default");
    }

    // vob path 의 상위 디렉터리에서 mount 를 실행해야 한다.
    internal void Mount(string vobTag)
    {
      Execute("mount \\" + vobTag);
    }

    // vob path 의 상위 디렉터리에서 umount 를 실행해야 한다.
    internal void UMount(string vobTag)
    {
      Execute("umount \\" + vobTag);
    }

    internal void Checkout(string pname)
    {
      Execute("checkout -ncomment " + pname);
    }

    internal void Uncheckout(string pname)
    {
      Execute("uncheckout -keep " + pname);
    }

    internal bool IsLabelingTargetExtension(string filePath)
    {
      List<string> targetExtension = new List<string>(new string[] { ".aspx", ".ascx", ".js", ".sql" });
      return targetExtension.Contains(Path.GetExtension(filePath.Split('@')[0]));
    }

    internal string GetCurrentView()
    {
      return GetExecutedResult("pwv").Split(' ')[3];
    }

    internal string GetLogInUser()
    {
      List<string> viewInfo = GetExecutedResultList("lsview -long " + GetCurrentView());
      return viewInfo.Find(info => info.StartsWith("View owner")).Split(' ')[2];
    }

    internal string GetLogInUserName()
    {
      return GetLogInUser().Split('\\').Last();
    }

    internal string Pwd()
    {
      return GetExecutedResult("pwd");
    }

    internal CCElementVersion Describe(string pname)
    {
      string description = GetExecutedResult("describe -fmt " + Fmt + " " + pname);
      return new CCElementVersion(description) { VobPath = this.VobPath };
    }

    internal List<string> LscheckoutInCurrentViewByLoginUser()
    {
      return GetExecutedResultList("lscheckout -short -cview -me -recurse");
    }

    internal List<string> LscheckoutInCurrentView()
    {
      return GetExecutedResultList("lscheckout -short -cview -recurse");
    }

    internal List<CCElementVersion> FindAllSymbolicLinks()
    {
      List<CCElementVersion> resultSLinkList = new List<CCElementVersion>();
      List<string> foundSLinkList;

      foundSLinkList = GetExecutedResultList("find " + VobPath + " -type l -print");

      foundSLinkList.ForEach(link => resultSLinkList.Add(Describe(link)));

      return resultSLinkList;
    }

    internal List<CCElementVersion> GetAllVersionsInBranch()
    {
      List<CCElementVersion> resultList = new List<CCElementVersion>();

      foreach(string file in FindAllFilesInBranch())
      {
        resultList.AddRange(Lshistory(file));
      }

      return resultList;
    }

    internal List<CCElementVersion> Lshistory(string pname)
    {
      List<CCElementVersion> resultList = new List<CCElementVersion>();

      GetExecutedResultList("lshistory -fmt " + Fmt + " " + pname)
        .ForEach(elemVersion => resultList.Add(
          new CCElementVersion(elemVersion) 
          {
            VobPath = this.VobPath
          }));

      return resultList;
    }

    internal List<CCElementVersion> Lshistory(string pname, DateTime since)
    {
      List<CCElementVersion> resultList = new List<CCElementVersion>();

      GetExecutedResultList("lshistory -fmt " + Fmt + " -since" + since.AddSeconds(1) + " " + pname)
        .ForEach(elemVersion => resultList.Add(
          new CCElementVersion(elemVersion) 
          {
            VobPath = this.VobPath 
          }));

      return resultList;
    }

    private void LabelLastElement(string filePath, string branch, string label)
    {
      Execute("mklabel -replace -version \\" + branch + "\\LATEST " + label + " " + filePath, false);
    }
  }
}
