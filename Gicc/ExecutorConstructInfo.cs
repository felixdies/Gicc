using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc.Lib
{
  public class ExecutorConstructInfo
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
        {
          return _executingPath;
        }
      }

      set
      {
        _executingPath = value;
      }
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
        {
          return _outPath;
        }
      }

      set
      {
        _outPath = value;
      }
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
        {
          return _logPath;
        }
      }

      set
      {
        _logPath = value;
      }
    }

    public string BranchName { get; set; }
  }
}
