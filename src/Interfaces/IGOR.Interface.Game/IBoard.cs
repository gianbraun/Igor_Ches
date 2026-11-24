using System;
using System.Collections.Generic;

namespace IGOR.Interface.Game
{
    public interface IBoard
    {
        int Size { get; }

        Player HumanPlayer { get; }
        Player AiPlayer { get; }

        /// <summary>
        /// Zulässige Spielfeldpositionen
        /// </summary>
        Dictionary<FieldPosition, (double x, double y)> ValidPositions { get; }

        /// <summary>
        /// Spielfeld mit Spielsteinen (durch Enumeration Spieler dargestellt)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Player this[int x, int y] { get; }

        /// <summary>
        /// ReadOnlyCollection des Spielfeldes
        /// </summary>
        IReadOnlyCollection<IReadOnlyCollection<Player>> Field { get; }

        /// <summary>
        /// Setzt ein neues Spielfeld
        /// </summary>
        /// <param name="newField"></param>
        /// <param name="actualPlayer"></param>
        void SetField(Player[][] newField, Player actualPlayer);

        /// <summary>
        /// Initializes the Board
        /// </summary>
        void InitializeBoard();
    }
}
