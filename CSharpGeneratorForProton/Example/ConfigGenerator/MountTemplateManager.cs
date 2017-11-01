using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ice.Project.Config {
  public sealed class MountTemplateExt : MountTemplate {
    protected override void OnInit() {
      if (Id < 0) {
        throw new ArgumentException(string.Format("Id[{0}] is less zero", Id));
      }
    }
  }

  public sealed class MountTemplateManager : ConfigSingleExtend<MountTemplateManager, MountTemplateExt> {
    protected override int GetId(MountTemplateExt item) {
      return item.Id;
    }
  }
}
