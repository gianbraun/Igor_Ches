using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Interface.Game
{
    public interface IGameService
    {
        IReadOnlyCollection<IGame> RegisteredGames { get; }

        IGame CurrentGame { get; set; }
        
        void RegisterGame(Type gameType);

        void RegisterGame<T>()
            where T : IGame;

        void ChangeGameBoard(string boardViewName);

    }
}
