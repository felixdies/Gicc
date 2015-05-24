using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace Gicc
{
	public class ClearCase
	{
		static string Fmt
		{
			get
			{
				string fmt = "'";
				fmt += "Attributes=%a";
				fmt += "|Comment=%Nc";
				fmt += "|CreatedDate=%d";
				fmt += "|EventDescription=%e";
				fmt += "|CheckoutInfo=%Rf";
				fmt += "|HostName=%h";
				fmt += "|IndentLevel=%i";
				fmt += "|Labels=%l";
				fmt += "|ObjectKind=%m";
				fmt += "|ElementName=%En";
				fmt += "|Version=%Vn";
				fmt += "|PredecessorVersion=%PVn";
				fmt += "|Operation=%o";
				fmt += "|Type=%[type]p";
				fmt += "|SymbolicLink=%[slink_text]p";
				fmt += "|OwnerLoginName=%[owner]p";
				fmt += "|OwnerFullName=%[owner]Fp";
				fmt += "|HyperLink=%[hlink]p";
				fmt += "\n'";
				return fmt;
			}
		}

		internal static string CurrentView
		{
			get
			{
				Execute("pwv > " + IOHandler.CCoutPath);
				return IOHandler.ReadCCout()[0].Split(' ')[3];
			}
		}

		internal static string LogInUser
		{
			get
			{
				Execute("lsview -long " + CurrentView + " > " + IOHandler.CCoutPath);
				List<string> viewInfo = IOHandler.ReadCCout();
				return viewInfo.Find(info => info.StartsWith("View owner")).Split(' ')[2];
			}
		}

		internal static string LogInUserName
		{
			get
			{
				return LogInUser.Split('\\').Last();
			}
		}

		static string Command
		{
			get { return "cleartool "; }
		}

		internal static List<string> CatCS()
		{
			Execute("catcs > " + IOHandler.CCoutPath);
			return IOHandler.ReadCCout();
		}

		internal static void SetCS(List<string> configSpec)
		{
			IOHandler.WriteCache(configSpec);
			Execute("setcs " + IOHandler.CachePath);
		}

		internal static void SetBranchCS(string branchName)
		{
			List<string> branchCS = new List<string>(new string[] {
				"element * CHECKEDOUT"
				, "element -dir * /main/LATEST"
				, "element -file * /main/" + branchName + "/LATEST"
				, "element -file * /main/LATEST -mkbranch " + branchName
			});

			SetCS(branchCS);
		}

		internal static void SetDefaultCS()
		{
			Execute("setcs -default");
		}

		public static List<string> FindAllFilesInBranch(string pname, string branchName)
		{
      string args = string.Empty;
			
			if (!string.IsNullOrWhiteSpace(branchName))
				args += " -branch 'brtype(" + branchName + ")'";

			Execute("find " + pname + args + " -print > " + IOHandler.CCoutPath);
      
      return IOHandler.ReadCCout();
		}

    public static void ViewVersionTree(string pname)
    {
      Execute("lsvtree -graphical " + pname + "\\LATEST", false);
    }

    public static void LabelLatestMain(string pname, string label)
    {
      Execute("mklabel -replace -version \\main\\LATEST " + label + " " + pname, false);
    }

		internal static List<CCElementVersion> FindAllSymbolicLinks()
		{
			List<CCElementVersion> resultSLinkList = new List<CCElementVersion>();
			List<string> foundSLinkList;

			Execute("find " + IOHandler.VobPath + " -type l -print > " + IOHandler.CCoutPath);
			foundSLinkList = IOHandler.ReadCCout();

			foundSLinkList.ForEach(link => resultSLinkList.Add(Describe(link)));
			
			return resultSLinkList;
		}

		internal static List<CCElementVersion> Lshistory(string pname)
		{
			List<CCElementVersion> resultList = new List<CCElementVersion>();

			Execute("lshistory -fmt " + Fmt + " " + pname + " > " + IOHandler.CCoutPath);
			foreach (string info in IOHandler.ReadCCout())
			{
				resultList.Add(new CCElementVersion(info));
			}

			return resultList;
		}

		internal static List<CCElementVersion> Lshistory(string pname, DateTime since)
		{
			List<CCElementVersion> resultList = new List<CCElementVersion>();

			Execute("lshistory -fmt " + Fmt + " -since" + since.ToString() + " " + pname + " > " + IOHandler.CCoutPath);
			foreach (string info in IOHandler.ReadCCout())
			{
				resultList.Add(new CCElementVersion(info));
			}

			return resultList;
		}

		internal static CCElementVersion Describe(string pname)
		{
			Execute("describe -fmt " + Fmt + " " + pname + " > " + IOHandler.CCoutPath); 
			return new CCElementVersion(IOHandler.ReadCCout()[0]);
		}

		internal static void Pwd()
		{
			Execute("pwd > " + IOHandler.CCoutPath);
		}

		internal static List<string> LscheckoutInCurrentViewByLoginUser()
		{
			Execute("lscheckout -short -cview -me -recurse > " + IOHandler.CCoutPath);

			return IOHandler.ReadCCout();
		}

		internal static void Mount(string vobTag)
		{
			Execute(Path.GetDirectoryName(IOHandler.VobPath), "mount \\" + vobTag);
		}

		internal static void UMount(string vobTag)
		{
			Execute(Path.GetDirectoryName(IOHandler.VobPath), "umount \\" + vobTag);
		}

		internal static void Checkout(string pname)
		{
			Execute("checkout -ncomment " + pname);
		}

		internal static void Uncheckout(string pname)
		{
			Execute("uncheckout -keep " + pname);
		}

    protected static void Execute(string workingDirectory, string arg, bool wait = true)
		{
			Process cleartool = new Process();

			ProcessStartInfo proInfo = new ProcessStartInfo()
			{
				WorkingDirectory = workingDirectory,
				FileName = @"powershell",
				Arguments = Command + arg,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true
			};

			cleartool.StartInfo = proInfo;
			cleartool.Start();
			IOHandler.WriteLog(">>> " + DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " " + proInfo.Arguments + Environment.NewLine);
			
			if (wait)
			{
				using (StreamReader errReader = cleartool.StandardError)
				{
					string err = errReader.ReadToEnd(); // wait for exit
					if (!string.IsNullOrWhiteSpace(err))
						IOHandler.WriteLog(err);
				}
			}
		}

		protected static void Execute(string arg, bool wait = true)
		{
			Execute(IOHandler.VobPath, arg, wait);
		}
	}

	class CCElementVersion
	{
		public string Attributes { get; set; }
		public string Comment { get; set; }
		public DateTime CreatedDate { get; set; }
		public string EventDescription { get; set; }
		public string CheckoutInfo { get; set; }
		public string HostName { get; set; }
		public string IndentLevel { get; set; }
		public string Labels { get; set; }
		public string ObjectKind { get; set; }
		public string ElementName { get; set; }
		public string Version { get; set; }
		public string PredecessorVersion { get; set; }
		public string Operation { get; set; }
		public string Type { get; set; }
		public string SymbolicLink { get; set; }
		public string OwnerLoginName { get; set; }
		public string OwnerFullName { get; set; }
		public string HyperLinkInfo { get; set; }
		
		public string Branch
		{
			get
			{
				string[] elemArr = Version.Split(new char[] { '\\', '/' });
				return elemArr[elemArr.Length - 2];
			}
		}

		public CCElementVersion Predecessor { get; set; }
		public CCElementVersion HyperLinkedFrom { get; set; }
		public CCElementVersion HyperLinkedTo { get; set; }

		internal CCElementVersion()
		{
		}

		internal CCElementVersion(string versionInfo)
		{
			ParseFileInfo(versionInfo);
		}

		internal void ParseFileInfo(string versionInfo)
		{
			List<string> versionInfoList = new List<string>(versionInfo.Split('|'));
			Dictionary<string, string> versionInfoDic = new Dictionary<string, string>();

			foreach (string info in versionInfoList)
			{
				int i = info.IndexOf('=');
				if (i < 0 || i == info.Length - 1)
					continue;

				string key = info.Substring(0, i);
				string value = info.Substring(i + 1, info.Length - (i + 1));
				versionInfoDic.Add(key, value);
			}

			foreach (KeyValuePair<string, string> pair in versionInfoDic)
			{
				System.Reflection.PropertyInfo propertyInfo = this.GetType().GetProperty(pair.Key);
				if (propertyInfo != null && propertyInfo.PropertyType == typeof(string))
					propertyInfo.SetValue(this, pair.Value);
			}

			CreatedDate = DateTime.Parse(versionInfoDic["CreatedDate"]);
			
			if(versionInfoDic.ContainsKey("SymbolicLink"))
				SymbolicLink = Path.GetFullPath((new Uri(Path.Combine(IOHandler.VobPath, versionInfoDic["SymbolicLink"]))).LocalPath);
		}
	}
}
