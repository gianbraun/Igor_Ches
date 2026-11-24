using IGOR.Interface.Game;
using System.Collections.Generic;

namespace IGOR.Games.TicTacToe
{
    /// <summary>
    /// Tic-Tac-Toe constants.
    /// </summary>
    internal static class TicTacToeConstants
    {
        /// <summary>
        /// Gets the potential winning line points.
        /// </summary>
        public const int PotentialWinningLinePoints = 1;

        /// <summary>
        /// Gets the points for victory.
        /// </summary>
        public const int VictoryPoints = 1000;

        /// <summary>
        /// All Tic-Tac-Toe game lines.
        /// </summary>
        public static readonly IReadOnlyList<IReadOnlyList<FieldPosition>> Lines =
            new List<IReadOnlyList<FieldPosition>>
            {
                new List<FieldPosition> { new FieldPosition(0, 0), new FieldPosition(0, 1), new FieldPosition(0, 2) },
                new List<FieldPosition> { new FieldPosition(1, 0), new FieldPosition(1, 1), new FieldPosition(1, 2) },
                new List<FieldPosition> { new FieldPosition(2, 0), new FieldPosition(2, 1), new FieldPosition(2, 2) },
                new List<FieldPosition> { new FieldPosition(0, 0), new FieldPosition(1, 0), new FieldPosition(2, 0) },
                new List<FieldPosition> { new FieldPosition(0, 1), new FieldPosition(1, 1), new FieldPosition(2, 1) },
                new List<FieldPosition> { new FieldPosition(0, 2), new FieldPosition(1, 2), new FieldPosition(2, 2) },
                new List<FieldPosition> { new FieldPosition(0, 0), new FieldPosition(1, 1), new FieldPosition(2, 2) },
                new List<FieldPosition> { new FieldPosition(2, 0), new FieldPosition(1, 1), new FieldPosition(0, 2) },
            }.AsReadOnly();
    }
}
