using IGOR.Interface.Game;
using System.Collections.Generic;

namespace IGOR.Games.Kono
{
    /// <summary>
    /// Constants for Kono game.
    /// </summary>
    internal static class KonoConstants
    {
        public const int VictoryPoints = 100000;
        public const int GoalPoints = 500;
        public const int PossibleMovesPoints = 5;
        public const int ProgressPoints = 15;
        public const int BlockPoints = 5;
        public const int MoveAdvantageNumber = 10;
        /// <summary>
        /// 4 possible directions.
        /// </summary>
        public static readonly IReadOnlyList<FieldPosition> Directions = new List<FieldPosition>
        {
            new FieldPosition(-1, -1),  new FieldPosition(1, -1),
            new FieldPosition(-1, 1),   new FieldPosition(1, 1)
        }.AsReadOnly();
    }
}
