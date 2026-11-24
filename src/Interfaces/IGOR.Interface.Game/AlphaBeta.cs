using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IGOR.Interface.Game
{
    /// <summary>
    /// Implementation of minimax search with alpha-beta pruning.
    /// </summary>
    /// <typeparam name="Node">Search node.</typeparam>
    public class AlphaBeta : IGameAi
    {

        Random random = new Random();

        /// <summary>
        /// Performs minimax search and returns the best child node.
        /// </summary>
        /// <param name="root">Initial state.</param>
        /// <returns>The best child node.</returns>
        public TNode Best<TNode>(TNode root, uint searchDepth) where TNode : INode
        {
            if (searchDepth == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(searchDepth), searchDepth, "Zero depth.");
            }
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root), "Null node.");
            }
            else if (root.Self == Player.None)
            {
                throw new ArgumentException(nameof(root.Self), "The player is not specified.");
            }

            // TODO: Maximieren vielleicht an anderen Merkmalen entscheiden (bspw. übrige Spielsteine, Schwierigkeitsgrad)?
            bool maximizing = root.Self == root.Board.HumanPlayer;

            //List<Task<Tuple<double, Node>>> tasks = new List<Task<Tuple<double, Node>>>();
            var results = new List<(double, TNode)>();
            foreach (TNode child in root.Children)
            {
                results.Add((Search(child, searchDepth - 1, double.NegativeInfinity, double.PositiveInfinity, !maximizing), child));
            }


            TNode bestNode = default;
            if (maximizing)
            {
                double bestValue = double.NegativeInfinity;

                foreach (var result in results)
                {
                    if (result.Item1 > bestValue | (result.Item1 == bestValue && random.Next(2) > 0))
                    {
                        bestNode = result.Item2;
                        bestValue = result.Item1;
                    }
                }
            }
            else
            {
                double bestValue = double.PositiveInfinity;

                foreach (var result in results)
                {
                    if (result.Item1 < bestValue | (result.Item1 == bestValue && random.Next(2) > 0))
                    {
                        bestNode = result.Item2;
                        bestValue = result.Item1;
                    }
                }
            }
            return bestNode;
        }

        /// <summary>
        /// Minimax search with alpha-beta pruning.
        /// </summary>
        /// <param name="node">Initial node.</param>
        /// <param name="depth">Search depth.</param>
        /// <param name="alpha">Alpha value.</param>
        /// <param name="beta">Beta value.</param>
        /// <param name="maximizing">Current player.</param>
        /// <returns>Heuristic value of current node for current player.</returns>
        private double Search(
            INode node,
            uint depth,
            double alpha,
            double beta,
            bool maximizing)
        {
            if (depth == 0 || !node.Children.Any())
            {
                return node.Heuristics;
            }

            if (maximizing)
            {
                double value = double.NegativeInfinity;

                foreach (INode child in node.Children)
                {
                    value = Math.Max(value, Search(child, depth - 1, alpha, beta, false));
                    alpha = Math.Max(alpha, value);
                    if (beta <= alpha)
                    {
                        // Beta-cutoff
                        break;
                    }
                }

                return value;
            }
            else
            {
                double value = double.PositiveInfinity;

                foreach (INode child in node.Children)
                {
                    value = Math.Min(value, Search(child, depth - 1, alpha, beta, true));
                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        // Alpha-cutoff
                        break;
                    }
                }

                return value;
            }
        }
    }
}
