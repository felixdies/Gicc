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
		string CWD { get; set; }
		string GiccPath
		{
			get
			{
				return Path.Combine(CWD, @".git\gicc");
			}
		}
		string ConfigPath
		{
			get
			{
				return Path.Combine(GiccPath, "config");
			}
		}
		string CCoutPath
		{
			get { return Path.Combine(GiccPath, "ccout"); }
		}
		string GitoutPath
		{
			get { return Path.Combine(GiccPath, "gitout"); }
		}
		string VobPath { get; set; }
		string BranchName { get; set; }
		string RepoPath { get; set; }

		public Gicc()
		{
			CWD = Environment.CurrentDirectory;
			ParseAllConfigs();
		}

		public Gicc(string cwd)
		{
			this.CWD = cwd;
			ParseAllConfigs();
		}

		public void WriteConfig(string vobPath, string branchName, string repoPath)
		{
			string[] config = new string[]{
				"vob = " + vobPath
				, "branch = " + branchName
				, "repository = " + repoPath};

			File.WriteAllLines(ConfigPath, config);
		}
		
		void ParseAllConfigs()
		{
			if(!File.Exists(ConfigPath))
				return;

			VobPath = ParseConfig("vob");
			BranchName = ParseConfig("branch");
			RepoPath = ParseConfig("repository");
		}

		string ParseConfig(string configName)
		{
			return File.ReadAllLines(ConfigPath).ToList().Find(config => config.ToLower().StartsWith(configName)).Split('=').Last().Trim();
		}

		public void Pull()
		{
			List<CCElementVersion> ccHistory = null;
			ClearCase cc = new ClearCase(VobPath, BranchName);

			cc.CheckCheckedoutFileIsNotExist();
			cc.CheckAllSymbolicLinksAreMounted();
			cc.CheckModifiedFileIsNotExist();

			List<string> branchFileList = cc.FindAllFilesInBranch();
			branchFileList.ForEach(file => ccHistory.AddRange(new ClearCase(VobPath).Lshistory(file)));

			List<DateTime> commitPoints = GetCommitPoints(ccHistory);
			for (int i = 0; i < commitPoints.Count - 2; i++)
				CopyAndCommit(ccHistory, commitPoints[i], commitPoints[i + 1]);
		}

		internal void CopyAndCommit(List<CCElementVersion> ccHistory, DateTime since, DateTime until)
		{
			Git git = new Git(IOHandler.RepoPath);
			string author = "gicc <gicc@test.test>"; // todo : implement

			// main -> master
			ClearCase mainCC = new ClearCase(VobPath, "main");
			mainCC.SetBranchCS(until);
			git.Checkout("master");
			
			List<string> mainFileList = mainCC.FindAllFilesInBranch(since, until);
			foreach (string relativeFilePath in mainFileList)
			{
				if (!git.IsIgnored(relativeFilePath))
					IOHandler.Copy(System.IO.Path.Combine(IOHandler.VobPath,relativeFilePath), 
						System.IO.Path.Combine(IOHandler.RepoPath, relativeFilePath));
			}
			git.AddCommit("gicc", author);
			git.TagPull();

			// vob branch -> git branch
			ClearCase branchCC = new ClearCase(VobPath, BranchName);
			branchCC.SetBranchCS(until);
			git.Checkout(IOHandler.BranchName);

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

			git.AddCommit("gicc", author);
			git.TagPull();
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
	}
}
