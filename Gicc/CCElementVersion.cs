using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc
{
  public class CCElementVersion
  {
    internal CCElementVersion()
    {
    }

    internal CCElementVersion(string versionInfo)
    {
      ParseFileInfo(versionInfo);
    }

    public string Attributes { get; set; }

    public string Comment { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public string EventDescription { get; set; }
    
    public string CheckoutInfo { get; set; }
    
    public string HostName { get; set; }
    
    public string IndentLevel { get; set; }
    
    public string Labels { get; set; }
    
    public string ObjectKind { get; set; }
    
    // <summary> Gets or sets relative path from VOB path </summary>
    public string ElementName { get; set; }
    
    public string Version { get; set; }
    
    public string PredecessorVersion { get; set; }
    
    public string Operation { get; set; }
    
    public string Type { get; set; }
    
    public string SymbolicLink { get; set; }
    
    public string OwnerLoginName { get; set; }
    
    public string OwnerFullName { get; set; }
    
    public string HyperLinkInfo { get; set; }

    public string VobPath { get; set; }

    public string Branch
    {
      get
      {
        string[] elemArr = Version.Split(new char[] { '\\', '/' });
        return elemArr[elemArr.Length - 2];
      }
    }

    // Grouping operations
    public int OperationGroup
    {
      get
      {
        if (Operation == "rmver" || Operation == "rmhlink")
        {
          // revert
          return -1;
        }
        else
        {
          // commit : "checkin", "mkelem", "rmelem", ...
          return 0;
        }
      }
    }

    public CCElementVersion Predecessor { get; set; }
    
    public CCElementVersion HyperLinkedFrom { get; set; }
    
    public CCElementVersion HyperLinkedTo { get; set; }

    internal void ParseFileInfo(string versionInfo)
    {
      List<string> versionInfoList = versionInfo.Split('|').ToList();
      Dictionary<string, string> versionInfoDic = new Dictionary<string, string>();

      foreach (string info in versionInfoList)
      {
        int i = info.IndexOf('=');
        if (i < 0 || i == info.Length - 1)
        {
          continue;
        }

        string key = info.Substring(0, i);
        string value = info.Substring(i + 1, info.Length - (i + 1));
        versionInfoDic.Add(key, value);
      }

      foreach (KeyValuePair<string, string> pair in versionInfoDic)
      {
        System.Reflection.PropertyInfo propertyInfo = this.GetType().GetProperty(pair.Key);
        if (propertyInfo != null && propertyInfo.PropertyType == typeof(string))
        {
          propertyInfo.SetValue(this, pair.Value);
        }
      }

      CreatedDate = DateTime.Parse(versionInfoDic["CreatedDate"]);

      if (versionInfoDic.ContainsKey("SymbolicLink"))
      {
        SymbolicLink = Path.GetFullPath((new Uri(Path.Combine(VobPath, versionInfoDic["SymbolicLink"]))).LocalPath);
      }
    }
  }
}
