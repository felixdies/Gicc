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
			List<CCElementVersion> ccHistory = null;  // foreach FindAllFilesInBranch() - Add LsHistory()

			List<DateTime> commitPoints = GetCommitPoints(ccHistory);

			for (int i = 0; i < commitPoints.Count - 2; i++)
				Pull(commitPoints[i], commitPoints[i + 1]);
		}

		public void Pull(DateTime since, DateTime until)
		{
			CheckAnyFileIsNotCheckedOut();
			CheckAllSymbolicLinksAreMounted();

			CheckModifiedFileIsNotExist();

			ClearCase.SetDefaultCS();
			new Git(IOHandler.RepoPath).Checkout("master");
			new Git(IOHandler.RepoPath).Execute("status >" + IOHandler.GitoutPath);
			// copy vob >> repo
			// master commit

			ClearCase.SetBranchCS(IOHandler.BranchName);
			new Git(IOHandler.RepoPath).Checkout(IOHandler.BranchName);
			new Git(IOHandler.RepoPath).Execute("status >" + IOHandler.GitoutPath);
			// copy vob >> repo
			// user_branch commit
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
