using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gicc
{
  internal class GitIgnore
  {
    List<Regex> ignoreDirList;
    List<Regex> unIgnoreDirList;
    List<Regex> ignoreFileList;
    List<Regex> unIgnoreFileList;

    internal GitIgnore(string[] ignoredPatternArr)
    {
      ignoreFileList = new List<Regex>();
      unIgnoreFileList = new List<Regex>();
      ignoreDirList = new List<Regex>();
      unIgnoreDirList = new List<Regex>();

      ParseIgnoredPatternArr(ignoredPatternArr);
    }

    /// <summary>
    /// 주어진 디렉토리 경로가 .gitignore 파일에 설정 돼 있는 지 여부를 반환합니다.
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    public bool IsIgnoredDir(string dirPath)
    {
      if (!dirPath.StartsWith("\\"))
      {
        dirPath = "\\" + dirPath;
      }

      foreach (Regex unIgnoreDir in unIgnoreDirList)
      {
        Match m = unIgnoreDir.Match(dirPath);
        if (m.Success && m.Length == dirPath.Length)
        {
          return false;
        }
      }

      foreach (Regex ignoreDir in ignoreDirList)
      {
        Match m = ignoreDir.Match(dirPath);
        if (m.Success && m.Length == dirPath.Length)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// 주어진 파일 경로가 .gitignore 파일에 설정 돼 있는 지 여부를 반환합니다.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool IsIgnoredFile(string filePath)
    {
      if (!filePath.StartsWith("\\"))
      {
        filePath = "\\" + filePath;
      }

      foreach (Regex unIgnoreFile in unIgnoreFileList)
      {
        Match m = unIgnoreFile.Match(filePath);
        if (m.Success && m.Length == filePath.Length)
        {
          return false;
        }
      }

      foreach (Regex ignoreFile in ignoreFileList)
      {
        Match m = ignoreFile.Match(filePath);
        if (m.Success && m.Length == filePath.Length)
        {
          return true;
        }
      }

      return false;
    }

    private void ParseIgnoredPatternArr(string[] ignoredPatternArr)
    {
      foreach (string patt in ignoredPatternArr)
      {
        if (patt.StartsWith("#"))
        {
          continue;
        }
        else if (patt.StartsWith("!"))
        {
          if (patt.EndsWith("/"))
          {
            this.unIgnoreDirList.Add(GlobPatternToRegex(patt.Substring(1)));
          }
          else
          {
            this.unIgnoreFileList.Add(GlobPatternToRegex(patt.Substring(1)));
          }
        }
        else
        {
          if (patt.EndsWith("/"))
          {
            this.ignoreDirList.Add(GlobPatternToRegex(patt));
          }
          else
          {
            this.ignoreFileList.Add(GlobPatternToRegex(patt));
          }
        }
      }
    }

    private Regex GlobPatternToRegex(string globPatt)
    {
      // 1. escape
      string patt = Regex.Escape(globPatt);

      // 2. parse the leading slash
      // 
      if (patt.StartsWith("/"))
      {
        patt = patt.Substring(1);
      }
      else
      {
        patt = ".*" + patt;
      }

      // 3. replace '/'
      patt.Replace("/", "[\\/]");

      // 4. parse asterisks


      return null;
    }
  }
}
