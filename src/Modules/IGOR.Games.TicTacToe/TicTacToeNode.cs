using IGOR.Interface.Communication.Job;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace IGOR.Games.TicTacToe
{
    /// <summary>
    /// Tic-Tac-Toe game node.
    /// </summary>
    public class TicTacToeNode : INode
    {
        /// <summary>
        /// The state table.
        /// </summary>
        internal TicTacToeBoard Board { get; set; }
        IBoard INode.Board => Board;

        /// <summary>
        /// The winner.
        /// </summary>
        private readonly Lazy<Player> winner;

        /// <summary>
        /// The children of current node.
        /// </summary>
        private readonly Lazy<IReadOnlyList<TicTacToeNode>> children;

        /// <summary>
        /// The heuristic value of current node.
        /// </summary>
        private readonly Lazy<int> heuristics;

        /// <summary>
        /// Gets the current player.
        /// </summary>
        public Player Self { get; private set; }

        /// <summary>
        /// Gets the current player's opponent.
        /// </summary>
        public Player Opponent { get; private set; }

        public bool IsGameOver => !Children.Any();

        /// <summary>
        /// Gets the list of children nodes.
        /// </summary>
        public IReadOnlyList<INode> Children => children.Value;
        public IReadOnlyList<PortalCommand> PortalCommands { get; }

        /// <summary>
        /// Gets the heuristics value of current position.
        /// </summary>
        public int Heuristics => heuristics.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToeNode"/> class
        /// with an initial game state.
        /// </summary>
        public TicTacToeNode(Player humanPlayer, Player startPlayer = Player.White) :
            this(new TicTacToeBoard(humanPlayer), startPlayer, new List<PortalCommand>())
        {
        }

        // end node erstellen
        private TicTacToeNode(TicTacToeBoard table)
        {
            Board = table;
            PortalCommands = new List<PortalCommand>();
            children = new Lazy<IReadOnlyList<TicTacToeNode>>(
                () => new List<TicTacToeNode>().AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TicTacToeNode"/> class.
        /// </summary>
        /// <param name="table">The game state.</param>
        /// <param name="player">The current player.</param>
        private TicTacToeNode(TicTacToeBoard table, Player player, List<PortalCommand> commands)
        {
            Board = table;

            PortalCommands = commands;

            winner = new Lazy<Player>(
                () => IsFinished(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            children = new Lazy<IReadOnlyList<TicTacToeNode>>(
                () => GetChildren(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            heuristics = new Lazy<int>(
                () => GetHeuristics(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            Debug.Assert(player != Player.None, "Verifying that the player value is valid.");
            Self = player;
            Opponent = player == Player.Black
                ? Player.White
                : Player.Black;
        }

        /// <summary>
        /// Computes the child nodes.
        /// </summary>
        /// <returns>The list of child nodes.</returns>
        private IReadOnlyList<TicTacToeNode> GetChildren()
        {
            if (winner.Value != Player.None)
            {
                return new List<TicTacToeNode>().AsReadOnly();
            }

            return GetFreeCells().Select(move =>
            {
                ExecuteMove(move, out var board, out var commands);
                return new TicTacToeNode(board, Opponent, commands);
            }).ToList();
        }




        /// <summary>
        /// Compares to another Tic-Tac-Toe node.
        /// </summary>
        /// <param name="other">The other node.</param>
        /// <returns>Comparison result.</returns>
        public int CompareTo(INode other)
        {
            return Heuristics.CompareTo(other.Heuristics);
        }


        /// <summary>
        /// Gets the free cells.
        /// </summary>
        /// <returns>Enumeration of free cells.</returns>
        private IEnumerable<FieldPosition> GetFreeCells()
        {
            for (int y = 0; y < Board.Size; ++y)
            {
                for (int x = 0; x < Board.Size; ++x)
                {
                    if (Board[x, y] == Player.None)
                    {
                        yield return new FieldPosition(x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the game is finished.
        /// </summary>
        /// <returns>The winner of the game.</returns>
        private Player IsFinished()
        {
            // Check if at least one line is filled by the same player.
            foreach (var line in TicTacToeConstants.Lines)
            {
                var first = line[0];
                var current = Board[first.X, first.Y];

                for (int i = 1; current != Player.None && i < Board.Size; ++i)
                {
                    var val = Board[line[i].X, line[i].Y];
                    if (val != current)
                    {
                        current = Player.None;
                        break;
                    }
                }

                if (current != Player.None)
                {
                    return current;
                }
            }
            return Player.None;
        }

        /// <summary>
        /// Gets the table after given move.
        /// </summary>
        /// <param name="setPos">The move.</param>
        /// <returns>If the move was successful.</returns>
        private bool ExecuteMove(FieldPosition setPos, out TicTacToeBoard board, out List<PortalCommand> commands)
        {
            board = default;
            commands = null;
            if (Board[setPos.X, setPos.Y] == Player.None)
            {
                commands = new List<PortalCommand>();
                board = new TicTacToeBoard(Board);
                board[setPos.X, setPos.Y] = Self;
                commands.Add(new PortalCommand(setPos, setPos, MoveType.NewStone, Self));

                return true;
            }

            return false;
        }



        /// <summary>
        /// Computes the heuristics of current node.
        /// </summary>
        /// <returns>Heuristic value of current node</returns>
        private int GetHeuristics()
        {
            return winner.Value == Board.HumanPlayer
                ? TicTacToeConstants.VictoryPoints
                : winner.Value == Board.AiPlayer
                    ? -TicTacToeConstants.VictoryPoints
                    : GetWinPathsCount() * TicTacToeConstants.PotentialWinningLinePoints;
        }

        /// <summary>
        /// Gets score for potential win paths.
        /// </summary>
        /// <returns>Difference between players' number of paths not yet occupied by opponent.</returns>
        private int GetWinPathsCount()
        {
            int countX = 0;
            int countO = 0;

            foreach (var line in TicTacToeConstants.Lines)
            {
                bool containsX = false;
                bool containsO = false;

                foreach (Player cell in line.Select(position => Board[position.X, position.Y]))
                {
                    containsX |= cell == Board.HumanPlayer;
                    containsO |= cell == Board.AiPlayer;
                }

                countX += containsX ? 1 : 0;
                countO += containsO ? 1 : 0;
            }

            return countX - countO;
        }

        public INode NodeForMove(FieldPosition posStart, FieldPosition posEnd, FieldPosition oppStoneKick)
        {
            if (ExecuteMove(posStart, out var board, out var commands))
                return new TicTacToeNode(board, Opponent, commands);

            return null;
        }

        public Player GetWinner()
        {
            return IsFinished();
        }

        public INode SetField(Player[][] newField)
        {
            var field = (IReadOnlyCollection<IReadOnlyCollection<Player>>)newField;

            foreach (TicTacToeNode child in Children)
            {
                var boardEquals = true;
                for (int y = 0; y < child.Board.Size; y++)
                {
                    for (int x = 0; x < child.Board.Size; x++)
                    {
                        if (child.Board[x, y] != field.ElementAt(x).ElementAt(y))
                            boardEquals = false;
                    }
                }
                if (boardEquals)
                {
                    TicTacToeBoard newBoard = new (child.Board);
                    TicTacToeNode newNode = new(newBoard, Opponent, new List<PortalCommand>());
                    return newNode;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates an Node with GameOver = true
        /// </summary>
        /// <returns></returns>
        public INode NodeForEnd()
        {
            var endNode = new TicTacToeNode((TicTacToeBoard)Board);

            return endNode;
        }
    }
}
