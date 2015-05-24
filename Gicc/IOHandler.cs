using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Gicc
{
	class IOHandler
	{
		internal static List<string> ReadCCout()
		{
			return new List<string>(File.ReadAllLines(CCoutPath));
		}

		internal static List<string> ReadGitout()
		{
			return new List<string>(File.ReadAllLines(GitoutPath));
		}

		internal static List<string> ReadConfig()
		{
			return new List<string>(File.ReadAllLines(ConfigPath));
		}

		internal static string EliminateRepoPath(string path)
		{
			return Eliminate(path, RepoPath);
		}

		internal static string EliminateVobPath(string path)
		{
			return Eliminate(path, VobPath);
		}

		private static string Eliminate(string srcPath, string eliminatedPath)
		{
			if (srcPath.StartsWith(eliminatedPath))
				return srcPath.Remove(0, eliminatedPath.Length);
			else
				return srcPath;
		}
	}
}
