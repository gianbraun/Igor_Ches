using IGOR.Games.TicTacToe.Views;
using IGOR.Interface.Game;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Games.TicTacToe
{
    public class TicTacToeModule : IModule
    {
        private IGameService _gameservice;

        public TicTacToeModule(IGameService gameservice)
        {
            _gameservice = gameservice;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _gameservice.RegisterGame<TicTacToeGame>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterForNavigation<TicTacToeView>();
            containerRegistry.RegisterForNavigation<TicTacToeTouchView>();
            containerRegistry.RegisterForNavigation<TicTacToeBoardView>();
            containerRegistry.RegisterForNavigation<TicTacToeCamView>();
        }
    }
}
