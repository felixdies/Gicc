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
		static string ConfigPath
		{
			// CONFIG_PATH 를 읽을 때 cwd 를 이용하므로, Gicc 를 실행 할 때의 cwd 는 항상 git repo 여야 한다.
			get { return Path.Combine(Environment.CurrentDirectory, @".git/gicc/config"); }
		}

		public static string VobPath
		{
			get { return ParseConfig("vob"); }
		}

		public static string BranchName
		{
			get { return ParseConfig("branch"); }
		}

		public static string RepoPath
		{
			get { return ParseConfig("repository"); }
		}

		internal static string GiccPath
		{
			get { return Path.Combine(RepoPath, @".git/gicc"); }
		}

		internal static string CachePath
		{
			get { return Path.Combine(GiccPath, @"cache"); }
		}

		internal static string LogPath
		{
			get { return Path.Combine(GiccPath, @"log"); }
		}

		internal static string CCoutPath
		{
			get { return Path.Combine(GiccPath, @"ccout"); }
		}

		internal static string GitoutPath
		{
			get { return Path.Combine(GiccPath, @"gitout"); }
		}

		internal static void WriteLog(string text)
		{
			File.AppendAllText(LogPath, text);
		}
		
		internal static List<string> ReadCCout()
		{
			return new List<string>(File.ReadAllLines(CCoutPath));
		}

		internal static List<string> ReadGitout()
		{
			return new List<string>(File.ReadAllLines(GitoutPath));
		}

		internal static void WriteConfig(List<string> configList)
		{
			string text = string.Join(Environment.NewLine, configList);
			File.WriteAllText(ConfigPath, text);
		}

		internal static List<string> ReadConfig()
		{
			return new List<string>(File.ReadAllLines(ConfigPath));
		}
		
		internal static void WriteCache(List<string> cacheList)
		{
			string text = string.Join(Environment.NewLine, cacheList);
			File.WriteAllText(CachePath, text);
		}

		internal static List<string> ReadCache()
		{
			return new List<string>(File.ReadAllLines(CachePath));
		}

		static string ParseConfig(string configName)
		{
			return IOHandler.ReadConfig().Find(config => config.ToLower().StartsWith(configName)).Split('=').Last().Trim();
		}

		internal static void Copy(string sourceFileName, string destFileName)
		{
			File.Copy(sourceFileName, destFileName, true);
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
