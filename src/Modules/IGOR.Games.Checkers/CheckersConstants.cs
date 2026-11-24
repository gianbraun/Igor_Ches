using IGOR.Interface.Game;
using System.Collections.Generic;

namespace IGOR.Games.Checkers
{
    /// <summary>
    /// Constants for checkers game.
    /// </summary>
    internal static class CheckersConstants
    {

        /// <summary>
        /// Gets the stability points per cell.
        /// </summary>
        public const int StabilityPoints = 20;

        public const int VictoryPoints = 100000;

        public const int StonePoints = 100;

        public const int KingPoints = 500;

        public const int PossibleMovesPoints = 30;

        public const int MoveAdvantageNumber = 6;

        /// <summary>
        /// 28 possible moves from king.
        /// </summary>
        public static readonly IReadOnlyList<FieldPosition> DirectionsKing = new List<FieldPosition>
        {
            new FieldPosition(-1, -1),   new FieldPosition(-1, 1),
            new FieldPosition(1, -1),    new FieldPosition(1, 1),

            new FieldPosition(-2, -2),   new FieldPosition(-2, 2),
            new FieldPosition(2, -2),    new FieldPosition(2, 2),

            new FieldPosition(-3, -3),   new FieldPosition(-3, 3),
            new FieldPosition(3, -3),    new FieldPosition(3, 3),

            new FieldPosition(-4, -4),   new FieldPosition(-4, 4),
            new FieldPosition(4, -4),    new FieldPosition(4, 4),

            new FieldPosition(-5, -5),   new FieldPosition(-5, 5),
            new FieldPosition(5, -5),    new FieldPosition(5, 5),

            new FieldPosition(-6, -6),   new FieldPosition(-6, 6),
            new FieldPosition(6, -6),    new FieldPosition(6, 6),

            new FieldPosition(-7, -7),   new FieldPosition(-7, 7),
            new FieldPosition(7, -7),    new FieldPosition(7, 7)
        }.AsReadOnly();

        /// <summary>
        /// 2 possible directions from max Player.
        /// </summary>
        public static readonly IReadOnlyList<FieldPosition> DirectionsBottomPlayer = new List<FieldPosition>
        {
            new FieldPosition(-1, -1),    new FieldPosition(1, -1)
        }.AsReadOnly();

        /// <summary>
        /// 2 possible directions from min Player.
        /// </summary>
        public static readonly IReadOnlyList<FieldPosition> DirectionsTopPlayer = new List<FieldPosition>
        {
            new FieldPosition(-1, 1),   new FieldPosition(1, 1)
        }.AsReadOnly();

        /// <summary>
        /// 2 directions in which the cell must be limited at least from one side with a stable cell
        /// of the same color in order to be stable.
        /// </summary>
        public static readonly IReadOnlyList<FieldPosition> StabilityDirections = new List<FieldPosition>
        {
            new FieldPosition(-1, -1),   new FieldPosition(-1, 1),

            new FieldPosition(1, -1),    new FieldPosition(1, 1)
        }.AsReadOnly();
    }
}
