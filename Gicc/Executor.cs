using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc.Lib
{
  public abstract class Executor
  {
    public Executor(ExecutorConstructInfo constructInfo)
    {
      this.BranchName = constructInfo.BranchName;

      this.ExecutingPath = constructInfo.ExecutingPath;
      this.OutPath = constructInfo.OutPath;
      this.LogPath = constructInfo.LogPath;
    }

    internal string BranchName { get; set; }

    protected abstract string Command { get; }

    protected string ExecutingPath { get; set; }
    
    protected string OutPath { get; set; }
    
    protected string LogPath { get; set; }

    /// <summary>
    /// 성능상 문제가 있을 때에만 사용.
    /// Input 을 redirect 하기 위해 CreateNoWindow 를 false 로 설정하므로, 커맨드창이 나타나는 부작용이 있다.
    /// </summary>
    /// <param name="argList"></param>
    /// <returns></returns>
    protected internal string GetExecutedResultWithoutFIO(List<string> argList)
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
      inputWriter.AutoFlush = true;

      foreach (string arg in argList)
      {
        inputWriter.WriteLine(Command + " " + arg);
      }

      inputWriter.WriteLine("exit");

      string err = proc.StandardError.ReadToEnd();
      if (!string.IsNullOrWhiteSpace(err))
      {
        new Logger(LogPath).Write(err);
        throw new GiccException("명령을 수행하던 중 오류가 발생 하였습니다.");
      }

      return proc.StandardOutput.ReadToEnd();
    }

    protected internal List<string> GetExecutedResultListWithoutFIO(List<string> argList)
    {
      string executedResult = GetExecutedResultWithoutFIO(argList);
      string[] resultLines = executedResult.Split(Environment.NewLine.ToCharArray());

      List<string> resultList = Enumerable.Repeat(string.Empty, argList.Count).ToList();

      int index = -1;
      foreach (string line in resultLines)
      {
        if (line.StartsWith(ExecutingPath + ">"))
        {
          index++;
          continue;
        }

        if (string.IsNullOrWhiteSpace(line))
        {
          continue;
        }

        if (index < 0)
        {
          continue;
        }

        resultList[index] += line + Environment.NewLine;
      }

      return resultList.Select(result => result.Trim()).ToList();
    }

    /// <summary>
    /// 첫 번째 매개변수를 인자로 하는 Command 를 실행 합니다.
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="wait">해당 명령을 synchronized 처리합니다.</param>
    protected void Execute(string arg, bool wait = true)
    {
      Process proc = new Process();
      ProcessStartInfo proInfo = new ProcessStartInfo()
      {
        WorkingDirectory = ExecutingPath,
        FileName = @"powershell", // "cmd" doesn't execute the passed command.
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
          {
            new Logger(LogPath).Write(err);
          }
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
  }
}
