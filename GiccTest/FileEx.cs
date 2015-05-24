using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Gicc.Test
{
	class FileEx
	{
		internal static void DeleteIfExists(string path)
		{
			if (File.Exists(path))
			{
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}
			else if (Directory.Exists(path))
			{
				foreach (string file in Directory.GetFiles(path))
					DeleteIfExists(file);

				foreach (string dir in Directory.GetDirectories(path))
					DeleteIfExists(dir);

				Directory.Delete(path, false);
			}
		}

		internal static void MoveIfExists(string srcPath, string destPath)
		{
			DeleteIfExists(destPath);

			if (File.Exists(srcPath))
				File.Move(srcPath, destPath);
		}

		internal static void BackUp(string path)
		{
			MoveIfExists(path, path + "_backup");
		}

		internal static void Restore(string path)
		{
			MoveIfExists(path + "_backup", path);
		}
	}
}
