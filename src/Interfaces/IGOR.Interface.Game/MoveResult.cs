using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Interface.Game
{
    public readonly struct MoveResult
    {
        
        public IEnumerable<PortalCommand> PortalCommands { get; }
        public bool GameOver { get; }
        public Player Winner { get; }
        IBoard Board { get; }

        public MoveResult(IEnumerable<PortalCommand> portalCommands, bool gameOver, Player winner, IBoard board)
        {
            PortalCommands = portalCommands;
            GameOver = gameOver;
            Winner = winner;
            Board = board;
        }

    }
}
