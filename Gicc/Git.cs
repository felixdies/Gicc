using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace Gicc
{
  class Git : Executor
  {
		public Git(GitConstructInfo constructInfo)
			: base(constructInfo)
		{
			this.RepoPath = constructInfo.RepoPath;
		}

		string RepoPath { get; set; }

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

				if (string.IsNullOrEmpty(lastPulledCommit)) // not tagged yet
					return new DateTime(1990, 1, 1);

				string lastGiccPullTime = GetExecutedResult("show -s --format=%ai " + lastPulledCommit.Substring(0, 10));
				return DateTime.Parse(lastGiccPullTime);
			}
		}

		internal void CheckModifiedFileIsNotExist()
		{
			if (UntrackedFileList.Count > 0)
			{
				string message = "새로 추가된 후 commit 되지 않은 파일이 있습니다." + Environment.NewLine;
				message += string.Join(Environment.NewLine, UntrackedFileList);
				throw new GiccException(message);
			}

			if (ModifiedFileList.Count > 0)
			{
				string message = "변경된 후 commit 되지 않은 파일이 있습니다." + Environment.NewLine;
				message += string.Join(Environment.NewLine, ModifiedFileList);
				throw new GiccException(message);
			}
		}

		internal string Init()
		{
			return GetExecutedResult("init");
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

		internal List<bool> IsIgnoredList(List<string> fileNameList)
		{
			List<string> argList = fileNameList.Select(filename => "check-ignore " + filename).ToList();
			List<string> ExecuteResultList = GetExecutedResultListWithoutFIO(argList);
			
			// result is not empty if the file is ignored
			return ExecuteResultList.Select(result => !string.IsNullOrWhiteSpace(result)).ToList();
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
  }
}
