using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Configuration;

using NUnit.Framework;
using Gicc;

namespace Gicc.Test
{
	public class GiccTestBase
	{
		protected string VOB_PATH = ConfigurationManager.AppSettings["VobPath"];
		protected string CC_TEST_PATH = ConfigurationManager.AppSettings["CCTestPath"];
		protected string CC_SYMBOLIC_LINK_PATH = ConfigurationManager.AppSettings["CCSymbolicLinkPath"];
		protected string BRANCH_NAME = ConfigurationManager.AppSettings["BranchName"];
		protected string REPO_PATH = ConfigurationManager.AppSettings["RepoPath"];
		protected string REPO_MOCKUP_PATH = ConfigurationManager.AppSettings["RepoMockupPath"];

		protected string GICC_PATH { get { return Path.Combine(REPO_PATH, @".git\gicc"); } }
		protected string GICC_MOCKUP_PATH { get { return Path.Combine(REPO_MOCKUP_PATH, @".git\gicc"); } }

		protected GitConstructInfo GitInfo
		{
			get
			{
				return new GitConstructInfo()
				{
					RepoPath = REPO_PATH,
					BranchName = BRANCH_NAME,
					ExecutingPath = REPO_PATH,
					OutPath = Path.Combine(GICC_PATH, "gitout"),
					LogPath = Path.Combine(GICC_PATH, "log")
				};
			}
		}

		protected GitConstructInfo GitMockupInfo
		{
			get
			{
				return new GitConstructInfo()
				{
					RepoPath = REPO_MOCKUP_PATH,
					BranchName = BRANCH_NAME,
					ExecutingPath = REPO_MOCKUP_PATH,
					OutPath = Path.Combine(GICC_MOCKUP_PATH, "gitout"),
					LogPath = Path.Combine(GICC_MOCKUP_PATH, "log")
				};
			}
		}

		protected List<CCElementVersion> CCMockupHistoryList
		{
			get
			{
				List<CCElementVersion> ccHistoryList = new List<CCElementVersion>();
				DateTime checkinTime = new DateTime(2015, 4, 1, 1, 0, 0);

				// FindAllFilesInBranch() 함수를 먼저 호출하므로 history 는 file 단위로 나타남.
				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime,
					ElementName = "tt.txt",
					Version = @"\main\0",
					Operation = "mkelem",
					OwnerFullName = "Init"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime,
					ElementName = "tt.txt",
					Version = @"\main\1",
					Operation = "mkelem",
					OwnerFullName = "Init"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(1),
					ElementName = "tt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\0",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(1),
					ElementName = "tt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\1",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(3),
					ElementName = "tt.txt",
					Version = @"\main\2",
					Operation = "chekin",
					OwnerFullName = "B"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(6),
					ElementName = "tt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\2",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(8),
					ElementName = "tt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\3",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(10),
					ElementName = "tt.txt",
					Version = @"\main\3",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(12),
					ElementName = "tt.txt",
					Version = @"\main\3",
					Operation = "rmver",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime,
					ElementName = @"sub\subtt.txt",
					Version = @"\main\0",
					Operation = "mkelem",
					OwnerFullName = "Init"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime,
					ElementName = @"sub\subtt.txt",
					Version = @"\main\1",
					Operation = "mkelem",
					OwnerFullName = "Init"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(2),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\0",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(2),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\1",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(4),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\2",
					Operation = "chekin",
					OwnerFullName = "B"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(5),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\3",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(7),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\2",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(9),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\" + BRANCH_NAME + @"\3",
					Operation = "chekin",
					OwnerFullName = "B"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(11),
					ElementName = @"sub\subtt.txt",
					Version = @"\main\4",
					Operation = "chekin",
					OwnerFullName = "A"
				}
				);

				ccHistoryList.Add(new CCElementVersion()
				{
					CreatedDate = checkinTime.AddMinutes(13),
					ElementName = "tt.txt",
					Version = @"\main\4",
					Operation = "rmver",
					OwnerFullName = "B"
				}
				);

				return ccHistoryList;
			}
		}

		public void CreateGitTestMockUp()
		{
			if (Directory.Exists(REPO_MOCKUP_PATH))
				return;

			string cachedCWD = Environment.CurrentDirectory;

			Git git = new Git(GitMockupInfo);
			string nl = Environment.NewLine;
			string tt = "tt.txt";
			string subtt = "sub\\subtt.txt";
			string authorA = "A <A@A.A>";
			string authorB = "B <B@B.B>";
			DateTime commitTime = new DateTime(2015, 05, 01, 01, 0, 0);

			Directory.CreateDirectory(Path.Combine(REPO_MOCKUP_PATH, @".git/gicc"));

			Environment.CurrentDirectory = REPO_MOCKUP_PATH;
			new Gicc(REPO_MOCKUP_PATH, CC_TEST_PATH, BRANCH_NAME, REPO_MOCKUP_PATH).WriteConfig();

			git.Init();
			File.WriteAllText(tt, string.Empty);
			Directory.CreateDirectory("sub");
			File.WriteAllText(subtt, string.Empty);
			git.AddCommit("first commit", "First <F@F.F>", commitTime.ToString());

			git.Checkout(BRANCH_NAME);
			File.WriteAllText(tt, "A1" + nl);
			File.WriteAllText(subtt, "A2" + nl);
			git.AddCommit("A1", authorA, commitTime.AddMinutes(1).ToString());

			git.Checkout("master");
			File.WriteAllText(tt, "B3" + nl);
			File.WriteAllText(subtt, "B4" + nl);
			git.AddCommit("B2", authorB, commitTime.AddMinutes(2).ToString());

			git.Checkout("master");
			File.WriteAllText(subtt, "A5" + nl);
			git.AddCommit("A3", authorA, commitTime.AddMinutes(3).ToString());

			git.Checkout(BRANCH_NAME);
			File.WriteAllText(tt, "A6" + nl);
			File.WriteAllText(subtt, "A7" + nl);
			File.WriteAllText(tt, "A8" + nl); // write again(cc history)
			git.AddCommit("A4", authorA, commitTime.AddMinutes(4).ToString());

			git.Checkout(BRANCH_NAME);
			File.WriteAllText(subtt, "B9" + nl);
			git.AddCommit("B5", authorB, commitTime.AddMinutes(5).ToString());

			git.Checkout("master");
			File.WriteAllText(tt, "A10" + nl);
			File.WriteAllText(subtt, "A11" + nl);
			git.AddCommit("A6", authorA, commitTime.AddMinutes(6).ToString());

			git.Checkout("master");
			File.Delete(tt);
			git.AddCommit("A7", authorA, commitTime.AddMinutes(7).ToString());

			git.Checkout("master");
			File.Delete(subtt);
			git.AddCommit("B8", authorB, commitTime.AddMinutes(8).ToString());

			Environment.CurrentDirectory = cachedCWD;
		}
	}
}
