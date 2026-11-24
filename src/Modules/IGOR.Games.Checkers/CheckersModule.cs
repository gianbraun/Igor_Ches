using IGOR.Common.Modularity;
using IGOR.Games.Checkers.ViewModels;
using IGOR.Games.Checkers.Views;
using IGOR.Interface.Game;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Games.Checkers
{
    public class CheckersModule : IModule
    {

        private readonly IGameService _gameService;

        public CheckersModule(IGameService gameService)
        {
            _gameService = gameService;
        }


        public void OnInitialized(IContainerProvider containerProvider)
        {
            _gameService.RegisterGame<CheckersGame>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CheckersView>();
            containerRegistry.RegisterForNavigation<CheckersCamView>();
            containerRegistry.RegisterForNavigation<CheckersBoardView>();
        }
    }
}
