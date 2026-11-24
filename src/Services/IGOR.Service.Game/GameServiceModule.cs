using IGOR.Common.Modularity;
using IGOR.Interface.Game;
using Prism.Ioc;
using Prism.Modularity;
using System;

namespace IGOR.Service.Game
{
    [ModulePriority(ModuleType.Service)]
    public class GameServiceModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IGameService, GameService>();
        }
    }
}
