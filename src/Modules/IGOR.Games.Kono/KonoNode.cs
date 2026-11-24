using IGOR.Interface.Communication.Job;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace IGOR.Games.Kono
{

    /// <summary>
    /// An immutable representation of Kono game position.
    /// </summary>
    public class KonoNode : INode
    {

        /// <summary>
        /// The game state.
        /// </summary>
        internal KonoTable Board { get; set; }
        IBoard INode.Board => Board;

        /// <summary>
        /// The child nodes.
        /// </summary>
        private readonly Lazy<IReadOnlyList<KonoNode>> children;

        /// <summary>
        /// The game state heuristics.
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

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        public IReadOnlyList<INode> Children => children.Value;

        public IReadOnlyList<PortalCommand> PortalCommands { get; }

        /// <summary>
        /// Gets heuristics of current position.
        /// </summary>
        public int Heuristics => heuristics.Value;

        public bool IsGameOver => !Children.Any();

        private Difficulty _difficulty;

        // Spiel startet immer mit weiß
        public KonoNode(Player humanPlayer, Difficulty difficulty, Player startPlayer = Player.White) : this(
           new KonoTable(humanPlayer),
           startPlayer, new List<PortalCommand>(), difficulty)
        {
            _difficulty = difficulty;
        }

        private KonoNode(KonoTable table)
        {
            Board = table;
            PortalCommands = new List<PortalCommand>();
            children = new Lazy<IReadOnlyList<KonoNode>>(
                () => new List<KonoNode>().AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KonoNode" /> class.
        /// </summary>
        /// <param name="table">The game state.</param>
        /// <param name="currentPlayer">Current player.</param>
        private KonoNode(KonoTable table, Player currentPlayer, List<PortalCommand> commands, Difficulty difficulty)
        {
            _difficulty = difficulty;
            Board = table;

            heuristics = new Lazy<int>(
                () => GetHeuristics(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            children = new Lazy<IReadOnlyList<KonoNode>>(
                () => GetChildren(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            if (commands.Count == 0)
            {
                for (int y = 0; y < Board.Size; y++)
                {
                    for (int x = 0; x < Board.Size; x++)
                    {
                        if (Board[x, y] == Player.None)
                            continue;
                        commands.Add(new PortalCommand(
                            new FieldPosition(0, 0),
                            new FieldPosition(x, y),
                            MoveType.NewStone,
                            Board[x, y]));
                    }
                }
            }

            PortalCommands = commands;

            // Weiß fängt immer an
            Self = currentPlayer;
            Opponent = currentPlayer == Player.Black ? Player.White : Player.Black;
        }

        /// <summary>
        /// Gets the cell value.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>The cell value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">row, column;Row or column index out of range.</exception>
        public Player GetPlayer(int row, int column)
        {
            if (row < 0 || row >= Board.Size ||
                column < 0 || column >= Board.Size)
            {
                throw new ArgumentOutOfRangeException("row, column", "Row or column index out of range.");
            }

            return Board.GetPlayer(row, column);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("  ");
            for (int i = 0; i < Board.Size; ++i)
            {
                sb.Append($"{i} ");
            }

            sb.AppendLine();
            for (int i = 0; i < Board.Size; ++i)
            {
                sb.Append($"{i}|");
                for (int j = 0; j < Board.Size; ++j)
                {
                    Player p = GetPlayer(i, j);
                    if (p == Player.None)
                    {
                        sb.Append('-');
                    }
                    else if (p == Player.Black)
                    {
                        sb.Append('b');
                    }
                    else
                    {
                        sb.Append('w');
                    }

                    if (j < Board.Size - 1)
                    {
                        sb.Append(' ');
                    }
                }

                sb.Append('|');
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Computes all children nodes of current node.
        /// </summary>
        /// <returns>Children nodes.</returns>
        private IReadOnlyList<KonoNode> GetChildren()
        {
            List<KonoNode> children = new();
            if (GetWinner() != Player.None)
                return children.AsReadOnly();

            foreach ((var position, var validMoves) in GetValidMoves(Self))
            {
                foreach (FieldPosition move in validMoves)
                {
                    (var table, var commands) = ExecuteMove(Board, new(), position, move);
                    children.Add(new KonoNode(table, Opponent, commands, _difficulty));
                }
            }
            return children.AsReadOnly();
        }

        /// <summary>
        /// Returns the table after given move.
        /// </summary>
        /// <param name="move">The move.</param>
        /// <returns>The table after given move.</returns>
        private (KonoTable table, List<PortalCommand> commands) ExecuteMove(KonoTable table, List<PortalCommand> commands, FieldPosition pos, FieldPosition move)
        {
            FieldPosition newPos = new(pos.X + move.X, pos.Y + move.Y);
            KonoTable tableAfterMove = new(table);
            var portalCmds = commands;
            tableAfterMove.SetPlayer(Player.None, pos.Y, pos.X);             
            portalCmds.Add(new PortalCommand(pos, newPos, MoveType.MoveStone, Self));
            tableAfterMove.SetPlayer(Self, newPos.Y, newPos.X);
            return (tableAfterMove, portalCmds);
        }

        /// <summary>
        /// Candidates the cells.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <returns>List of valid moves.</returns>
        private IEnumerable<(FieldPosition position, IEnumerable<FieldPosition> validMoves)> GetValidMoves(Player player)
        {
            foreach (FieldPosition pos in GetStonePositions(player))
            {
                IList<FieldPosition> moves;
                moves = GetPossibleMoves(pos).ToList();

                if (moves.Count > 0)
                {
                    yield return (pos, moves);
                }
            }
        }

        /// <summary>
        /// Finds all empty cells.
        /// </summary>
        /// <returns>The list of empty cells.</returns>
        private IEnumerable<FieldPosition> GetStonePositions(Player Player)
        {
            for (int y = 0; y < Board.Size; ++y)
                for (int x = 0; x < Board.Size; ++x)
                    if (Board.GetPlayer(y, x) == Player)
                        yield return new FieldPosition(x, y);
        }

        /// <summary>
        /// Gets directions in which the specified move flips opponent's cells.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <param name="stonePos">The move represented by cell coordinates.</param>
        /// <returns>Directions in which the specified move flips opponent's cells.</returns>
        private IEnumerable<FieldPosition> GetPossibleMoves(FieldPosition stonePos)
        {
            return KonoConstants.Directions.Where(direction => CheckValidMove(Board, Self, stonePos, direction));
        }

        /// <summary>
        /// Determines whether the specified move flips opponent's cells in given direction.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <param name="startPos">The move represented by cell coordinates.</param>
        /// <param name="direction">The direction represented by coordinate deltas.</param>
        /// <returns>True if the move flips opponent's cells in given direction, false otherwise.</returns>
        private bool CheckValidMove(KonoTable table, Player player, FieldPosition startPos, FieldPosition direction)
        {
            Debug.Assert(player != Player.None, "Verifying that the player value is valid.");

            int newCol = startPos.X + direction.X;
            int newRow = startPos.Y + direction.Y;

            if (newRow >= 0 && newRow < Board.Size && newCol >= 0 && newCol < Board.Size && table.GetPlayer(newRow, newCol) == Player.None)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Computes the heuristics of current position.
        /// </summary>
        /// <returns>Heuristic value of current position.</returns>
        private int GetHeuristics()
        {
            Player winner = GetWinner();
            if (winner == Board.HumanPlayer)
                return KonoConstants.VictoryPoints;
            else if (winner == Board.AiPlayer)
                return -KonoConstants.VictoryPoints;
            else if (_difficulty == Difficulty.Unbeatable)
                return GetNumberStoneInGoalScore() + GetPogressScore() + GetBlockScore() + GetNumberMovesScore();
            else if (_difficulty == Difficulty.Hard || _difficulty == Difficulty.Normal)
                return GetNumberStoneInGoalScore() + GetPogressScore() + GetNumberMovesScore();
            else
                return GetNumberStoneInGoalScore() + GetPogressScore();
        }

        private int GetNumberStoneInGoalScore()
        {
            // Zählt die Spielsteine die bereits in Zielposition sind und gibt Punkte
            int count = 0;
            if (Self == Board.HumanPlayer)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Board.GetPlayer(4, i) == Board.AiPlayer)
                    {
                        count--;
                    }
                    if (i == 0 || i == 4)
                    {
                        if (Board.GetPlayer(3, i) == Board.AiPlayer)
                        {
                            count--;
                        }
                    }
                }
            }
            else if (Self == Board.AiPlayer)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Board.GetPlayer(0, i) == Board.HumanPlayer)
                    {
                        count++;
                    }
                    if (i == 0 || i == 4)
                    {
                        if (Board.GetPlayer(1, i) == Board.HumanPlayer)
                        {
                            count++;
                        }
                    }
                }
            }
            return count * KonoConstants.GoalPoints;
        }

        private int GetPogressScore()
        {
            // Gibt Punkte, je weiter die Spielsteine nach vorne gerückt sind
            int score = 0;
            if (Self == Board.HumanPlayer)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (Board.GetPlayer(i, j) == Board.AiPlayer)
                        {
                            score -= i;
                        }
                    }
                }
            }
            else if (Self == Board.AiPlayer)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (Board.GetPlayer(i, j) == Board.HumanPlayer)
                        {
                            score += 4 - i;
                        }
                    }
                }
            }
            return score * KonoConstants.ProgressPoints;
        }

        private int GetBlockScore()
        {
            // Gibt Punkte, wenn ein gegnerischer Spielstein blockiert wird
            int score = 0;
            if (Self == Board.HumanPlayer)
            {
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 5; x++)
                    {
                        if (Board.GetPlayer(y, x) == Board.HumanPlayer)
                        {
                            if ((y != 0) &&
                                ((x == 0 && Board.GetPlayer(y - 1, x + 1) == Board.AiPlayer) ||
                                 (x == 4 && Board.GetPlayer(y - 1, x - 1) == Board.AiPlayer) ||
                                 (x != 0 && x != 4 && Board.GetPlayer(y - 1, x + 1) == Board.AiPlayer && Board.GetPlayer(y - 1, x - 1) == Board.AiPlayer)))
                            {
                                score -= 1;
                            }
                        }
                    }
                }
            }
            if (Self == Board.AiPlayer)
            {
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 5; x++)
                    {
                        if (Board.GetPlayer(y, x) == Board.AiPlayer)
                        {
                            if ((y != 4) &&
                                ((x == 0 && Board.GetPlayer(y + 1, x + 1) == Board.HumanPlayer) ||
                                 (x == 4 && Board.GetPlayer(y + 1, x - 1) == Board.HumanPlayer) ||
                                 (x != 0 && x != 4 && Board.GetPlayer(y + 1, x + 1) == Board.HumanPlayer && Board.GetPlayer(y + 1, x - 1) == Board.HumanPlayer)))
                            {
                                score += 1;
                            }
                        }
                    }
                }
            }
            return score * KonoConstants.BlockPoints;
        }

        private int GetNumberMovesScore()
        {
            // TODO: was passiert hier?
            int score;
            score = Children.Count >= KonoConstants.MoveAdvantageNumber ? KonoConstants.MoveAdvantageNumber : Children.Count;
            score *= KonoConstants.PossibleMovesPoints;
            score = Self == Board.AiPlayer ? score : -score;
            return score;
        }

        public Player GetWinner()
        {
            return Board.GetWinner();
        }

        public INode SetField(Player[][] newField)
        {
            var field = (IReadOnlyCollection<IReadOnlyCollection<Player>>)newField;
            foreach (KonoNode child in Children)
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
                if (boardEquals) { 
                    KonoTable newBoard = new(child.Board);
                    KonoNode newNode = new(newBoard, Opponent, new List<PortalCommand>(), _difficulty);
                    return newNode;
                }
            }

            return null;
        }

        public INode NodeForMove(FieldPosition posStart, FieldPosition posEnd, FieldPosition optKick)
        {
            FieldPosition diffPos = new(posEnd.X - posStart.X, posEnd.Y - posStart.Y);
            if (diffPos.Y == 0 || diffPos.X == 0 || Board[posStart.X, posStart.Y] != Self)
                return null;

            FieldPosition direction = new(diffPos.X > 0 ? 1 : -1, diffPos.Y > 0 ? 1 : -1);
            bool validDirection = false;
            foreach (FieldPosition dir in KonoConstants.Directions)
                if (dir.Equals(direction))
                    validDirection = true;

            if (CheckValidMove(Board, Self, posStart, direction) && validDirection)
            {
                (var table, var commands) = ExecuteMove(Board, new(), posStart, direction);
                foreach (KonoNode child in Children)
                {
                    bool same = true;
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            if (child.Board.Field.ElementAt(i).ElementAt(j) != table.Field.ElementAt(i).ElementAt(j))
                            {
                                same = false;
                                break;
                            }
                        }
                        if (!same) break;
                    }
                    if (same)
                    {
                        return new KonoNode(table, Opponent, commands, _difficulty);
                    }
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
            var endNode = new KonoNode((KonoTable)Board);

            return endNode;
        }

        public int CompareTo(INode other)
        {
            return Heuristics.CompareTo(other.Heuristics);
        }
    }
}
