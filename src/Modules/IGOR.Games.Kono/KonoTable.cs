using IGOR.Games.Kono.Views;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;

namespace IGOR.Games.Kono
{
    /// <summary>
    /// Memory efficient representation of Kono game board.
    /// </summary>
    internal class KonoTable : BoardBase
    {
        /// <summary>
        /// Gets the length and width of the board. 5 is the classic version.
        /// </summary>
        public override int Size => 5;

        public override Dictionary<FieldPosition, (double x, double y)> ValidPositions => KonoBoardView.ValidPositions;
        //public override Dictionary<FieldPosition, (double x, double y)> ValidPositions => throw new NotImplementedException();

        public KonoTable(KonoTable oldTable) : base(oldTable)
        {
            //_field = new Player[5][];
            //for (int i = 0; i < 5; i++)
            //{
            //    _field[i] = new Player[5];
            //}
            //for (int y = 0; y < Size; y++)
            //{
            //    for (int x = 0; x < Size; x++)
            //    {
            //        _field[x][y] = oldTable._field[x][y];
            //    }
            //}
        }

        //public KonoTable(IReadOnlyCollection<IReadOnlyCollection<Player>> field)
        //   : base(field)
        //{
        //}

        public KonoTable(Player humanPlayer)
            : base(humanPlayer)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KonoTable"/> struct.
        /// </summary>
        /// <returns>Kono table at the beginning of the game.</returns>
        protected override void InitializeBoard()
        {
            _field = new Player[5][];
            for (int i = 0; i < 5; i++)
            {
                _field[i] = new Player[5];
            }

            for (int i = 0; i < 5; i++)
            {
                // Opponent startet immer hinten
                _field[i][0] = AiPlayer;
                if (i == 0 || i == 4)
                {
                    _field[i][1] = AiPlayer;
                }

                // HumanPlayer startet immer vorne
                _field[i][4] = HumanPlayer;
                if (i == 0 || i == 4)
                {
                    _field[i][3] = HumanPlayer;
                }
            }
        }

        public override void SetField(Player[][] newField, Player player)
        {
            _field = newField;
        }

        public Player GetWinner()
        {
            if (TargetPosOpponent() == 7)
                return AiPlayer;
            else if (TargetPosHumanPlayer() == 7)
                return HumanPlayer;
            else
                return Player.None;
        }

        /// <summary>
        /// Gets the value by coordinates.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>Cell value.</returns>
        public Player GetPlayer(int row, int column)
        {
            return _field[column][row];
        }

        /// <summary>
        /// Sets the value by coordinates.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void SetPlayer(Player value, int row, int column)
        {
            _field[column][row] = value;
        }

        /// <summary>
        /// Computes the target cells occupied by the white player.
        /// </summary>
        /// <returns>The target cells occupied by the white player.</returns>
        public int TargetPosOpponent()
        {
            // Opponent muss die vorderen Felder belegen, um zu gewinnen
            int number = 0;
            for (int i = 0; i < 5; i++)
            {
                if (_field[i][4] == AiPlayer)
                {
                    number++;
                }
                if (i == 0 || i == 4)
                {
                    if (_field[i][3] == AiPlayer)
                    {
                        number++;
                    }
                }
            }
            return number;
        }

        /// <summary>
        /// Computes the target cells occupied by the black player.
        /// </summary>
        /// <returns>The target cells occupied by the black player.</returns>
        public int TargetPosHumanPlayer()
        {
            // HumanPlayer muss die hinteren Felder belegen um zu gewinnen
            int number = 0;
            for (int i = 0; i < 5; i++)
            {
                if (_field[i][0] == HumanPlayer)
                {
                    number++;
                }
                if (i == 0 || i == 4)
                {
                    if (_field[i][1] == HumanPlayer)
                    {
                        number++;
                    }
                }
            }
            return number;
        }
    }
}
