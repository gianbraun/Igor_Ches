namespace IGOR.Games.Morris
{
    using IGOR.Interface.Game;
    using System.Collections.Generic;

    /// <summary>
    /// Constants for Morris game.
    /// </summary>
    internal static class MorrisConstants
    {

        /// <summary>
        /// Gets the stability points per cell.
        /// </summary>
        public const int MobilityPoints = 5;

        public const int VictoryPoints = 100000;

        public const int StonePoints = 300;

        public const int MillPoints = 20;

        public const int StabilityPoints = 10;


        /// <summary>
        /// 2 possible directions from max Player.
        /// </summary>
        public static readonly IReadOnlyList<FieldPosition> DirectionsMove = new List<FieldPosition>
        {
                        new FieldPosition(0, -1),
            new FieldPosition(-1, 0),    new FieldPosition(1, 0),
                        new FieldPosition(0, 1)
        }.AsReadOnly();


        /// <summary>
        /// invalid FieldPositions
        /// </summary>
        public static readonly IReadOnlyList<FieldPosition> InvalidPositions = new List<FieldPosition>
        {
            new FieldPosition(1, 0), new FieldPosition(2, 0), new FieldPosition(4, 0), new FieldPosition(5, 0),
            new FieldPosition(0, 1), new FieldPosition(2, 1), new FieldPosition(4, 1), new FieldPosition(6, 1),
            new FieldPosition(0, 2), new FieldPosition(1, 2), new FieldPosition(5, 2), new FieldPosition(6, 2),
                                        new FieldPosition(3, 3),
            new FieldPosition(0, 4), new FieldPosition(1, 4), new FieldPosition(5, 4), new FieldPosition(6, 4),
            new FieldPosition(0, 5), new FieldPosition(2, 5), new FieldPosition(4, 5), new FieldPosition(6, 5),
            new FieldPosition(1, 6), new FieldPosition(2, 6), new FieldPosition(4, 6), new FieldPosition(5, 6),
        }.AsReadOnly();

        /// <summary>
        /// All Morris mill lines.
        /// </summary>
        public static readonly IReadOnlyList<IReadOnlyList<FieldPosition>> MillLines =
            new List<IReadOnlyList<FieldPosition>>
            {
                new List<FieldPosition> { new FieldPosition(0, 0), new FieldPosition(3, 0), new FieldPosition(6, 0) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(1, 1), new FieldPosition(3, 1), new FieldPosition(5, 1) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(2, 2), new FieldPosition(3, 2), new FieldPosition(4, 2) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(0, 3), new FieldPosition(1, 3), new FieldPosition(2, 3) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(4, 3), new FieldPosition(5, 3), new FieldPosition(6, 3) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(2, 4), new FieldPosition(3, 4), new FieldPosition(4, 4) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(1, 5), new FieldPosition(3, 5), new FieldPosition(5, 5) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(0, 6), new FieldPosition(3, 6), new FieldPosition(6, 6) }.AsReadOnly(),


                new List<FieldPosition> { new FieldPosition(0, 0), new FieldPosition(0, 3), new FieldPosition(0, 6) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(1, 1), new FieldPosition(1, 3), new FieldPosition(1, 5) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(2, 2), new FieldPosition(2, 3), new FieldPosition(2, 4) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(3, 0), new FieldPosition(3, 1), new FieldPosition(3, 2) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(3, 4), new FieldPosition(3, 5), new FieldPosition(3, 6) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(4, 2), new FieldPosition(4, 3), new FieldPosition(4, 4) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(5, 1), new FieldPosition(5, 3), new FieldPosition(5, 5) }.AsReadOnly(),
                new List<FieldPosition> { new FieldPosition(6, 0), new FieldPosition(6, 3), new FieldPosition(6, 6) }.AsReadOnly()
            }.AsReadOnly();
    }
}
