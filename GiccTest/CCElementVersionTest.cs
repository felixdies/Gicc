using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

using NUnit.Framework;
using Gicc.Lib;

namespace Gicc.Test
{
	[TestFixture]
	public class CCFileVersionTest : GiccTestBase
	{
		[Test]
		public void ParseFmtTest()
		{
			CCElementVersion expected = new CCElementVersion();
			CCElementVersion actual = new CCElementVersion();

			expected.CreatedDate = DateTime.Parse("2015-05-04 15:57:38");
			expected.EventDescription = "checkout version";
			expected.CheckoutInfo = "reserved";
			expected.HostName = ConfigurationManager.AppSettings["HostName"];
			expected.ObjectKind = "version";
			expected.ElementName = "branch1.txt";
			expected.Version = @"\main\" + BRANCH_NAME + @"\CHECKEDOUT";
			expected.PredecessorVersion = @"\main\" + BRANCH_NAME + @"\4";
			expected.Operation = "checkout";
			expected.Type = "text_file";
			expected.OwnerLoginName = ConfigurationManager.AppSettings["LogInUserName"];
			expected.OwnerFullName = "Park";

			actual.ParseFileInfo(@"null|Attributes=|Comment=|CreatedDate=2015-05-04T15:57:38+09:00|EventDescription=checkout version|CheckoutInfo=reserved|HostName=D-SEL-00650801|IndentLevel=|Labels=|ObjectKind=version|ElementName=branch1.txt|Version=\main\" + BRANCH_NAME + @"\CHECKEDOUT|PredecessorVersion=\main\" + BRANCH_NAME
					+ @"\4|Operation=checkout|Type=text_file|SymbolicLink=|OwnerLoginName=" + ConfigurationManager.AppSettings["LogInUserName"]
					+ "|OwnerFullName=Park|HyperLink=");
			AssertEx.PropertyValuesAreEquals(expected, actual);

			expected.CreatedDate = DateTime.Now;
			AssertEx.PropertyValuesAreNotEquals(expected, actual);
		}
	}
}
