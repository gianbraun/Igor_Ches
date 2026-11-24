using IGOR.Common.Modularity;
using IGOR.Interface.Camera;
using Prism.Ioc;
using Prism.Modularity;


namespace IGOR.Service.Camera
{
    [ModulePriority(ModuleType.Service, -10)]
    public class CameraModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ICameraService>().Initialize();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ICameraService, CameraService>();
        }
    }
}
