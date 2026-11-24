using Prism.Ioc;
using Prism.Modularity;
using IGOR.Common.Modularity;
using IGOR.Interface.Parameters;
using IGOR.Interface.Communication;

namespace IGOR.Service.Parameters
{
    [ModulePriority(ModuleType.Service)]
    public class ParametersModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IParameterService, ParameterService>();
        }
    }
}