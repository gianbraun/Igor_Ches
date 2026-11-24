using IGOR.Interface.Communication.Job;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace IGOR.Games.Checkers
{
    /// <summary>
    /// An immutable representation of Checkers game position.
    /// </summary>
    public class CheckersNode : INode
    {

        /// <summary>
        /// The game state.
        /// </summary>
        internal CheckersTable Board { get; private set; }

        IBoard INode.Board => Board;

        /// <summary>
        /// The child nodes.
        /// </summary>
        private readonly Lazy<IReadOnlyList<CheckersNode>> children;

        /// <summary>
        /// The game state heuristics.
        /// </summary>
        private readonly Lazy<int> heuristics;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckersNode" /> class with the starting position.
        /// </summary>


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

        public bool IsGameOver => !Children.Any();

        /// <summary>
        /// Gets heuristics of current position.
        /// </summary>
        public int Heuristics => heuristics.Value;

        public CheckersNode(Player humanPlayer, Player startPlayer = Player.White) : this(
           new CheckersTable(humanPlayer),
           startPlayer, new List<PortalCommand>())
        {
        }

        private CheckersNode(CheckersTable table)
        {
            Board = table;
            PortalCommands = new List<PortalCommand>();
            children = new Lazy<IReadOnlyList<CheckersNode>>(
                () => new List<CheckersNode>().AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckersNode" /> class.
        /// </summary>
        /// <param name="table">The game state.</param>
        /// <param name="currentPlayer">Current player.</param>
        /// <param name="playerCanPass">Indicates whether the current player can pass a move.</param>
        private CheckersNode(CheckersTable table, Player currentPlayer, List<PortalCommand> commands)
        {



            Board = table;

            heuristics = new Lazy<int>(
                () => GetHeuristics(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            children = new Lazy<IReadOnlyList<CheckersNode>>(
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

            Self = currentPlayer;
            Opponent = currentPlayer == Player.Black
                ? Player.White
                : Player.Black;
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
        /// Compares to another Checkers node.
        /// </summary>
        /// <param name="other">The other node to compare with.</param>
        /// <returns>Comparison result.</returns>
        public int CompareTo(INode other)
        {
            return Heuristics.CompareTo(other.Heuristics);
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
                    Player king = Board.GetKing(i, j);
                    if (p == Player.None)
                    {
                        sb.Append('-');
                    }
                    else if (p == Player.Black)
                    {
                        if (king == p)
                            sb.Append('B');
                        else
                            sb.Append('b');
                    }
                    else
                    {
                        if (king == p)
                            sb.Append('W');
                        else
                            sb.Append('w');
                    }

                    if (j < Board.Size - 1)
                    {
                        sb.Append(" ");
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
        private IReadOnlyList<CheckersNode> GetChildren()
        {
            int opponentStones = Board.CountOccupiedCells(Opponent);
            int minOppStones = opponentStones;

            List<(CheckersTable board, List<PortalCommand> commands)> childrenTables = new();
            foreach ((var position, var validMoves) in GetValidMoves(Self))
            {
                foreach (FieldPosition move in validMoves)
                {
                    (var table, var commands) = ExecuteMove(Board, new(), position, move);
                    opponentStones = Board.CountOccupiedCells(Opponent);
                    if (opponentStones < minOppStones)
                    {
                        minOppStones = opponentStones;
                        childrenTables.Clear(); //Alle bisherigen Züge können gelöscht werden, weil dieser mehr Sprünge zulässt
                    }
                    if (opponentStones == minOppStones)//es muss der Zug gewählt werden, bei dem die meisten Gegnersteine geschmissen werden
                    {
                        table.GetTableWithUpdatedStability();
                        childrenTables.Add(new(table, commands));
                    }
                }
            }

            List<CheckersNode> children = new();

            foreach (var (board, commands) in childrenTables)
                children.Add(new CheckersNode(board, Opponent, commands));

            return children.AsReadOnly();
        }

        /// <summary>
        /// Returns the table after given move.
        /// </summary>
        /// <param name="move">The move.</param>
        /// <returns>The table after given move.</returns>
        private (CheckersTable table, List<PortalCommand> commands) ExecuteMove(CheckersTable table, List<PortalCommand> commands, FieldPosition pos, FieldPosition move)
        {
            FieldPosition newPos = new(pos.X + move.X, pos.Y + move.Y);
            FieldPosition removePos = default;
            CheckersTable tableAfterMove = new(table);
            var portalCmds = commands;
            bool jumped = false;

            tableAfterMove.SetPlayer(Player.None, pos.Y, pos.X); //Ausgangsfeld löschen

            if (tableAfterMove.GetKing(pos.Y, pos.X) != Player.None)
            {
                tableAfterMove.SetKing(Player.None, pos.Y, pos.X);
                (var player, var position) = CheckStoneOnPath(table, pos, move);

                if (player == Opponent)
                {
                    tableAfterMove.SetPlayer(Player.None, position.Y, position.X); //Gegnerstein schmeißen
                    removePos = position;
                    tableAfterMove.SetKing(Player.None, position.Y, position.X);
                    jumped = true;
                }

                tableAfterMove.SetKing(Self, newPos.Y, newPos.X);
            }
            else
            {
                if (tableAfterMove.GetPlayer(newPos.Y, newPos.X) == Opponent)
                {
                    tableAfterMove.SetPlayer(Player.None, newPos.Y, newPos.X); //Gegnerstein schmeißen
                    removePos = newPos;
                    tableAfterMove.SetKing(Player.None, newPos.Y, newPos.X);
                    newPos = new(newPos.X + move.X, newPos.Y + move.Y);
                    jumped = true;
                }
            }
            portalCmds.Add(new PortalCommand(pos, newPos, MoveType.MoveStone, Self));
            tableAfterMove.SetPlayer(Self, newPos.Y, newPos.X); //Neues Feld besetzen
            if (removePos != default)
                portalCmds.Add(new PortalCommand(removePos, removePos, MoveType.RemoveStone, Opponent));

            if (jumped)
                (tableAfterMove, portalCmds) = AnotherJump(tableAfterMove, portalCmds, newPos, new(move.X > 0 ? -1 : 1, move.Y > 0 ? -1 : 1));

            if (AtOpponentBorder(newPos))
                tableAfterMove.SetKing(Self, newPos.Y, newPos.X);

            tableAfterMove.GetTableWithUpdatedStability();

            return (tableAfterMove, portalCmds);
        }

        private (CheckersTable table, List<PortalCommand> commands) AnotherJump(CheckersTable table, List<PortalCommand> commands, FieldPosition stonePos, FieldPosition invDir)
        {
            IList<FieldPosition> moves;
            bool king = table.GetKing(stonePos.Y, stonePos.X) != Player.None;
            if (king)
                moves = GetPossibleMovesKing(table, Self, stonePos, invDir).ToList();
            else
                moves = GetPossibleMoves(Self == Board.HumanPlayer, stonePos, true).ToList();

            if (moves.Count > 0)
            {
                foreach (FieldPosition move in moves)
                {
                    if (CheckJump(table, stonePos, move))
                    {
                        bool validMove = king ? CheckValidMoveKing(table, Self, stonePos, move) : CheckValidMove(table, Self, stonePos, move);
                        if (validMove)
                            return ExecuteMove(table, commands, stonePos, move);
                    }
                }
            }
            return (table, commands);
        }

        private bool CheckJump(CheckersTable table, FieldPosition pos, FieldPosition move)
        {
            if (table.GetKing(pos.Y, pos.X) != Player.None)
            {
                if (table.GetPlayer(pos.Y + move.Y, pos.X + move.X) == Player.None)
                    if (CheckStoneOnPath(table, pos, move).Item1 == Opponent)
                        return true;

            }
            else
            {
                if (move.Y != 0 && move.X != 0)
                {
                    FieldPosition dir = new FieldPosition(move.X / Math.Abs(move.X), move.Y / Math.Abs(move.Y));
                    if (table.GetPlayer(pos.Y + dir.Y, pos.X + dir.X) == Opponent)
                        return true;
                }
            }
            return false;
        }

        private bool AtOpponentBorder(FieldPosition pos)
        {
            return Self == Board.HumanPlayer && pos.Y == 0 ||
                    Self == Board.AiPlayer && pos.Y == Board.Size - 1;
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
                if (Board.GetKing(pos.Y, pos.X) != Player.None && Board.GetPlayer(pos.Y, pos.X) == player)
                    moves = GetPossibleMovesKing(Board, player, pos).ToList();
                else
                    moves = GetPossibleMoves(player == Board.HumanPlayer, pos).ToList();

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

        private IEnumerable<FieldPosition> GetPossibleMovesKing(CheckersTable table, Player player, FieldPosition stonePos, FieldPosition invalDirection = default)
        {
            return CheckersConstants.DirectionsKing.Where(move => CheckValidMoveKing(table, player, stonePos, move, invalDirection));
        }

        /// <summary>
        /// Gets directions in which the specified move flips opponent's cells.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <param name="stonePos">The move represented by cell coordinates.</param>
        /// <returns>Directions in which the specified move flips opponent's cells.</returns>
        private IEnumerable<FieldPosition> GetPossibleMoves(bool isHumanPlayer, FieldPosition stonePos, bool jumping = false)
        {
            //if (!jumping)
            //{
            if (isHumanPlayer)
                return CheckersConstants.DirectionsBottomPlayer.Where(direction => CheckValidMove(Board, Board.HumanPlayer, stonePos, direction));
            else
                return CheckersConstants.DirectionsTopPlayer.Where(direction => CheckValidMove(Board, Board.AiPlayer, stonePos, direction));
            //}
            //else
            //{
            //    return CheckersConstants.StabilityDirections.Where(direction => CheckValidMove(Board, isHumanPlayer ? Board.HumanPlayer : Board.AiPlayer, stonePos, direction));
            //}
        }

        private bool CheckValidMoveKing(CheckersTable table, Player player, FieldPosition startPos, FieldPosition move, FieldPosition invDir = default)
        {
            // TODO: Überprüfen, ob übergebener player gleich dem HumanPlayer aus table ist. Dann kann weiter unten opponent ersetzt werden und parameterübergabe entfernt werden.
            if (invDir != default)
            {
                for (int i = 0; i < Board.Size; i++)
                {
                    FieldPosition compareDir = new(invDir.X * i, invDir.Y * i);
                    if (move.Equals(compareDir))
                        return false;
                }
            }

            int newRow = startPos.Y + move.Y;
            int newCol = startPos.X + move.X;

            if (newRow < 0 || newRow >= Board.Size || newCol < 0 || newCol >= Board.Size) //außerhalb Spielfeld
            {
                return false;
            }

            if (Board.GetPlayer(newRow, newCol) != Player.None) //Feld bereits belegt
            {
                return false;
            }

            (var disturbingPlayer, var position) = CheckStoneOnPath(table, startPos, move);

            if (disturbingPlayer == player)
            {
                return false; //Spieler im Weg
            }

            if (disturbingPlayer == Opponent)
            {
                return true; //Kann Gegner schmeißen
            }

            return true; //Weg ist frei
        }

        private (Player player, FieldPosition position) CheckStoneOnPath(CheckersTable table, FieldPosition startPos, FieldPosition move)
        {
            FieldPosition comparePos = new FieldPosition(startPos.X + move.X, startPos.Y + move.Y);
            int y, x;
            y = move.Y > 1 ? 1 : move.Y < -1 ? -1 : move.Y;
            x = move.X > 1 ? 1 : move.X < -1 ? -1 : move.X;
            FieldPosition direc = new FieldPosition(x, y);
            FieldPosition checkPos = startPos;
            Player checkStoneVal;
            var foundPlayer = Player.None;
            var foundPos = new FieldPosition(0, 0);

            do
            {
                checkPos = new FieldPosition(checkPos.X + direc.X, checkPos.Y + direc.Y);
                checkStoneVal = table.GetPlayer(checkPos.Y, checkPos.X);
                if (checkStoneVal != Player.None)
                {
                    if (foundPlayer == Player.None)
                    {
                        foundPlayer = checkStoneVal;
                        foundPos = checkPos;
                    }
                    else
                        return (Self, checkPos);
                }
            } while (!comparePos.Equals(checkPos));

            return (foundPlayer, foundPos);
        }

        /// <summary>
        /// Determines whether the specified move flips opponent's cells in given direction.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <param name="startPos">The move represented by cell coordinates.</param>
        /// <param name="direction">The direction represented by coordinate deltas.</param>
        /// <returns>True if the move flips opponent's cells in given direction, false otherwise.</returns>
        private bool CheckValidMove(CheckersTable table, Player player, FieldPosition startPos, FieldPosition direction)
        {
            Debug.Assert(player != Player.None, "Verifying that the player value is valid.");

            int newCol = startPos.X + direction.X;
            int newRow = startPos.Y + direction.Y;

            bool opponentCellsCaptured = false;

            while (newRow >= 0 && newRow < Board.Size && newCol >= 0 && newCol < Board.Size)
            {
                Player value = table.GetPlayer(newRow, newCol);

                if (!opponentCellsCaptured)
                {
                    if (value == Opponent)
                    {
                        opponentCellsCaptured = true;
                        newRow += direction.Y;
                        newCol += direction.X;
                    }
                    else if (value == player)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (value == Player.None)
                        return true;
                    else
                        return false;
                }
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
                return CheckersConstants.VictoryPoints;
            else if (winner == Board.AiPlayer)
                return -CheckersConstants.VictoryPoints;
            else
                return GetNumberStoneScore() + GetStabilityScore() + GetNumberMovesScore();
        }

        private int GetNumberStoneScore()
        {
            int maxPlayerCells = Board.CountOccupiedCells(Board.HumanPlayer);
            int minPlayerCells = Board.CountOccupiedCells(Board.AiPlayer);
            // TODO: Kings werden noch nicht richtig berechnet
            int maxPlayerKings = Board.MaxPlayerKings();
            int minPlayerKings = Board.MinPlayerKings();

            // wenn HumanPlayer white ist, die berechnung für maximizing und minimizing player tauschen
            if (Board.HumanPlayer == Player.White)
            {
                var tmpKings = maxPlayerKings;
                maxPlayerKings = minPlayerKings;
                minPlayerKings = tmpKings;
            }

            return ((maxPlayerCells - maxPlayerKings) - (minPlayerCells - minPlayerKings)) * CheckersConstants.StonePoints +
                (maxPlayerKings - minPlayerKings) * CheckersConstants.KingPoints;
        }

        private int GetStabilityScore()
        {
            return Board.GetStabilityScore() * CheckersConstants.StabilityPoints;
        }

        private int GetNumberMovesScore()
        {
            int score;
            score = Children.Count >= CheckersConstants.MoveAdvantageNumber ? CheckersConstants.MoveAdvantageNumber : Children.Count;
            score *= CheckersConstants.PossibleMovesPoints;
            score = Self == Board.HumanPlayer ? score : -score;
            return score;
        }

        public Player GetWinner()
        {
            return Children.Count == 0 ? Opponent : Player.None;
        }

        public INode SetField(Player[][] newField)
        {
            var field = (IReadOnlyCollection<IReadOnlyCollection<Player>>)newField;
            foreach (var child in Children)
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
                    return child;
                }
            }

            return null;
        }

        public INode NodeForMove(FieldPosition posStart, FieldPosition posEnd, FieldPosition oppStoneKick)
        {
            FieldPosition diffPos = new(posEnd.X - posStart.X, posEnd.Y - posStart.Y);
            if (diffPos.Y == 0 || diffPos.X == 0 || Board[posStart.X, posStart.Y] != Self)
                return null;

            FieldPosition direction = new(diffPos.X > 0 ? 1 : -1, diffPos.Y > 0 ? 1 : -1);
            bool validDirection = false;
            if (Board.GetKing(posStart.Y, posStart.X) == Player.None)
            {
                foreach (FieldPosition dir in Self == Board.HumanPlayer ? CheckersConstants.DirectionsBottomPlayer : CheckersConstants.DirectionsTopPlayer)
                    if (dir.Equals(direction))
                        validDirection = true;

                if (CheckValidMove(Board, Self, posStart, direction) && validDirection)
                {
                    (var table, var commands) = ExecuteMove(Board, new(), posStart, direction);
                    foreach (CheckersNode child in Children)
                        if (table.Equals(child.Board))
                            return new CheckersNode(table, Opponent, commands);
                }

                return null; //invalid move
            }
            else
            {
                foreach (FieldPosition dir in CheckersConstants.DirectionsKing)
                    if (dir.Equals(diffPos))
                        validDirection = true;

                if (CheckValidMoveKing(Board, Self, posStart, diffPos) && validDirection)
                {
                    (var table, var commands) = ExecuteMove(Board, new(), posStart, diffPos);
                    foreach (CheckersNode child in Children)
                        if (table.Equals(child.Board))
                            return new CheckersNode(table, Opponent, commands);
                }

                return null; //invalid move
            }


        }


        /// <summary>
        /// Creates an Node with GameOver = true
        /// </summary>
        /// <returns></returns>
        public INode NodeForEnd()
        {
            var endNode = new CheckersNode((CheckersTable)Board);

            return endNode;
        }

        public string KingsString { get
            {
                return GetKingsString();
            } }

        public string GetKingsString()
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
                    Player king = Board.GetKing(i, j);
                    if (king == Player.None)
                    {
                        sb.Append('-');
                    }
                    else if (king == Player.Black)
                    {
                        sb.Append('B');
                    }
                    else
                    {
                        sb.Append('W');
                    }

                    if (j < Board.Size - 1)
                    {
                        sb.Append(" ");
                    }
                }

                sb.Append('|');
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
