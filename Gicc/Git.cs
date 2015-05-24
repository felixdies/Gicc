using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace Gicc
{
  public class Git : Executor
  {
		public Git(string executingPath, string giccPath)
			: base(executingPath, giccPath + @"\gitout", giccPath + @"\log") { }

		public Git(string executingPath, string giccPath, string branchName)
			: base(executingPath, giccPath + @"\gitout", giccPath + @"\log")
		{
			this.BranchName = branchName;
		}

		public Git(string executingPath, string outPath, string logPath, string branchName)
			: base(executingPath, outPath, logPath)
		{
			this.BranchName = branchName;
		}

		internal string RepositoryPath
		{
			get { return GetExecutedResult("rev-parse --show-toplevel"); }
		}

		internal string CurrentBranch
		{
			get { return GetExecutedResult("rev-parse --abbrev-ref HEAD"); }
		}

		internal List<string> UntrackedFileList
		{
			get { return GetExecutedResultList("ls-files --others"); }
		}

		internal List<string> ModifiedFileList
		{
			get { return GetExecutedResultList("diff --name-only"); }
		}

		internal List<string> BranchList
		{
			get { return GetExecutedResultList("branch"); }
		}

		internal DateTime LastGiccPull
		{
			get
			{
				string lastPulledCommit = GetExecutedResult("show-ref --tags gicc_pull");
				string lastGiccPullTime = GetExecutedResult("git show -s --format=%ci " + lastPulledCommit.Substring(0, 10));
				return DateTime.Parse(lastGiccPullTime);
			}
		}

		internal void CheckModifiedFileIsNotExist()
		{
			List<string> untrackedFileList = UntrackedFileList;
			if (untrackedFileList.Count > 0)
			{
				string message = "새로 추가된 후 commit 되지 않은 파일이 있습니다." + Environment.NewLine;
				message += string.Join(Environment.NewLine, untrackedFileList);
				throw new GiccException(message);
			}

			List<string> modifiedFileList = ModifiedFileList;
			if (modifiedFileList.Count > 0)
			{
				string message = "변경된 후 commit 되지 않은 파일이 있습니다." + Environment.NewLine;
				message += string.Join(Environment.NewLine, modifiedFileList);
				throw new GiccException(message);
			}
		}

		internal string Help()
		{
			return GetExecutedResult("help");
		}

		internal void AddCommit(string message, string author)
		{
			Execute("add --all .");
			Commit(message, author);
		}

		internal void AddCommit(string message, string author, string date)
		{
			Execute("add --all .");
			Commit(message, author, date);
		}

		internal void Commit(string message, string author)
		{
			Commit(message, author, DateTime.Now.ToString());
		}

		internal void Commit(string message, string author, string date)
		{
			Execute("commit --author='" + author + "' --date='" + date + "' -am '" + message + "'");
		}

		internal void TagPull()
		{
			Execute("tag -f gicc_pull");
		}

		internal bool IsIgnored(string fileName)
		{
			return GetExecutedResultList("check-ignore " + fileName).Count > 0;
		}

		internal void Checkout(string branch)
		{
			if (!BranchList.Any(existBranch => existBranch.Contains(branch)))
				Execute("checkout -b " + branch);

			Execute("checkout " + branch);
		}

		protected override string Command
		{
			get { return "git"; }
		}

		protected override void ValidateBeforeExecution()
		{
			if (!File.Exists(OutPath))
				throw new GiccException("출력 경로를 찾을 수 없습니다. 현재 위치가 Local 저장소의 최상위 폴더가 맞는 지 확인 해 주세요.");
		}
  }
}
