using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace Gicc
{
	abstract class Executor
	{
		public Executor(ExecutorConstructInfo constructInfo)
		{
			this.BranchName = constructInfo.BranchName;

			this.ExecutingPath = constructInfo.ExecutingPath;
			this.OutPath = constructInfo.OutPath;
			this.LogPath = constructInfo.LogPath;
		}

		internal string BranchName { get; set; }
		
		protected string ExecutingPath { get; set; }
		protected string OutPath { get; set; }
		protected string LogPath { get; set; }
		
		protected abstract string Command { get; }

		protected void Execute(string arg, bool wait = true)
		{
			Process proc = new Process();
			ProcessStartInfo proInfo = new ProcessStartInfo()
			{
				WorkingDirectory = ExecutingPath,
				FileName = @"powershell",
				Arguments = Command + " " + arg,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true
			};

			proc.StartInfo = proInfo;
			proc.Start();
			new Logger(LogPath).WriteCommand(proInfo.Arguments, DateTime.Now);

			if (wait)
			{
				using (StreamReader errReader = proc.StandardError)
				{
					string err = errReader.ReadToEnd(); // wait for exit
					if (!string.IsNullOrWhiteSpace(err))
						new Logger(LogPath).Write(err);
				}
			}
		}

		protected string GetExecutedResult(string arg)
		{
			Execute(arg + " > " + OutPath);
			return File.ReadAllText(OutPath);
		}

		protected List<string> GetExecutedResultList(string arg)
		{
			Execute(arg + " > " + OutPath);
			return File.ReadAllLines(OutPath).ToList();
		}
	}

	class ExecutorConstructInfo
	{
		private string _executingPath;
		private string _outPath;
		private string _logPath;

		public string ExecutingPath
		{
			get
			{
				if (_executingPath == null)
				{
					Console.WriteLine(Environment.StackTrace);
					throw new NullReferenceException("ExecutorConstructInfo 의 ExecutingPath 가 선언되지 않았습니다.");
				}
				else
					return _executingPath;
			}
			set { _executingPath = value; }
		}
		public string OutPath
		{
			get
			{
				if (_outPath == null)
				{
					Console.WriteLine(Environment.StackTrace);
					throw new NullReferenceException("ExecutorConstructInfo 의 OutPath 가 선언되지 않았습니다.");
				}
				else
					return _outPath;
			}
			set { _outPath = value; }
		}
		public string LogPath
		{
			get
			{
				if (_logPath == null)
				{
					Console.WriteLine(Environment.StackTrace);
					throw new NullReferenceException("ExecutorConstructInfo 의 LogPath 가 선언되지 않았습니다.");
				}
				else
					return _logPath;
			}
			set { _logPath = value; }
		}

		public string BranchName { get; set; }
	}
}
