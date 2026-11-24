using System;
using System.Collections.Generic;

namespace IGOR.Interface.Game
{
    /// <summary>
    /// Interface for an immutable state of a zero-sum game for 2 players.
    /// The node implements IComparable to allow sorting decisions in the search tree.
    /// Default implementation of comparison would simply compare heuristic values.
    /// </summary>
    public interface INode : IComparable<INode>
    {
        /// <summary>
        /// Gets the current player.
        /// </summary>
        Player Self { get; }

        /// <summary>
        /// Gets the board of the node
        /// </summary>
        IBoard Board { get; }

        /// <summary>
        /// Gets the opponent of current player.
        /// </summary>
        Player Opponent { get; }

        /// <summary>
        /// States whether the Node results in a game over
        /// </summary>
        bool IsGameOver { get; }

        /// <summary>
        /// Gets the list of children nodes. -> next possible nodes?
        /// </summary>
        IReadOnlyList<INode> Children { get; }


        IReadOnlyList<PortalCommand> PortalCommands { get; }

        /// <summary>
        /// Gets the heuristics value of current position
        /// from maximizing player's perspective.
        /// </summary>
        int Heuristics { get; }

        Player GetWinner();

        /// <summary>
        /// Creates a Node for moving a stone.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="direc"></param>
        /// <param name="oppStoneKick"></param>
        /// <returns></returns>
        INode NodeForMove(FieldPosition pos, FieldPosition direc, FieldPosition oppStoneKick);

        /// <summary>
        /// Creates a Node to end the game.
        /// </summary>
        /// <returns></returns>
        INode NodeForEnd();

        INode SetField(Player[][] newField);
    }
}
