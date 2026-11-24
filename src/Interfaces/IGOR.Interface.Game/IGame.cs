using System;
using System.Collections.Generic;

namespace IGOR.Interface.Game
{
    public interface IGame
    {
        event EventHandler<Player> GameOver;
        event EventHandler BoardReady;
        Difficulty Difficulty { get; set; }

        Player HumanPlayer { get; set; }

        IGameAi GameAi { get; }

        string Name { get; }

        string ImageRessourcePath { get; }

        Dictionary<string,(string board, string hmi)> Views { get; }

        public (string board, string hmi) ActualView { get; set; }

        string GameRulePath { get; }

        INode CurrentState { get; }

        INode NextState { get; }

        IBoard CurrentBoard { get; }

        bool? IsGameOver { get; }

        void Start();
        
        /// <summary>
        /// Sets an end state immideately. ConfirmNextState is not neccessary.
        /// </summary>
        void End();

        bool SetField(Player[][] field);
         
        bool SetField(FieldPosition start, FieldPosition end, FieldPosition oppKick);

        /// <summary>
        /// Sets an ai-move as next state
        /// </summary>
        void MakeAiMove();

        /// <summary>
        /// Confirms, that the next state is reached
        /// </summary>
        void ConfirmNextState();

        void RaiseGameOver(Player player);
    }

}
