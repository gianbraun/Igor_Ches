namespace IGOR.Games.Checkers
{
    using IGOR.Interface.Game;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The logic for computing heuristic value of Checkers position.
    /// </summary>
    internal static class CheckersHeuristics
    {
        /// <summary>
        /// Returns the table with updated stability values after given move.
        /// </summary>
        /// <param name="table">Table after the move with original stability state.</param>
        /// <returns>The table after given move with updated stability values.</returns>
        public static void GetTableWithUpdatedStability(this CheckersTable table)
        {
            // Start with all currently unstable cells.
            Queue<FieldPosition> unstableCells = new Queue<FieldPosition>(table.GetUnstableCells().ToList());

            // Process all unstable cells.
            while (unstableCells.Count > 0)
            {
                FieldPosition candidate = unstableCells.Dequeue();

                if (!table.GetStable(candidate.Y, candidate.X) &&
                    table.IsStable(candidate))
                {
                    // Mark the cell as stable.
                    table.SetStable(true, candidate.Y, candidate.X);

                    // All unstable neighbors need to be checked again.
                    foreach (FieldPosition delta in CheckersConstants.StabilityDirections)
                    {
                        FieldPosition neighbor = new FieldPosition(candidate.X + delta.X, candidate.Y + delta.Y);
                        if (neighbor.Y >= 0 && neighbor.Y < table.Size &&
                            neighbor.X >= 0 && neighbor.X < table.Size &&
                            table.GetPlayer(neighbor.Y, neighbor.X) != Player.None &&
                            !table.GetStable(neighbor.Y, neighbor.X))
                        {
                            unstableCells.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of currently unstable occupied cells that are potentially stable.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns>The list of unstable cells.</returns>
        public static IEnumerable<FieldPosition> GetUnstableCells(this CheckersTable table)
        {
            for (int i = 0; i < table.Size; ++i)
            {
                for (int j = 0; j < table.Size; ++j)
                {
                    if (table.GetPlayer(i, j) != Player.None && !table.GetStable(i, j))
                    {
                        yield return new FieldPosition(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified cell is stable.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="cell">The cell.</param>
        /// <returns>True if the cell is stable, false otherwise.</returns>
        public static bool IsStable(this CheckersTable table, FieldPosition cell)
        {
            if (table.GetPlayer(cell.Y, cell.X) == Player.None)
            {
                return false;
            }

            // If cell is at the edge, it's stable.
            if (cell.Y == 0 || cell.Y == table.Size - 1 ||
                cell.X == 0 || cell.X == table.Size - 1)
            {
                return true;
            }

            foreach (FieldPosition delta in CheckersConstants.StabilityDirections)
            {
                Player player = table.GetPlayer(cell.Y, cell.X);
                FieldPosition dir = new(cell.X + delta.X, cell.Y + delta.Y);

                // If at least one of the neighbors is a stable cell belonging to the same player,
                // the direction is stable.
                if (table.GetStable(dir.Y, dir.X) && table.GetPlayer(dir.Y, dir.X) == player)
                    continue;
                else
                    return false;
            }

            // No unstable directions found.
            return true;
        }

        /// <summary>
        /// Gets the stability score.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns>The difference between maximizing and minimizing players' stable cells.</returns>
        public static int GetStabilityScore(this CheckersTable table)
        {
            int score = 0;

            for (int i = 0; i < table.Size; ++i)
            {
                for (int j = 0; j < table.Size; ++j)
                {
                    if (table.GetStable(i, j))
                    {
                        score += table.GetPlayer(i, j) == table.HumanPlayer ? 1 : -1;
                    }
                }
            }

            return score;
        }
    }
}
