using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Service.Game
{
    internal class GameService : IGameService
    {
        private List<IGame> _games = new List<IGame>();
        private HashSet<Type> _registredGames = new HashSet<Type>();
        private IContainerProvider _container;
        private readonly INavigationService _navigationService;

        public GameService(IContainerProvider container, INavigationService navigationService)
        {
            _container = container;
            _navigationService = navigationService;
        }

        public IReadOnlyCollection<IGame> RegisteredGames => _games;
        
        public IGame CurrentGame { get; set; }

        public void ChangeGameBoard(string boardViewName)
        {
            _navigationService.NavigateToBoard(boardViewName);
        }

        public void RegisterGame(Type gameType)
        {
            if (_registredGames.Contains(gameType))
                throw new Exception($"Game: {gameType} already registered");

            if (!gameType.IsAssignableTo(typeof(IGame)))
                throw new Exception("Game has to implement IGame");

            _games.Add((IGame)_container.Resolve(gameType));
        }

        public void RegisterGame<T>() where T : IGame
        {
            RegisterGame(typeof(T));
        }
    }
}
