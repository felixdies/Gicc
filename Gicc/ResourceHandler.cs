using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.IO;

namespace Gicc
{
	class ResourceHandler
	{
		/// <summary>
		/// 어셈블리에 포함된 리소스의 문자열을 읽는다.
		/// 이 메서드는 용량이 크지 않은 리소스 파일을 읽을 때에만 사용할 것.
		/// </summary>
		/// <param name="path"></param>
		internal static string GetResource(string path)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "Gicc.resources." + path;

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
