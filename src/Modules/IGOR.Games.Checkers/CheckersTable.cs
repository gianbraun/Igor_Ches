using IGOR.Games.Checkers.Views;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;

namespace IGOR.Games.Checkers
{
    /// <summary>
    /// Memory efficient representation of Checkers game board.
    /// </summary>
    internal class CheckersTable : BoardBase, IEquatable<IBoard>
    {
        /// <summary>
        /// Gets the length and width of the board. 8 is the classic version.
        /// </summary>
        public override int Size => 8;

        public override Dictionary<FieldPosition, (double x, double y)> ValidPositions => CheckersBoardView.ValidPositions;

        /// <summary>
        /// Gets the table of cell stability values at starting position in a form of bit map.
        /// </summary>
        private const ulong StabilityCellsBitMap = 0x0UL;

        private const ulong KingCellsBitMap = 0x0UL;

        private const ulong KingPlayerCellsBitMap = 0x0UL;

        /// <summary>
        /// Flags indicating which cells are stable.
        /// </summary>
        private ulong stabilityTable;

        private ulong kingCellsTable;

        private ulong kingPlayerTable;

        public CheckersTable(CheckersTable oldTable) : base(oldTable)
        {
            stabilityTable = oldTable.stabilityTable;
            kingCellsTable = oldTable.kingCellsTable;
            kingPlayerTable = oldTable.kingPlayerTable;
        }

        public CheckersTable(Player humanPlayer)
            : base(humanPlayer)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckersTable"/> struct.
        /// </summary>
        /// <returns>Checkers table at the beginning of the game.</returns>
        protected override void InitializeBoard()
        {
            stabilityTable = StabilityCellsBitMap;
            kingCellsTable = KingCellsBitMap;
            kingPlayerTable = KingPlayerCellsBitMap;
            _field = new Player[8][];
            for (int i = 0; i < 8; i++)
            {
                _field[i] = new Player[8];
            }


            for (int i = 0; i < 4; i++)
            {
                _field[2 * i + 1][0] = AiPlayer;
                _field[2 * i + 1][2] = AiPlayer;
                _field[2 * i][1] = AiPlayer;

                _field[(Size - 1) - (2 * i + 1)][7] = HumanPlayer;
                _field[(Size - 1) - (2 * i + 1)][5] = HumanPlayer;
                _field[(Size - 1) - 2 * i][6] = HumanPlayer;
            }
             
        }

        public override void SetField(Player[][] newField, Player player)
        {
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    _field[x][y] = newField[x][y];

            UpdateKings();
            this.GetTableWithUpdatedStability();
        }

        private void UpdateKings()
        {
            for (int x = 0; x < Size; x++)
            {

                for (int y = 0; y < Size; y++)
                { 
                    if (GetPlayer(y, x) == HumanPlayer)
                        SetKing(HumanPlayer, y, x);

                    if (GetPlayer(y, x) == AiPlayer)
                        SetKing(AiPlayer, y, x);
                }
            }
        }

        //public Player GetWinner()
        //{
        //    if (CountOccupiedCells(AiPlayer) == 0)
        //        return HumanPlayer;
        //    else if (CountOccupiedCells(HumanPlayer) == 0)
        //        return AiPlayer;
        //    else
        //        return Player.None;
        //}


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

        public void SetKing(Player value, int row, int column)
        {
            ulong mask = 1UL << (row * Size + column);

            if (value == Player.None)
            {
                kingCellsTable &= ~mask;
                kingPlayerTable &= ~mask;
            }
            else
            {
                kingCellsTable |= mask;
                if (value == HumanPlayer)
                    kingPlayerTable |= mask;
                else
                    kingPlayerTable &= ~mask;
            }
        }

        public Player GetKing(int row, int column)
        {
            int index = row * Size + column;

            if (((kingCellsTable >> index) & 1UL) == 1UL)
            {
                return ((kingPlayerTable >> index) & 1UL) == 1UL
                    ? AiPlayer
                    : HumanPlayer;
            }

            return Player.None;
        }

        /// <summary>
        /// Gets the stability value by coordinates.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>Cell stability value.</returns>
        public bool GetStable(int row, int column)
        {
            int index = row * Size + column;
            return ((stabilityTable >> index) & 1UL) == 1UL;
        }

        /// <summary>
        /// Sets the stability value by coordinates.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void SetStable(bool value, int row, int column)
        {
            ulong mask = 1UL << (row * Size + column);

            if (value)
            {
                stabilityTable |= mask;
            }
            else
            {
                stabilityTable &= ~mask;
            }
        }


        /// <summary>
        /// Computes the number of occupied cells.
        /// </summary>
        /// <returns>The number of occupied cells.</returns>
        public int OccupiedCells()
        {
            int number = 0;
            foreach (var players in _field)
                foreach (var player in players)
                    if (player != Player.None)
                        number++;


            return number;
        }

        /// <summary>
        /// Computes the number of cells occupied by the player.
        /// </summary>
        /// <returns>The number of cells occupied by the player.</returns>
        public int CountOccupiedCells(Player player)
        {
            int number = 0;

            foreach (var column in _field)
                foreach (var value in column)
                    if (value == player)
                        number++;


            return number;
        }


        public int MaxPlayerKings()
        {
            return HammingWeightBitcount(kingPlayerTable);
        }
        public int MinPlayerKings()
        {
            return KingCells() - MaxPlayerKings();
        }
        public int KingCells()
        {
            return HammingWeightBitcount(kingCellsTable);
        }

        public bool Equals(IBoard other)
        {
            if (other is not CheckersTable checkersTable)
                return false;

            if (stabilityTable != checkersTable.stabilityTable)
                return false;

            if (kingCellsTable != checkersTable.kingCellsTable)
                return false;

            if (kingPlayerTable != checkersTable.kingPlayerTable)
                return false;

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (_field[x][y] != checkersTable._field[x][y])
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Counts the number of set bits in an unsigned long using Hamming's weight algorithm.
        /// </summary>
        /// <param name="map">The bitmap.</param>
        /// <returns>Number of set bits.</returns>

        private static int HammingWeightBitcount(ulong map)
        {
            map = map - ((map >> 1) & 0x5555555555555555UL);
            map = (map & 0x3333333333333333UL) + ((map >> 2) & 0x3333333333333333UL);
            return (int)(unchecked(((map + (map >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

    }
}
