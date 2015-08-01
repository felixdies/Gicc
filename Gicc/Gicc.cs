using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Gicc
{
  public class Gicc
  {
    /// <summary>
    /// Clone 이외 명령어 실행 시 호출되는 생성자
    /// </summary>
    /// <param name="cwd"></param>
    /// <param name="parseConfigs">
    /// </param>
    public Gicc(string cwd)
    {
      this.CWD = cwd;

      ParseAllConfigsFromConfigFile();
    }

    /// <summary>
    /// Clone 명령어 실행 시 호출되는 생성자
    /// </summary>
    /// <param name="absVobPath"></param>
    /// <param name="branchName"></param>
    /// <param name="absRepoPath"></param>
    public Gicc(string cwd, string absVobPath, string branchName, string absRepoPath)
    {
      this.CWD = cwd;
      this.VobPath = absVobPath;
      this.BranchName = branchName;
      this.RepoPath = absRepoPath;
    }

    internal string CWD { get; set; }
    internal string GiccPath { get { return Path.Combine(CWD, @".git\gicc"); } }
    internal string ConfigPath { get { return Path.Combine(GiccPath, "config"); } }

    #region Clone, Pull, Push 실행 필수 정보
    // Clone : 매개변수로 주어짐.
    // Pull, Push : config 파일에서 읽어 옴

    internal string VobPath { get; set; }
    internal string BranchName { get; set; }
    internal string RepoPath { get; set; }

    void ParseAllConfigsFromConfigFile()
    {
      if (!File.Exists(ConfigPath))
        throw new GiccException("Gicc 설정 파일을 찾을 수 없습니다. 현재 위치가 Local 저장소의 최상위 폴더가 맞는 지 확인 해 주세요.");

      VobPath = ParseConfigFromConfigFile("vob");
      BranchName = ParseConfigFromConfigFile("branch");
      RepoPath = ParseConfigFromConfigFile("repository");
    }

    string ParseConfigFromConfigFile(string configName)
    {
      return File.ReadAllLines(ConfigPath).ToList().Find(config => config.ToLower().StartsWith(configName)).Split('=').Last().Trim();
    }

    #endregion Clone, Pull, Push 실행 필수 정보

    #region Executor 생성자 정보

    /// <summary>
    /// git 실행 정보.
    /// git 실행 경로는 언제나 local repository 이다.
    /// </summary>
    GitConstructInfo CreateGitInfo()
    {
      return new GitConstructInfo()
      {
        RepoPath = this.RepoPath,
        BranchName = this.BranchName,
        ExecutingPath = this.RepoPath,
        OutPath = Path.Combine(this.GiccPath, "gitout"),
        LogPath = Path.Combine(this.GiccPath, "log")
      };
    }

    /// <summary>
    /// cc 실행 정보.
    /// cleartool 실행 경로는 언제나 cc 의 VOB path 이다.
    /// </summary>
    ClearCaseConstructInfo CreateCCInfo(string branchName)
    {
      return new ClearCaseConstructInfo()
      {
        VobPath = this.VobPath,
        BranchName = branchName,
        ExecutingPath = this.VobPath,
        OutPath = Path.Combine(this.GiccPath, "ccout"),
        LogPath = Path.Combine(this.GiccPath, "log")
      };
    }

    #endregion Executor 생성자 정보

    public void Clone()
    {
      // todo : init git
      WriteConfig();
      // todo : 최초 브랜치 checkin 지점 직전 snapshot 복사
      // todo : pull tag
      // todo : pull

      throw new NotImplementedException();
    }

    public void Pull()
    {
      Git git = new Git(CreateGitInfo());
      ClearCase cc = new ClearCase(CreateCCInfo(this.BranchName));
      List<CCElementVersion> ccHistory = null;

      cc.CheckAllSymbolicLinksAreMounted();
      cc.CheckCheckedoutFileIsNotExist();
      git.CheckModifiedFileIsNotExist();

      List<string> branchFileList = cc.FindAllFilesInBranch();
      branchFileList.ForEach(file => ccHistory.AddRange(cc.Lshistory(file)));

      List<DateTime> commitPoints = GetCommitPoints(ccHistory);
      for (int i = 0; i < commitPoints.Count - 2; i++)
        CopyAndCommit(ccHistory, commitPoints[i], commitPoints[i + 1]); // todo: pull tag 가 since 가 돼야 함

      throw new NotImplementedException();
    }

    public void Push()
    {
      throw new NotImplementedException();
    }

    public List<string> ListCCFilesOnBranch(string branchName)
    {
      this.BranchName = branchName;
      ClearCase cc = new ClearCase(CreateCCInfo(branchName));

      return cc.FindAllFilesInBranch();
    }

    /// <summary>
    /// Config 파일에 VobPath, BranchName, RepoPath 속성을 기록
    /// </summary>
    internal void WriteConfig()
    {
      File.WriteAllLines(ConfigPath,
        new string[]{
					"vob = " + VobPath
				, "branch = " + BranchName
				, "repository = " + RepoPath});
    }

    internal void CopyAndCommit(List<CCElementVersion> ccHistory, DateTime since, DateTime until)
    {
      Git git = new Git(CreateGitInfo());
      ClearCase mainCC = new ClearCase(CreateCCInfo("main"));
      ClearCase branchCC = new ClearCase(CreateCCInfo(this.BranchName));
      string author = "gicc <gicc@test.test>"; // todo : implement

      // main -> master
      mainCC.SetBranchCS(until);
      git.Checkout("master");

      List<string> mainFileList = mainCC.FindAllFilesInBranch(since, until);
      CopyFromVOBToRepo(mainFileList);

      git.AddCommit("gicc", author);
      git.TagPull(); // todo : if changed

      // vob branch -> git branch
      branchCC.SetBranchCS(until);
      git.Checkout(BranchName);

      List<string> branchFileList = ccHistory
        .Where(elemVer => elemVer.CreatedDate > since && elemVer.CreatedDate <= until) // todo : pull 에서 날짜 제한 걸어주면 필요 없을 듯
        .Select(elemVer => elemVer.ElementName).ToList()
        .Distinct().ToList();
      CopyFromVOBToRepo(branchFileList);

      git.AddCommit("gicc", author);
      git.TagPull(); // todo : if changed
    }

    private void CopyFromVOBToRepo(List<string> mainFileList)
    {
      Git git = new Git(CreateGitInfo());

      foreach (string relativeFilePath in mainFileList)
      {
        //if (!git.IsIgnored(relativeFilePath))
        //File.Copy(Path.Combine(VobPath, relativeFilePath), Path.Combine(RepoPath, relativeFilePath), true);
      }
    }

    /// <summary>
    /// 매개변수로 전달 된 cc history 를 snapshot 으로 만들기 위해, commit points 를 잡는다.
    /// 동일 브랜치 내에서 한 명의 사용자가 연속으로 check in 한 기록이 하나의 snapshot 이 된다.
    /// </summary>
    /// <param name="ccHistory"></param>
    /// <returns></returns>
    internal List<DateTime> GetCommitPoints(List<CCElementVersion> ccHistory)
    {
      List<DateTime> resultCommitPoints = new List<DateTime>();
      List<CCElementVersion> orderedHistory = ccHistory.OrderBy(x => x.CreatedDate).ToList();

      for (int i = 0; i < orderedHistory.Count; i++)
      {
        if (i == orderedHistory.Count - 1)
        {
          resultCommitPoints.Add(orderedHistory[i].CreatedDate);
          break;
        }

        if (orderedHistory[i].Branch == orderedHistory[i + 1].Branch
          && orderedHistory[i].OwnerFullName == orderedHistory[i + 1].OwnerFullName
          && orderedHistory[i].OperationGroup == orderedHistory[i + 1].OperationGroup)
        {
          continue;
        }
        else
        {
          resultCommitPoints.Add(orderedHistory[i].CreatedDate);
        }
      }

      return resultCommitPoints;
    }
  }
}
