using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc
{
	public class Gicc
	{
		public static void SetConfig(string vobPath, string branchName, string repoPath)
		{
			List<string> config = new List<string>(new string[]{
				@"vob = " + vobPath
				, @"branch = " + branchName
				, @"repository = " + repoPath}
				);
			IOHandler.WriteConfig(config);
		}

		public void Pull()
		{
			List<CCElementVersion> ccHistory = null;

			CheckAnyFileIsNotCheckedOut();
			CheckAllSymbolicLinksAreMounted();
			CheckModifiedFileIsNotExist();

			List<string> branchFileList = ClearCase.FindAllFilesInBranch(IOHandler.VobPath, IOHandler.BranchName);
			branchFileList.ForEach(file => ccHistory.AddRange(ClearCase.Lshistory(file)));

			List<DateTime> commitPoints = GetCommitPoints(ccHistory);
			for (int i = 0; i < commitPoints.Count - 2; i++)
				Pull(ccHistory, commitPoints[i], commitPoints[i + 1]);
		}

		private void Pull(List<CCElementVersion> ccHistory, DateTime since, DateTime until)
		{
			Git git = new Git(IOHandler.RepoPath);
			string author = "gicc <gicc@test.test>"; // todo : implement

			// main -> master
			ClearCase.SetBranchCS("master", until);
			List<string> mainFileList = ClearCase.FindAllFilesInBranch(IOHandler.VobPath, "main", since, until);
			
			foreach (string relativeFilePath in mainFileList)
			{
				if (!git.IsIgnored(relativeFilePath))
					IOHandler.Copy(System.IO.Path.Combine(IOHandler.VobPath,relativeFilePath), 
						System.IO.Path.Combine(IOHandler.RepoPath, relativeFilePath));
			}
			
			git.Checkout("master");
			git.AddCommit("gicc", author);

			// vob branch -> git branch
			ClearCase.SetBranchCS(IOHandler.BranchName, until);

			List<string> branchFileList = ccHistory
				.Where(elemVer => elemVer.CreatedDate > since && elemVer.CreatedDate <= until)
				.Select(elemVer => elemVer.AbsoluteFilePath).ToList()
				.Distinct().ToList();

			foreach (string absFilePath in branchFileList)
			{
				string relativeFilePath = IOHandler.EliminateVobPath(absFilePath);
				if (!git.IsIgnored(relativeFilePath))
					IOHandler.Copy(absFilePath, System.IO.Path.Combine(IOHandler.RepoPath, relativeFilePath));
			}

			git.Checkout(IOHandler.BranchName);
			git.AddCommit("gicc", author);
		}

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
					&& orderedHistory[i].OwnerFullName == orderedHistory[i + 1].OwnerFullName)
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

		internal void CheckAnyFileIsNotCheckedOut()
		{
			List<string> checkedoutFileList = ClearCase.LscheckoutInCurrentViewByLoginUser();
			if (checkedoutFileList.Count > 0)
			{
				string message =
					"체크아웃 된 파일이 있습니다." + Environment.NewLine
					+ string.Join(Environment.NewLine, checkedoutFileList);
				throw new GiccException(message);
			}
		}

		internal void CheckAllSymbolicLinksAreMounted()
		{
			List<CCElementVersion> slinkList = ClearCase.FindAllSymbolicLinks();

			foreach (CCElementVersion link in slinkList)
			{
				if (!System.IO.Directory.Exists(link.SymbolicLink))
					throw new GiccException(link.SymbolicLink + "  VOB 이 mount 되지 않았습니다.");
			}
		}

		internal void CheckModifiedFileIsNotExist()
		{
			List<string> untrackedFileList = new Git(IOHandler.RepoPath).GetUntrackedFileList();
			if (untrackedFileList.Count > 0)
			{
				string message = "새로 추가된 후 commit 되지 않은 파일이 있습니다." + Environment.NewLine;
				message += string.Join(Environment.NewLine, untrackedFileList);
				throw new GiccException(message);
			}

			List<string> modifiedFileList = new Git(IOHandler.RepoPath).GetModifiedFileList();
			if (modifiedFileList.Count > 0)
			{
				string message = "변경된 후 commit 되지 않은 파일이 있습니다." + Environment.NewLine;
				message += string.Join(Environment.NewLine, modifiedFileList);
				throw new GiccException(message);
			}
		}
	}
}
