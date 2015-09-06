using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc.Lib
{
  public class Git : Executor
  {
    public Git(GitConstructInfo constructInfo)
      : base(constructInfo)
    {
      if (!Path.IsPathRooted(constructInfo.RepoPath))
      {
        throw new ArgumentException("the repository path is not absolute path.");
      }

      this.RepoPath = constructInfo.RepoPath;
    }

    protected override string Command
    {
      get { return "git"; }
    }

    private GitIgnore _gitIgnore = null;

    internal GitIgnore GitIgnore
    {
      get
      {
        if (_gitIgnore == null)
        {
          string[] gitIgnoreTextArr = File.ReadAllLines(Path.Combine(ExecutingPath, ".gitignore"));
          _gitIgnore = new GitIgnore(gitIgnoreTextArr);
        }

        return _gitIgnore;
      }
    }

    /// <summary>
    /// Gets or sets absolute repository path.
    /// </summary>
    internal string RepoPath { get; set; }

    internal string GetCurrentBranch()
    {
      return GetExecutedResult("rev-parse --abbrev-ref HEAD");
    }

    internal List<string> GetUntrackedFileList()
    {
      return GetExecutedResultList("ls-files --others");
    }

    internal List<string> GetModifiedFileList()
    {
      return GetExecutedResultList("diff --name-only");
    }

    internal List<string> GetBranchList()
    {
      return GetExecutedResultList("branch");
    }

    internal DateTime GetLastGiccPullDate()
    {
      string lastPulledCommit = GetExecutedResult("show-ref --tags gicc_pull");

      // not tagged yet
      if (string.IsNullOrEmpty(lastPulledCommit))
      {
        return new DateTime(1990, 1, 1);
      }

      string lastGiccPullTime = GetExecutedResult("show -s --format=%ai " + lastPulledCommit.Substring(0, 10));
      return DateTime.Parse(lastGiccPullTime);
    }

    internal void CheckModifiedFileIsNotExist()
    {
      if (GetUntrackedFileList().Count > 0)
      {
        string message = "새로 추가된 후 commit 되지 않은 파일이 있습니다." + Environment.NewLine;
        message += "해당 파일을 무시하려면 .gitignore 에 추가하세요." + Environment.NewLine;
        message += string.Join(Environment.NewLine, GetUntrackedFileList());
        throw new GiccException(message);
      }

      if (GetModifiedFileList().Count > 0)
      {
        string message = "변경된 후 commit 되지 않은 파일이 있습니다." + Environment.NewLine;
        message += string.Join(Environment.NewLine, GetModifiedFileList());
        throw new GiccException(message);
      }
    }

    internal void AddCommit(string message, string author)
    {
      Execute("add --all .");
      Commit(message, author);
    }

    internal void AddCommit(string message, string author, string date)
    {
      Execute("add --all .");
      this.Commit(message, author, date);
    }

    internal void Commit(string message, string author)
    {
      this.Commit(message, author, DateTime.Now.ToString());
    }

    internal void Commit(string message, string author, string date)
    {
      Execute("commit --author='" + author + "' --date='" + date + "' -am '" + message + "'");
    }

    internal void TagPull()
    {
      Execute("tag -f gicc_pull");
    }

    internal void TagPush()
    {
      Execute("tag -f gicc_push");
    }

    internal void Checkout(string branch)
    {
      if (!GetBranchList().Any(existBranch => existBranch.Contains(branch)))
      {
        Execute("checkout -b " + branch);
      }

      Execute("checkout " + branch);
    }

    internal string Init()
    {
      if (!Directory.Exists(RepoPath))
      {
        Directory.CreateDirectory(RepoPath);
      }

      return GetExecutedResult("init");
    }

    internal string Help()
    {
      return GetExecutedResult("help");
    }

    internal string GetHeadCommitId()
    {
      return GetExecutedResult("git rev-list head -1");
    }

    /// <summary>
    /// Get last gicc_push or gicc_pull tagged commit
    /// </summary>
    /// <returns></returns>
    internal string GetLastPPCommit()
    {
      return GetExecutedResultList("rev-list gicc_push gicc_pull")[0];
    }

    /// <summary>
    /// Get committed files after last push or pull
    /// </summary>
    /// <returns></returns>
    internal Dictionary<string, FileChangeType> GetCommittedFilesAfterLastPP()
    {
      List<string> changedFileList = GetExecutedResultList("diff --name-status " + GetLastPPCommit());
      Dictionary<string, FileChangeType> resultDic = new Dictionary<string, FileChangeType>();

      foreach (string changedFile in changedFileList)
      {
        FileChangeType changeType;
        string[] splitedArr = changedFile.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        switch (splitedArr[0])
        {
          case "A":
            changeType = FileChangeType.Creation;
            break;
          case "M":
            changeType = FileChangeType.Modification;
            break;
          case "D":
            changeType = FileChangeType.Delete;
            break;
          default:
            throw new InvalidOperationException("Git 파일 변경 사항은 추가, 수정, 삭제 중 하나여야 합니다.");
        }

        resultDic.Add(splitedArr[1].Replace('/', '\\'), changeType);
      }

      return resultDic;
    }
  }
}
