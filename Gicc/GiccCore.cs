using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc.Lib
{
  public class GiccCore
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="GiccCore" /> class.
    /// Clone 이외 명령어 실행 시 호출되는 생성자. Config 파일에서 branch name, CC VOB path, git repository path 를 읽어 온다.
    /// </summary>
    /// <param name="cwd"></param>
    public GiccCore(string cwd)
    {
      this.CWD = cwd;

      ParseAllConfigsFromConfigFile();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GiccCore" /> class.
    /// Clone 명령어 실행 시 호출되는 생성자.
    /// </summary>
    /// <param name="cwd"></param>
    /// <param name="absVobPath"></param>
    /// <param name="branchName"></param>
    /// <param name="absRepoPath"></param>
    public GiccCore(string cwd, string absVobPath, string branchName, string absRepoPath)
    {
      this.CWD = cwd;
      this.VobPath = absVobPath;
      this.BranchName = branchName;
      this.RepoPath = absRepoPath;
    }

    internal string CWD { get; set; }

    internal string GiccPath
    {
      get
      {
        return Path.Combine(CWD, @".git\gicc");
      }
    }

    internal string ConfigPath
    {
      get
      {
        return Path.Combine(GiccPath, "config");
      }
    }

    /// <summary>
    /// Gets or sets ClearCase and Git's branch name.
    /// Clone 호출 시 생성자 매개변수로 주어지고, Pull 또는 Push 호출 시 config 파일에서 읽어 온다.
    /// </summary>
    internal string BranchName { get; set; }

    /// <summary>
    /// Gets or sets ClearCase's absolute VOB path.
    /// Clone 호출 시 생성자 매개변수로 주어지고, Pull 또는 Push 호출 시 config 파일에서 읽어 온다.
    /// </summary>
    internal string VobPath { get; set; }

    /// <summary>
    /// Gets or sets Git's absolute repository path.
    /// Clone 호출 시 생성자 매개변수로 주어지고, Pull 또는 Push 호출 시 config 파일에서 읽어 온다.
    /// </summary>
    internal string RepoPath { get; set; }

    public void Clone()
    {
      Git git = new Git(CreateGitInfo());

      git.Init();

      // move to git repository.
      CWD = git.RepoPath;

      WriteConfig();

      CopyMainBranchBeforeFirstBranchCheckin(BranchName);
      
      // todo : pull tag
      
      // todo : pull
    }

    public void Pull()
    {
      Git git = new Git(CreateGitInfo());
      ClearCase cc = new ClearCase(CreateCCInfo(this.BranchName));
      List<CCElementVersion> ccHistory = new List<CCElementVersion>();

      ////cc.CheckAllSymbolicLinksAreMounted(); // symbolic link 는 nuget 으로 관리
      cc.CheckCheckedoutFileIsNotExist();
      git.CheckModifiedFileIsNotExist();

      cc.FindAllFilesInBranch()
        .ForEach(file => ccHistory.AddRange(cc.Lshistory(file)));

      List<DateTime> commitPoints = GetCommitPoints(ccHistory);
      for (int i = 0; i < commitPoints.Count - 2; i++)
      {
        CopyAndCommit(ccHistory, commitPoints[i], commitPoints[i + 1]); // todo: pull tag 가 since 가 돼야 함
      }

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
    /// Config 파일에 VobPath, BranchName, RepoPath 속성을 기록한다.
    /// </summary>
    internal void WriteConfig()
    {
      string[] configArr = new string[]
      {
        "vob = " + VobPath,
        "branch = " + BranchName,
        "repository = " + RepoPath
      };

      File.WriteAllLines(ConfigPath, configArr);
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
      CopyFilesFromVOBToRepo(mainFileList);

      git.AddCommit("gicc", author);
      git.TagPull(); // todo : if changed

      // vob branch -> git branch
      branchCC.SetBranchCS(until);
      git.Checkout(BranchName);

      List<string> branchFileList = ccHistory
        .Where(elemVer => elemVer.CreatedDate > since && elemVer.CreatedDate <= until) // todo : pull 에서 날짜 제한 걸어주면 필요 없을 듯
        .Select(elemVer => elemVer.ElementName).ToList()
        .Distinct().ToList();
      CopyFilesFromVOBToRepo(branchFileList);

      git.AddCommit("gicc", author);
      git.TagPull(); // todo : if changed
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

    /// <summary>
    /// 대상 브렌치에서 첫 번째 체크인이 일어나기 직전 상태의 메인 브렌치 파일들을 복사합니다.
    /// </summary>
    private void CopyMainBranchBeforeFirstBranchCheckin(string branchName)
    {
      ClearCase main = new ClearCase(CreateCCInfo("main"));
      ClearCase branch = new ClearCase(CreateCCInfo(branchName));
      
      List<CCElementVersion> branchVerList = branch.GetAllVersionsInBranch();
      branchVerList.Sort(CCElementVersion.CompareVersionsByCreatedDate);

      DateTime firstCheckinDate = branchVerList[0].CreatedDate;

      main.SetMainCS(firstCheckinDate.AddSeconds(-1));

      CopyVOBIntoRepo();
    }
    
    /// <summary>
    /// VOB 의 파일들을 Repository 로 복사 합니다. gitignore 에 있는 파일들은 복사하지 않습니다.
    /// </summary>
    private void CopyVOBIntoRepo()
    {
      Git git = new Git(CreateGitInfo());

      throw new NotImplementedException();
    }

    private void CopyFilesFromVOBToRepo(List<string> mainFileList)
    {
      Git git = new Git(CreateGitInfo());

      foreach (string relativeFilePath in mainFileList)
      {
        ////if (!git.IsIgnored(relativeFilePath))
        ////File.Copy(Path.Combine(VobPath, relativeFilePath), Path.Combine(RepoPath, relativeFilePath), true);
      }
    }

    private void ParseAllConfigsFromConfigFile()
    {
      if (!File.Exists(ConfigPath))
      {
        throw new GiccException("Gicc 설정 파일을 찾을 수 없습니다. 현재 위치가 Local 저장소의 최상위 폴더가 맞는 지 확인 해 주세요.");
      }

      VobPath = ParseConfigFromConfigFile("vob");
      BranchName = ParseConfigFromConfigFile("branch");
      RepoPath = ParseConfigFromConfigFile("repository");
    }

    private string ParseConfigFromConfigFile(string configName)
    {
      return File.ReadAllLines(ConfigPath).ToList().Find(config => config.ToLower().StartsWith(configName)).Split('=').Last().Trim();
    }

    /// <summary>
    /// git 실행 정보.
    /// git 실행 경로는 언제나 local repository 이다.
    /// </summary>
    private GitConstructInfo CreateGitInfo()
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
    /// <param name="branchName">브랜치 이름입니다. main 또는 작업 중 branch 가 됩니다.</param>
    private ClearCaseConstructInfo CreateCCInfo(string branchName)
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
  }
}
