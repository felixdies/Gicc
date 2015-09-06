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

    private GitIgnore _gitIgnore = null;

    internal GitIgnore GitIgnore
    {
      get
      {
        if (_gitIgnore == null)
        {
          string[] gitIgnoreTextArr = File.ReadAllLines(Path.Combine(RepoPath, ".gitignore"));
          _gitIgnore = new GitIgnore(gitIgnoreTextArr);
        }

        return _gitIgnore;
      }
    }

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
      cc.CheckCheckedoutFileNotExistsInCurrentView();
      git.CheckModifiedFileIsNotExist();

      cc.FindAllFilesInBranch()
        .ForEach(file => ccHistory.AddRange(cc.Lshistory(file)));

      List<DateTime> commitPoints = GetCommitPoints(ccHistory);
      for (int i = 0; i < commitPoints.Count - 2; i++)
      {
        CopyAndCommit(ccHistory, commitPoints[i], commitPoints[i + 1]); // todo: pull/push tag 가 since 가 돼야 함
      }

      throw new NotImplementedException();
    }

    /// <summary>
    /// git repository 의 작업사항을 cc VOB 에 push 합니다.
    /// </summary>
    public void Push()
    {
      Git git = new Git(CreateGitInfo());
      ClearCase cc = new ClearCase(CreateCCInfo(this.BranchName));

      Dictionary<string, FileChangeType> committedFileDic = git.GetCommittedFilesAfterLastPP();

      // 1. validateion
      git.CheckModifiedFileIsNotExist();
      cc.CheckCheckoutNotExists(committedFileDic.Keys.ToList());

      // 2. pull & merge

      // 3. copy commited files after last pull/push tag & checkin
      CopyAndCheckin(committedFileDic,
        "checked in with gicc" + Environment.NewLine
        + "-git commit id : " + git.GetHeadCommitId());

      // 4. tag "push"
      git.TagPush();
    }

    internal void CopyAndCheckin(Dictionary<string, FileChangeType> committedFileDic, string checkInComment)
    {
      ClearCase cc = new ClearCase(CreateCCInfo(this.BranchName));

      foreach (KeyValuePair<string, FileChangeType> kvp in committedFileDic)
      {
        switch (kvp.Value)
        {
          case FileChangeType.Creation:
            File.Copy(Path.Combine(RepoPath, kvp.Key), Path.Combine(VobPath, kvp.Key), false);
            cc.CheckIn(Path.Combine(VobPath, kvp.Key), checkInComment);
            break;
          case FileChangeType.Modification:
            cc.Checkout(kvp.Key);
            File.Copy(Path.Combine(RepoPath, kvp.Key), Path.Combine(VobPath, kvp.Key), true);
            cc.CheckIn(Path.Combine(VobPath, kvp.Key), checkInComment);
            break;
          case FileChangeType.Delete:
            // Will not implement.
            break;
          default:
            throw new InvalidOperationException("Git 파일 변경 사항은 추가, 수정, 삭제 중 하나여야 합니다.");
        }
      }
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
      CopyFiles(mainFileList, VobPath, RepoPath);

      git.AddCommit("gicc", author);
      git.TagPull(); // todo : if changed

      // vob branch -> git branch
      branchCC.SetBranchCS(until);
      git.Checkout(BranchName);

      List<string> branchFileList = ccHistory
        .Where(elemVer => elemVer.CreatedDate > since && elemVer.CreatedDate <= until) // todo : pull 에서 날짜 제한 걸어주면 필요 없을 듯
        .Select(elemVer => elemVer.ElementName).ToList()
        .Distinct().ToList();
      CopyFiles(branchFileList, VobPath, RepoPath);

      git.AddCommit("gicc", author);
      git.TagPull(); // todo : if changed
    }

    private void CopyFiles(List<string> fileList, string srcRootPath, string destRootPath)
    {
      foreach (string absFilePath in fileList)
      {
        string relFilePath = MakeRelative(absFilePath, srcRootPath);

        if (GitIgnore.IsIgnoredFile(relFilePath) == false)
        {
          File.Copy(absFilePath, Path.Combine(destRootPath, relFilePath), true);
        }
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

      CopyDirectory(VobPath, RepoPath);
    }

    /// <summary>
    /// sourceDirName 의 하위 폴더 및 파일을 destDirName 로 복사 합니다. gitignore 에 있는 파일들은 복사하지 않습니다.
    /// </summary>
    private void CopyDirectory(string sourceDirName, string destDirName)
    {
      // Get the subdirectories for the specified directory.
      DirectoryInfo dir = new DirectoryInfo(sourceDirName);
      DirectoryInfo[] dirs = dir.GetDirectories();

      if (!dir.Exists)
      {
        throw new DirectoryNotFoundException(
            "Source directory does not exist or could not be found: "
            + sourceDirName);
      }

      if (GitIgnore.IsIgnoredDir(MakeRelative(sourceDirName)))
      {
        return;
      }

      // If the destination directory doesn't exist, create it. 
      if (!Directory.Exists(destDirName))
      {
        Directory.CreateDirectory(destDirName);
      }

      // Get the files in the directory and copy them to the new location.
      FileInfo[] files = dir.GetFiles();
      foreach (FileInfo file in files)
      {
        string temppath = Path.Combine(destDirName, file.Name);
        if (GitIgnore.IsIgnoredFile(MakeRelative(file.FullName)) == false)
        {
          file.CopyTo(temppath, true);
        }
      }

      // If copying subdirectories, copy them and their contents to new location. 
      foreach (DirectoryInfo subdir in dirs)
      {
        string temppath = Path.Combine(destDirName, subdir.Name);
        CopyDirectory(subdir.FullName, temppath);
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

    /// <summary>
    /// 주어진 path 에 VOB/Repo path 가 포함 된 경우, VOB/Repo 로부터의 상대 경로를 반환 합니다.
    /// 주어진 path 에 VOB/Repo path 가 포함되지 않은 경우, 주어진 path 를 그대로 반환 합니다.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal string MakeRelative(string path)
    {
      Uri uri = new Uri(path);
      Uri vobUri = new Uri(VobPath + "\\");
      Uri repoUri = new Uri(RepoPath + "\\");

      bool b = vobUri.IsFile;

      if (vobUri.IsBaseOf(uri))
      {
        return MakeRelative(path, VobPath);
      }
      else if (repoUri.IsBaseOf(uri))
      {
        return MakeRelative(path, RepoPath);
      }
      else
      {
        return path;
      }
    }

    private string MakeRelative(string filePath, string rootPath)
    {
      Uri fileUri = new Uri(filePath);
      Uri rootUri = new Uri(rootPath);
      return rootUri.MakeRelativeUri(fileUri).ToString();
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
