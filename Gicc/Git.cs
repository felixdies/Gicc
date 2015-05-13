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

		string Command
		{
			get { return "git "; }
		}

		internal string Help()
		{
			Execute("help >" + IOHandler.GitoutPath);
			return IOHandler.ReadGitout()[0];
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

		internal void Checkout(string branch)
		{
			List<string> allBranches = GetAllBranches();
			if(allBranches.Contains(branch) || allBranches.Contains("* " + branch))
				Execute("checkout -b " + branch);
			Execute("checkout " + branch);
		}

		internal void Execute(string arg, bool wait = true)
		{
			Process gicProcess = new Process();

			ProcessStartInfo proInfo = new ProcessStartInfo()
			{
				WorkingDirectory = RepositoryPath,
				FileName = @"powershell",
				Arguments = Command + arg,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true
			};

			gicProcess.StartInfo = proInfo;
			gicProcess.Start();
			IOHandler.WriteLog(">>> " + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " " + proInfo.Arguments + Environment.NewLine);

			if (wait)
			{
				using (StreamReader errReader = gicProcess.StandardError)
				{
					string err = errReader.ReadToEnd(); // wait for exit
					if (!string.IsNullOrWhiteSpace(err))
						IOHandler.WriteLog(err);
				}
			}
		}
  }
}
