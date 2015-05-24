using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Gicc
{
	public class Logger
	{
		public string LogPath { get; set; }

		public Logger(string logPath)
		{
			this.LogPath = LogPath;
		}

		public void Write(string text)
		{
			// replace CR to CRLF
			string beautifiedText = text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n  ");	// add spaces
			File.AppendAllText(LogPath, beautifiedText);
		}

		public void WriteCommand(string command, DateTime time)
		{
			Write(">>> " + time.ToString("yy-MM-dd HH:mm:ss") + " " + command + Environment.NewLine);
		}
	}
}
