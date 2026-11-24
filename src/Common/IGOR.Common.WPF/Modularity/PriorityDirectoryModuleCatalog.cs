using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IGOR.Common.Modularity;

namespace IGOR.Common.WPF.Modularity
{
    public class PriorityDirectoryModuleCatalog : DirectoryModuleCatalog
    {
        protected override IEnumerable<IModuleInfo> Sort(IEnumerable<IModuleInfo> modules)
        => modules.Select(moduleInfo =>
        {
            var type = Type.GetType(moduleInfo.ModuleType);
            var modulePriority = (IModulePriority)type.GetCustomAttribute<ModulePriorityAttribute>();
            if (modulePriority == null)
                modulePriority = new DefaultModulePriority();
            return (moduleInfo, modulePriority);
        }).OrderByDescending(m => m.modulePriority.ModuleType)
          .ThenByDescending(m => m.modulePriority.Priority)
          .Select(m => m.moduleInfo);
    }
}
