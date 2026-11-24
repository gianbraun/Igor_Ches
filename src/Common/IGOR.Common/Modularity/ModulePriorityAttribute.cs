using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Common.Modularity
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ModulePriorityAttribute : Attribute, IModulePriority
    {

        public ModuleType ModuleType { get; } = ModuleType.Standard;

        public int Priority { get; }

        public ModulePriorityAttribute(ModuleType moduleType, int priority = 0)
        {
            ModuleType = moduleType;
            Priority = priority;
        }
    }
}
