using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Gicc
{
	class ClearCase : Executor
	{
		/// <summary>
		/// Clone, Pull, Push 명령어 실행 시 사용
		/// </summary>
		/// <param name="constructInfo"></param>
		public ClearCase(CCConstructInfo constructInfo)
			: base(constructInfo)
		{
			this.VobPath = constructInfo.VobPath;
		}

		/// <summary>
		/// Label, Tree 명령어 실행 시 사용
		/// </summary>
		/// <param name="constructInfo"></param>
		public ClearCase(ExecutorConstructInfo constructInfo)
			: base(constructInfo)
		{ }

		string Fmt
		{
			get
			{
				return "'"
				+ "Attributes=%a"
				+ "|Comment=%Nc"
				+ "|CreatedDate=%d"
				+ "|EventDescription=%e"
				+ "|CheckoutInfo=%Rf"
				+ "|HostName=%h"
				+ "|IndentLevel=%i"
				+ "|Labels=%l"
				+ "|ObjectKind=%m"
				+ "|ElementName=%En"
				+ "|Version=%Vn"
				+ "|PredecessorVersion=%PVn"
				+ "|Operation=%o"
				+ "|Type=%[type]p"
				+ "|SymbolicLink=%[slink_text]p"
				+ "|OwnerLoginName=%[owner]p"
				+ "|OwnerFullName=%[owner]Fp"
				+ "|HyperLink=%[hlink]p"
				+ "\n'";
			}
		}

		string VobPath { get; set; }

		internal string CurrentView
		{
			get
			{
				return GetExecutedResult("pwv").Split(' ')[3];
			}
		}

		internal string LogInUser
		{
			get
			{
				List<string> viewInfo = GetExecutedResultList("lsview -long " + CurrentView);
				return viewInfo.Find(info => info.StartsWith("View owner")).Split(' ')[2];
			}
		}

		internal string LogInUserName
		{
			get
			{
				return LogInUser.Split('\\').Last();
			}
		}

		internal void CheckCheckedoutFileIsNotExist()
		{
			List<string> checkedoutFileList = LscheckoutInCurrentViewByLoginUser();
			if (checkedoutFileList.Count > 0)
			{
				string message =
					"체크아웃 된 파일이 있습니다." + Environment.NewLine
					+ string.Join(Environment.NewLine, checkedoutFileList);
				throw new GiccException(message);
			}
		}

		internal void CheckAllSymbolicLinksAreMounted()
		{
			List<CCElementVersion> slinkList = FindAllSymbolicLinks();

			foreach (CCElementVersion link in slinkList)
			{
				if (!System.IO.Directory.Exists(link.SymbolicLink))
					throw new GiccException(link.SymbolicLink + " VOB 이 mount 되지 않았습니다.");
			}
		}

		internal List<string> CatCS()
		{
			return GetExecutedResultList("catcs");
		}

		internal void SetCS(string[] configSpec)
		{
			File.WriteAllLines(OutPath, configSpec);
			Execute("setcs " + OutPath);
		}

		internal void SetBranchCS()
		{
			string[] branchCS = new string[]{
				"element * CHECKEDOUT"
				, "element -dir * /main/LATEST"
				, "element -file * /main/" + BranchName + "/LATEST"
				, "element -file * /main/LATEST -mkbranch " + BranchName
			};

			SetCS(branchCS);
		}

		internal void SetBranchCS(DateTime time)
		{
			string[] branchCS = new string[] {
				"time " + time.ToString()
				, "element * CHECKEDOUT"
				, "element -dir * /main/LATEST"
				, "element -file * /main/" + BranchName + "/LATEST"
				, "element -file * /main/LATEST -mkbranch " + BranchName
				, "end time"
			};

			SetCS(branchCS);
		}

		internal void SetDefaultCS()
		{
			Execute("setcs -default");
		}

		public List<string> FindAllFilesInBranch()
		{
      string args = string.Empty;
			
			if (!string.IsNullOrWhiteSpace(BranchName))
				args += " -branch 'brtype(" + BranchName + ")'";

			return GetExecutedResultList("find . " + args + " -print");
		}

		public List<string> FindAllFilesInBranch(DateTime since, DateTime until)
		{
			string args = string.Empty;

			if (!string.IsNullOrWhiteSpace(BranchName))
				args += " -branch 'brtype(" + BranchName + ")'";

			args += " -version '{created_since(" + since.AddSeconds(1).ToString() + ") && !created_since(" + until.AddSeconds(1).ToString() + ")}'";

			return GetExecutedResultList("find . " + args + " -print");
		}

    public void ViewVersionTree(string filePath)
    {
			Execute("lsvtree -graphical " + filePath + "\\LATEST", false);
    }

    public void LabelLatestMain(string filePath, string label)
    {
			Execute("mklabel -replace -version \\main\\LATEST " + label + " " + filePath, false);
    }

		internal List<CCElementVersion> FindAllSymbolicLinks()
		{
			List<CCElementVersion> resultSLinkList = new List<CCElementVersion>();
			List<string> foundSLinkList;

			foundSLinkList = GetExecutedResultList("find " + VobPath + " -type l -print");

			foundSLinkList.ForEach(link => resultSLinkList.Add(Describe(link)));
			
			return resultSLinkList;
		}

		internal List<CCElementVersion> Lshistory(string pname)
		{
			List<CCElementVersion> resultList = new List<CCElementVersion>();

			GetExecutedResultList("lshistory -fmt " + Fmt + " " + pname)
				.ForEach(elemVersion => resultList.Add(
					new CCElementVersion(elemVersion) { VobPath = this.VobPath })
					);

			return resultList;
		}

		internal List<CCElementVersion> Lshistory(string pname, DateTime since)
		{
			List<CCElementVersion> resultList = new List<CCElementVersion>();

			GetExecutedResultList("lshistory -fmt " + Fmt + " -since" + since.AddSeconds(1) + " " + pname)
				.ForEach(elemVersion => resultList.Add(
					new CCElementVersion(elemVersion){ VobPath = this.VobPath })
					);

			return resultList;
		}

		internal CCElementVersion Describe(string pname)
		{
			string description = GetExecutedResult("describe -fmt " + Fmt + " " + pname);
			return new CCElementVersion(description) { VobPath = this.VobPath }; 
		}

		internal string Pwd()
		{
			return GetExecutedResult("pwd");
		}

		internal List<string> LscheckoutInCurrentViewByLoginUser()
		{
			return GetExecutedResultList("lscheckout -short -cview -me -recurse");
		}

		// vob path 의 상위 디렉터리에서 mount 를 실행해야 한다.
		internal void Mount(string vobTag)
		{
			Execute("mount \\" + vobTag);
		}

		// vob path 의 상위 디렉터리에서 umount 를 실행해야 한다.
		internal void UMount(string vobTag)
		{
			Execute("umount \\" + vobTag);
		}

		internal void Checkout(string pname)
		{
			Execute("checkout -ncomment " + pname);
		}

		internal void Uncheckout(string pname)
		{
			Execute("uncheckout -keep " + pname);
		}

		protected override string Command
		{
			get { return "cleartool"; }
		}
	}

	class CCConstructInfo : ExecutorConstructInfo
	{
		public string VobPath { get; set; }
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
		/// <summary> VOB path 로부터의 상대 경로 </summary>
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

		public string VobPath { get; set; }

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

			if (versionInfoDic.ContainsKey("SymbolicLink"))
				SymbolicLink = Path.GetFullPath((new Uri(Path.Combine(VobPath, versionInfoDic["SymbolicLink"]))).LocalPath);
		}
	}
}
