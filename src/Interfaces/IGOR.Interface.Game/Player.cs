using System.ComponentModel;

namespace IGOR.Interface.Game
{
    /// <summary>
    /// Possible game values.
    /// </summary>
    public enum Player
    {
        /// <summary>
        /// Not specified player or empty cell.
        /// </summary>
        None,

        /// <summary>
        /// The maximizing player which starts the game.
        /// </summary>
        [Description("Schwarz")]
        Black,

        /// <summary>
        /// The minimizing player.
        /// </summary>
        [Description ("Weiß")]
        White
    }
}
