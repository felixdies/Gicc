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
				File.Delete(path);
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
