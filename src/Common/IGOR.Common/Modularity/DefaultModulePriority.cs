using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Common.Modularity
{
    public class DefaultModulePriority : IModulePriority
    {
        public ModuleType ModuleType => ModuleType.Standard;
        public int Priority => int.MinValue;
    }
}
