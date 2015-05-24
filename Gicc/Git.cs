using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace Gicc
{
  public class Git
  {
		public Git(string repositoryPath)
		{
			this.RepositoryPath = repositoryPath;
		}

		public string RepositoryPath { get; set; }

		public string LogPath
		{
			get { return Path.Combine(RepositoryPath, @".git/gicc/log"); }
		}

		string Command
		{
			get { return "git "; }
		}

		internal string Help()
		{
			Execute("help >" + IOHandler.GitoutPath);
			return IOHandler.ReadGitout()[0];
		}

		internal void AddCommit(string message, string author, string date)
		{
			Execute("add --all .");
			Commit(message, author, date);
		}
		
		internal void AddCommit(string message, string author)
		{
			Execute("add --all .");
			Commit(message, author);
		}

		internal void Commit(string message, string author, string date)
		{
			Execute("commit --author='" + author + "' --date='" + date + "' -am '" + message + "'");
		}

		internal void Commit(string message, string author)
		{
			Commit(message, author, DateTime.Now.ToString());
		}

		internal List<string> GetUntrackedFileList()
		{
			Execute("ls-files --others >" + IOHandler.GitoutPath);
			return IOHandler.ReadGitout();
		}

		internal List<string> GetModifiedFileList()
		{
			Execute("diff --name-only >" + IOHandler.GitoutPath);
			return IOHandler.ReadGitout();
		}

		internal List<string> GetAllBranches()
		{
			Execute("branch >" + IOHandler.GitoutPath);
			return IOHandler.ReadGitout();
		}

		internal bool IsIgnored(string fileName)
		{
			Execute("check-ignore " + fileName + " >" + IOHandler.GitoutPath);
			return IOHandler.ReadGitout().Count > 0;
		}

		internal void Checkout(string branch)
		{
			List<string> allBranches = GetAllBranches();
			bool alreadyExists = allBranches.Any(existBranch => existBranch.Contains(branch));

			if (!alreadyExists)
				Execute("checkout -b " + branch);

			Execute("checkout " + branch);
		}

		internal void Execute(string arg, bool wait = true)
		{
			Process gitProcess = new Process();

			ProcessStartInfo proInfo = new ProcessStartInfo()
			{
				WorkingDirectory = RepositoryPath,
				FileName = @"powershell",
				Arguments = Command + arg,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true
			};

			gitProcess.StartInfo = proInfo;
			gitProcess.Start();
			File.AppendAllText(LogPath, ">>> " + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " " + proInfo.Arguments + Environment.NewLine);

			if (wait)
			{
				using (StreamReader errReader = gitProcess.StandardError)
				{
					string err = errReader.ReadToEnd(); // wait for exit
					if (!string.IsNullOrWhiteSpace(err))
						File.AppendAllText(LogPath, err);
				}
			}
		}
  }
}
