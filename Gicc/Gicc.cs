using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicc
{
  public class Gicc
  {
    public void Pull(DateTime since, DateTime until)
    {
      CheckCheckedOutFileIsNotExist();
      CheckAllSymbolicLinksAreMounted();

      throw new NotImplementedException();

      // git stash

      ClearCase.SetDefaultCS();
      // git checkout master
      // copy vob >> repo
      // master commit
      // 만약 stash 할 때 master branch 였다면 git stash pop

      ClearCase.SetBranchCS(IOHandler.BranchName);
      // git checkout user_branch
      // copy vob >> repo
      // user_branch commit
      // 만약 stash 할 때 user_branch 였다면 git stash pop
    }

    internal void CheckCheckedOutFileIsNotExist()
    {
      List<string> checkedoutFileList = ClearCase.LscheckoutInCurrentViewByLoginUser();
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
      List<CCElementVersion> slinkList = ClearCase.FindAllSymbolicLinks();

      foreach (CCElementVersion link in slinkList)
      {
        if (!System.IO.Directory.Exists(link.SymbolicLink))
          throw new GiccException(link.SymbolicLink + "  VOB 이 mount 되지 않았습니다.");
      }
    }
  }
}
