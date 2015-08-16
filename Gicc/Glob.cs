/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * source code from : https://github.com/IronLanguages/main/blob/master/Languages/Ruby/Ruby/Builtins/Glob.cs
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace IronRuby.Builtins
{
  public static class Glob
  {
    // Duplicated constants from File.Constants
    private static class Constants
    {
      public readonly static int FNM_CASEFOLD = 0x08;
      public readonly static int FNM_DOTMATCH = 0x04;
      public readonly static int FNM_NOESCAPE = 0x01;
      public readonly static int FNM_PATHNAME = 0x02;
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")] // TODO
      public readonly static int FNM_SYSCASE = 0x08;
    }

    private class CharClass
    {
      private readonly StringBuilder/*!*/ _chars = new StringBuilder();

      internal void Add(char c)
      {
        if (c == ']' || c == '\\')
        {
          _chars.Append('\\');
        }
        _chars.Append(c);
      }

      internal string MakeString()
      {
        if (_chars.Length == 0)
        {
          return null;
        }
        if (_chars.Length == 1 && _chars[0] == '^')
        {
          _chars.Insert(0, "\\");
        }
        _chars.Insert(0, "[");
        _chars.Append(']');
        return _chars.ToString();
      }
    }

    private static void AppendExplicitRegexChar(StringBuilder/*!*/ builder, char c)
    {
      builder.Append('[');
      if (c == '^' || c == '\\')
      {
        builder.Append('\\');
      }
      builder.Append(c);
      builder.Append(']');
    }

    internal static string/*!*/ PatternToRegex(string/*!*/ pattern, bool pathName, bool noEscape)
    {
      StringBuilder result = new StringBuilder(pattern.Length);
      result.Append("\\G");

      bool inEscape = false;
      CharClass charClass = null;

      foreach (char c in pattern)
      {
        if (inEscape)
        {
          if (charClass != null)
          {
            charClass.Add(c);
          }
          else
          {
            AppendExplicitRegexChar(result, c);
          }
          inEscape = false;
          continue;
        }
        else if (c == '\\' && !noEscape)
        {
          inEscape = true;
          continue;
        }

        if (charClass != null)
        {
          if (c == ']')
          {
            string set = charClass.MakeString();
            if (set == null)
            {
              // Ruby regex "[]" matches nothing
              // CLR regex "[]" throws exception
              return String.Empty;
            }
            result.Append(set);
            charClass = null;
          }
          else
          {
            charClass.Add(c);
          }
          continue;
        }
        switch (c)
        {
          case '*':
            result.Append(pathName ? "[^/]*" : ".*");
            break;

          case '?':
            result.Append('.');
            break;

          case '[':
            charClass = new CharClass();
            break;

          default:
            AppendExplicitRegexChar(result, c);
            break;
        }
      }

      return (charClass == null) ? result.ToString() : String.Empty;
    }

    public static bool FnMatch(string/*!*/ pattern, string/*!*/ path, int flags)
    {
      if (pattern.Length == 0)
      {
        return path.Length == 0;
      }

      bool pathName = ((flags & Constants.FNM_PATHNAME) != 0);
      bool noEscape = ((flags & Constants.FNM_NOESCAPE) != 0);
      string regexPattern = PatternToRegex(pattern, pathName, noEscape);
      if (regexPattern.Length == 0)
      {
        return false;
      }

      if (((flags & Constants.FNM_DOTMATCH) == 0) && path.Length > 0 && path[0] == '.')
      {
        // Starting dot requires an explicit dot in the pattern
        if (regexPattern.Length < 4 || regexPattern[2] != '[' || regexPattern[3] != '.')
        {
          return false;
        }
      }

      RegexOptions options = RegexOptions.None;
      if ((flags & Constants.FNM_CASEFOLD) != 0)
      {
        options |= RegexOptions.IgnoreCase;
      }
      Match match = Regex.Match(path, regexPattern, options);
      return match != null && match.Success && (match.Length == path.Length);
    }
  }
}