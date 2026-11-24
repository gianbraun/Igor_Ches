using IGOR.Games.Morris.Views;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IGOR.Games.Morris
{
    /// <summary>
    /// Memory efficient representation of Morris game board.
    /// </summary>
    internal class MorrisTable : BoardBase
    {
        /// <summary>
        /// Gets the length and width of the board. 8 is the classic version.
        /// </summary>
        public override int Size => 7;

        public override Dictionary<FieldPosition, (double x, double y)> ValidPositions => MorrisBoardView.ValidPositions;

        private const ulong StabilityCellsBitMap = 0x0UL;

        private const ushort StonesAvailableStart = 9;


        private ulong stabilityTable;

        private ushort stonesAvailableBlack;
        private ushort stonesAvailableWhite;

        public MorrisTable(Player humanPlayer)
            : base(humanPlayer)
        {
            stabilityTable = StabilityCellsBitMap;
            stonesAvailableBlack = StonesAvailableStart;
            stonesAvailableWhite = StonesAvailableStart;
        }

        public MorrisTable(MorrisTable oldField)
            : base(oldField)
        {
            stabilityTable = oldField.stabilityTable;
            stonesAvailableWhite = oldField.GetStonesAvailable(Player.White);
            stonesAvailableBlack = oldField.GetStonesAvailable(Player.Black);
        }

        protected override void InitializeBoard()
        {
            
        }


        public override void SetField(Player[][] newField, Player self)
        {
            _field = newField;
            //if(self == Player.Black)
            //    stonesAvailableBlack -= (ushort)(stonesAvailableBlack > 0 ? 1 : 0);
            //else
            //    stonesAvailableWhite -= (ushort)(stonesAvailableWhite > 0 ? 1 : 0);

            UpdateMills();
        }


        public bool GetStable(int x, int y)
        {
            int index = y * Size + x;
            return ((stabilityTable >> index) & 1UL) == 1UL;
        }

        public void SetStable(bool value, int x, int y)
        {
            ulong mask = 1UL << (y * Size + x);

            if (value)
            {
                stabilityTable |= mask;
            }
            else
            {
                stabilityTable &= ~mask;
            }
        }


        public ushort GetStonesAvailable(Player player)
        {
            return player == Player.Black ? stonesAvailableBlack : stonesAvailableWhite;
        }

        public void DecreaseStonesAvailable(Player player)
        {
            if (player == Player.Black)
            {
                if (stonesAvailableBlack > 0)
                    stonesAvailableBlack--;
            }
            else
            {
                if (stonesAvailableWhite > 0)
                    stonesAvailableWhite--;
            }

        }

        public int StableCellScore(Player player)
        {
            int score = 0;

            for (int y = 0; y < Size; ++y)
            {
                for (int x = 0; x < Size; ++x)
                {
                    if (GetStable(y, x))
                    {
                        score += _field[x][y] == player ? 1 : -1;
                    }
                }
            }
            return score;
        }

        public void UpdateMills()
        {
            stabilityTable = 0x0UL;
            foreach (var possibleMill in MorrisConstants.MillLines)
            {
                bool max = true;
                bool min = true;

                foreach (var pos in possibleMill)
                {
                    Player player = _field[pos.X][pos.Y];
                    max &= player == Player.Black;
                    min &= player == Player.White;
                    if (!max && !min)
                        break;
                }
                if(max || min)
                    foreach (var pos in possibleMill)
                        SetStable(true, pos.X, pos.Y);
            }
        }

        /// <summary>
        /// Computes the number of cells occupied by the player.
        /// </summary>
        /// <returns>The number of cells occupied by the player.</returns>
        public int PlayerCells(Player player)
        {
            return Field.Sum(f => f.Sum(p => p == player ? 1 : 0));
        }
    }
}
