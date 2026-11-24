using IGOR.Games.TicTacToe.Views;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IGOR.Games.TicTacToe
{
    /// <summary>
    /// Memory efficient representation of Tic-Tac-Toe game board.
    /// </summary>
    internal class TicTacToeBoard : BoardBase
    {
        /// <summary>
        /// Gets the board size. Classic version is 3x3.
        /// </summary>
        public override int Size => 3;
        public override Dictionary<FieldPosition, (double x, double y)> ValidPositions => TicTacToeBoardView.ValidPositions;

        public TicTacToeBoard(TicTacToeBoard field)
            : base(field)
        {
        }

        public TicTacToeBoard(Player humanPlayer)
            : base(humanPlayer)
        {
            
        }


        protected override void InitializeBoard()
        {
        }
    }
}
