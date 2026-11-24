using IGOR.Games.Morris.Views;
using IGOR.Interface.Game;
using Prism.Ioc;
using Prism.Modularity;

namespace IGOR.Games.Morris
{
    public class MorrisModule : IModule
    {
        private readonly IGameService _gameService;

        public MorrisModule(IGameService gameService)
        {
            _gameService = gameService;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _gameService.RegisterGame<MorrisGame>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MorrisView>();
            containerRegistry.RegisterForNavigation<MorrisBoardView>();
            containerRegistry.RegisterForNavigation<MorrisCamView>();
        }
    }
}
