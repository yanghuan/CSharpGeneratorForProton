using System;

namespace Ice.Project.Config {
  public static class LoaderHelp {
    public static void Load(string dir) {
      GeneratorConfig.ConfigDir = dir;

      GlobalTemplateManager.Load();
      MountTemplateManager.Load();
      HeroTemplateManager.Load();

      GeneratorConfig.InvokeDelayInitAction();
    }
  }
}
