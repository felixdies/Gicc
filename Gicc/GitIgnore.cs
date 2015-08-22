using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gicc
{
  internal class GitIgnore
  {
    List<Regex> ignoredDirList;
    List<Regex> unIgnoredDirList;
    List<Regex> ignoredFileList;
    List<Regex> unIgnoredFileList;

    internal GitIgnore()
    {
    }

    internal GitIgnore(string[] ignoredPatternArr)
    {
      ignoredFileList = new List<Regex>();
      unIgnoredFileList = new List<Regex>();
      ignoredDirList = new List<Regex>();
      unIgnoredDirList = new List<Regex>();

      ParseIgnoredPatternArr(ignoredPatternArr);
    }

    /// <summary>
    /// 주어진 디렉토리 경로가 .gitignore 파일에 설정 돼 있는 지 여부를 반환합니다.
    /// </summary>
    /// <param name="relDirPath">relative directory path</param>
    /// <returns></returns>
    public bool IsIgnoredDir(string relDirPath)
    {
      // remove leading slash
      // to use GlobPatternToRegex method, the path shouldn't start with a slash.
      if (relDirPath.StartsWith("/"))
      {
        relDirPath = relDirPath.Substring(1);
      }
      else if (relDirPath.StartsWith("\\"))
      {
        relDirPath = relDirPath.Substring(2);
      }

      foreach (Regex unIgnoreDir in unIgnoredDirList)
      {
        Match m = unIgnoreDir.Match(relDirPath);
        if (m.Success && m.Length == relDirPath.Length)
        {
          return false;
        }
      }

      foreach (Regex ignoreDir in ignoredDirList)
      {
        Match m = ignoreDir.Match(relDirPath);
        if (m.Success && m.Length == relDirPath.Length)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// 주어진 파일 경로가 .gitignore 파일에 설정 돼 있는 지 여부를 반환합니다.
    /// 파일 경로만을 체크하므로, .gitignore 파일에 폴더 단위로 설정 된 ignore 는 적용하지 않습니다.
    /// 폴더 단위로 설정 된 ignore 여부를 체크 하려면 IsIgnoredDir 메서드를 이용해야 합니다.
    /// </summary>
    /// <param name="relFilePath">relative file path</param>
    /// <returns></returns>
    public bool IsIgnoredFile(string relFilePath)
    {
      // remove leading slash
      // to use GlobPatternToRegex method, the path shouldn't start with a slash.
      if (relFilePath.StartsWith("/"))
      {
        relFilePath = relFilePath.Substring(1);
      }
      else if (relFilePath.StartsWith("\\"))
      {
        relFilePath = relFilePath.Substring(2);
      }

      foreach (Regex unIgnoreFile in unIgnoredFileList)
      {
        Match m = unIgnoreFile.Match(relFilePath);
        if (m.Success && m.Length == relFilePath.Length)
        {
          return false;
        }
      }

      foreach (Regex ignoreFile in ignoredFileList)
      {
        Match m = ignoreFile.Match(relFilePath);
        if (m.Success && m.Length == relFilePath.Length)
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
        if (string.IsNullOrWhiteSpace(patt) || patt.StartsWith("#"))
        {
          continue;
        }
        else if (patt.StartsWith("!"))
        {
          if (patt.EndsWith("/"))
          {
            this.unIgnoredDirList.Add(GlobPatternToRegex(patt.Substring(1).TrimEnd('/')));
          }
          else
          {
            this.unIgnoredFileList.Add(GlobPatternToRegex(patt.Substring(1)));
          }
        }
        else
        {
          if (patt.EndsWith("/"))
          {
            this.ignoredDirList.Add(GlobPatternToRegex(patt.TrimEnd('/')));
          }
          else
          {
            this.ignoredFileList.Add(GlobPatternToRegex(patt));
          }
        }
      }
    }

    /// <summary>
    /// Change glob pattern to regex
    /// </summary>
    /// <param name="globPatt"></param>
    /// <returns></returns>
    internal Regex GlobPatternToRegex(string globPatt)
    {
      bool startsWithSlash = false;

      if (globPatt == string.Empty)
      {
        throw new ArgumentException("globPatt is empty.");
      }

      // 1. escape
      string patt = EscapeRegex(globPatt);

      // 2. check the leading slash
      if (patt.StartsWith(@"/"))
      {
        patt = patt.Substring(1);
        startsWithSlash = true;
      }

      // stash before 3,4
      patt = patt.Replace(@"**/", @"\as");     // stash double asterisks+slash

      // 3. replace '/'
      patt = patt.Replace(@"/", @"[\\/]");

      // 4. parse asterisks
      patt = patt.Replace(@"**", @"\a");     // stash double asterisks
      patt = patt.Replace(@"*", @"[^\\/]*"); // change single asterisks

      // pop and replace after 4
      patt = patt.Replace(@"\as", @"(.+[\\/])*");     // stashed double asterisks+slash
      patt = patt.Replace(@"\a", @".*");     // stashed double asterisks

      // 5. replace prefix slash
      // - If the pattern starts with slash, just remove the slash.
      //   The target relative path shouldn't start with a slash.
      // - If not, attatch prefix.
      //   Then the pattern matches a file or dir in a subdirectory.
      if (startsWithSlash == false)
      {
        patt = @"(.+[\\/])*" + patt;
      }

      return new Regex(patt);
    }

    /// <summary>
    /// 문자(\, +, ?, |, {, (,), ^, $,., # 및 공백)의 최소 집합을 자체 이스케이프 코드로 대체하여 이스케이프합니다.
    /// Regex.Escape 와 달리 *, [ 를 escpae 하지 않습니다.
    /// </summary>
    /// <param name="globPatt"></param>
    /// <returns></returns>
    private string EscapeRegex(string globPatt)
    {
      string escapedPatt = Regex.Escape(globPatt);

      escapedPatt = escapedPatt.Replace(@"\*", "*");
      escapedPatt = escapedPatt.Replace(@"\[", "[");

      return escapedPatt;
    }
  }
}
