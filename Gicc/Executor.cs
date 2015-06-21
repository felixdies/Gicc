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
			Execute(arg + " > '" + OutPath + "'");
			return File.ReadAllText(OutPath);
		}

		protected List<string> GetExecutedResultList(string arg)
		{
			Execute(arg + " > '" + OutPath + "'");
			return File.ReadAllLines(OutPath).ToList();
		}

		/// <summary>
		/// 성능상 문제가 있을 때에만 사용.
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>
		internal protected List<string> GetExecutedResultListWithOutFIO(List<string> argList)
		{
			List<string> resultOutputList = new List<string>();

			ProcessStartInfo proInfo = new ProcessStartInfo("cmd")
			{
				WorkingDirectory = ExecutingPath,
				CreateNoWindow = false,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			Process proc = new Process();
			proc.StartInfo = proInfo;
			proc.Start();

			StreamWriter inputWriter = proc.StandardInput;
			List<string> outputList = new List<string>();
			string errorMessage = string.Empty;
			
			inputWriter.AutoFlush = true;

			proc.OutputDataReceived += (sender, e) => outputList.Add(e.Data);
			proc.BeginOutputReadLine();

			proc.ErrorDataReceived += (sender, e) => errorMessage += e.Data + Environment.NewLine;
			proc.BeginErrorReadLine();

			for (int i = 0; i < argList.Count; i++)
			{
				inputWriter.WriteLine(argList[i]);
				Console.WriteLine("errorMessage : " + errorMessage);
				Console.WriteLine("output : ");
				outputList.ForEach(output => Console.WriteLine(output));
			}

			resultOutputList = outputList;

			proc.CloseMainWindow();
			proc.Close();

			return resultOutputList;
		}
	}
}
