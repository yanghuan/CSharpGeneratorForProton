using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ice.Project.Config {
  public sealed class HeroTemplateExt : HeroTemplate, IDelayInit {
    public MountTemplateExt MountTemplate { get; private set; }

    protected override void OnInit() {
      if (Weapons == null || Weapons.Length == 0) {
        throw new ArgumentException(string.Format("hero[{0}]'s Weapons is empty", Id));
      }
    }

    public void OnDelayInit() {
      if (MountId != 0) {
        MountTemplate = MountTemplateManager.Instance.GetItemTemplate(MountId);
        if (MountTemplate == null) {
          throw new ArgumentException(string.Format("hero[{0}]'s mount[{1}] is not found"));
        }
      }
    }
  }

  public sealed class HeroTemplateManager : ConfigSingleExtend<HeroTemplateManager, HeroTemplateExt> {
    protected override int GetId(HeroTemplateExt item) {
      return item.Id;
    }
  }
}
