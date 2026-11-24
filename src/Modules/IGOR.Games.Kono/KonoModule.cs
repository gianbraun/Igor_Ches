using IGOR.Common.Modularity;
using IGOR.Games.Kono.Views;
using IGOR.Interface.Game;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Games.Kono
{ 
    public class KonoModule : IModule
    {
        private readonly IGameService _gameservice;

        public KonoModule(IGameService gameservice)
        {
            _gameservice = gameservice;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _gameservice.RegisterGame<KonoGame>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<KonoView>();
            containerRegistry.RegisterForNavigation<KonoBoardView>();
            containerRegistry.RegisterForNavigation<KonoCamView>();
        }
    }
}
