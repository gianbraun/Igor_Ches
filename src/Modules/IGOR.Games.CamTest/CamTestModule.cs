using IGOR.Games.CamTest.Views;
using IGOR.Interface.Game;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Games.CamTest
{
    public class CamTestModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
{
            var gameservice = containerProvider.Resolve<IGameService>();
            gameservice.RegisterGame<CamTestGame>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CameraTestView>();
        }
    }
}
