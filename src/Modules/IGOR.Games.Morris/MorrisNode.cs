using IGOR.Interface.Communication.Job;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace IGOR.Games.Morris
{
    /// <summary>
    /// An immutable representation of Morris game position.
    /// </summary>
    public class MorrisNode : INode
    {

        /// <summary>
        /// The child nodes.
        /// </summary>
        private readonly Lazy<IReadOnlyList<MorrisNode>> children;

        /// <summary>
        /// The game state heuristics.
        /// </summary>
        private readonly Lazy<int> heuristics;
         
        internal MorrisTable Board { get; set; }
        IBoard INode.Board => Board;
        public bool IsGameOver => !Children.Any();

        public GamePhase GamePhase { get; }

        public IReadOnlyList<PortalCommand> PortalCommands { get; }

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

        /// <summary>
        /// Gets heuristics of current position.
        /// </summary>
        public int Heuristics => heuristics.Value;


        // Spiel startet immer mit weiß
        /// <summary>
        /// Initializes a new instance of the <see cref="MorrisNode" /> class with the starting position.
        /// </summary>
        public MorrisNode(Player humanPlayer, Player startPlayer = Player.White) : this(
            new MorrisTable(humanPlayer),
            startPlayer,
            new List<PortalCommand>())
        {
        }

        private MorrisNode(MorrisTable board)
        { 
            Board = board;
            PortalCommands = new List<PortalCommand>();
            children = new Lazy<IReadOnlyList<MorrisNode>>(
                () => new List<MorrisNode>().AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MorrisNode" /> class.
        /// </summary>
        /// <param name="board">The game state.</param>
        /// <param name="currentPlayer">Current player.</param>
        /// <param name="playerCanPass">Indicates whether the current player can pass a move.</param>
        private MorrisNode(
            MorrisTable board,
            Player currentPlayer,
            IEnumerable<PortalCommand> commands)
        {
            Board = board;

            Self = currentPlayer;
            Opponent = currentPlayer == Player.Black
                ? Player.White
                : Player.Black;

            GamePhase = GetGamePhase();

            PortalCommands = commands.ToList().AsReadOnly();

            heuristics = new Lazy<int>(
                () => GetHeuristics(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            children = new Lazy<IReadOnlyList<MorrisNode>>(
                () => GetChildren(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }


        /// <summary>
        /// Compares to another Morris node.
        /// </summary>
        /// <param name="other">The other node to compare with.</param>
        /// <returns>Comparison result.</returns>
        public int CompareTo(INode other)
        {
            return Heuristics.CompareTo(other.Heuristics);
        }


        /// <summary>
        /// Computes all children nodes of current node.
        /// </summary>
        /// <returns>Children nodes.</returns>
        private IReadOnlyList<MorrisNode> GetChildren()
        {
            List<MorrisNode> children = new();

            if (GetWinner() != Player.None)
                return children;

            var oppStones = GetStonePositions(Opponent).Where(stone => !Board.GetStable(stone.X, stone.Y));
            if (GamePhase == GamePhase.Placing)
            {
                foreach (Tuple<FieldPosition, IEnumerable<FieldPosition>> validMove in GetValidMoves(Self))
                {
                    if (NewMill(validMove.Item1))
                    {
                        foreach (FieldPosition oppStone in oppStones)
                        {
                            var table = GetTableForMove(validMove.Item1, validMove.Item1, oppStone);
                            if (table != null)
                                children.Add(new MorrisNode(table.Item1, Opponent, table.Item2.ToList()));
                        }
                    }
                    else
                    {
                        var table = GetTableForMove(validMove.Item1, validMove.Item1, new FieldPosition(0, 0));
                        if (table != null)
                            children.Add(new MorrisNode(table.Item1, Opponent, table.Item2.ToList()));
                    }
                }
            }
            else
            {
                foreach (Tuple<FieldPosition, IEnumerable<FieldPosition>> validMove in GetValidMoves(Self))
                {
                    foreach (FieldPosition move in validMove.Item2)
                    {
                        if (NewMill(move))
                        {
                            foreach (FieldPosition oppStone in oppStones)
                            {
                                var table = GetTableForMove(validMove.Item1, move, oppStone);
                                if (table != null)
                                    children.Add(new MorrisNode(table.Item1, Opponent, table.Item2.ToList()));
                            }
                        }
                        else
                        {
                            var table = GetTableForMove(validMove.Item1, move, new FieldPosition(0, 0));
                            if (table != null)
                                children.Add(new MorrisNode(table.Item1, Opponent, table.Item2.ToList()));
                        }
                    }
                }
            }

            return children.AsReadOnly();
        }

        private Tuple<MorrisTable, IEnumerable<PortalCommand>> GetTableForMove(FieldPosition pos, FieldPosition move, FieldPosition oppStone)
        {
            var boardAfterMove = new MorrisTable(Board);
            List<PortalCommand> portalCmds = new();

            if (GamePhase != GamePhase.Placing)
            {
                boardAfterMove[pos.X, pos.Y] = Player.None; //Ausgangsfeld löschen
                portalCmds.Add(new(pos, move, MoveType.MoveStone, Self));
            }
            else
            {
                portalCmds.Add(new(pos, move, MoveType.NewStone, Self));
                boardAfterMove.DecreaseStonesAvailable(Self);
            }

            boardAfterMove[move.X, move.Y] = Self; //Neues Feld besetzen
            boardAfterMove.UpdateMills();

            if (boardAfterMove.GetStable(move.X, move.Y))//NewMill(move))
            {
                if (boardAfterMove[oppStone.X, oppStone.Y] == Opponent)
                {
                    boardAfterMove[oppStone.X, oppStone.Y] = Player.None;
                    portalCmds.Add(new(oppStone, oppStone, MoveType.RemoveStone, Opponent));
                }
                else
                    return null;
            }

            return new Tuple<MorrisTable, IEnumerable<PortalCommand>>(boardAfterMove, portalCmds);
        }

        private bool NewMill(FieldPosition newCell)
        {
            var newTable = new MorrisTable(Board);
            newTable[newCell.X, newCell.Y] = Self;
            newTable.UpdateMills();
            return newTable.GetStable(newCell.X, newCell.Y);
        }

        /// <summary>
        /// Candidates the cells.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <returns>List of valid moves.</returns>
        private IEnumerable<Tuple<FieldPosition, IEnumerable<FieldPosition>>> GetValidMoves(Player player)
        {
            if (GamePhase == GamePhase.Moving)
            {
                foreach (FieldPosition pos in GetStonePositions(player))
                {
                    var moves = GetPossibleMoves(pos).ToList();
                    if (moves.Count > 0)
                        yield return new Tuple<FieldPosition, IEnumerable<FieldPosition>>(pos, moves);
                }
            }
            else if (GamePhase == GamePhase.Placing)
            {
                foreach (FieldPosition pos in GetFreeCells())
                    yield return new Tuple<FieldPosition, IEnumerable<FieldPosition>>(pos, new List<FieldPosition>());
            }
            else
            {
                foreach (FieldPosition pos in GetStonePositions(player))
                    yield return new Tuple<FieldPosition, IEnumerable<FieldPosition>>(pos, GetFreeCells());
            }

        }
        private IEnumerable<FieldPosition> GetFreeCells()
        {
            for (int y = 0; y < Board.Size; ++y)
            {
                for (int x = 0; x < Board.Size; ++x)
                {
                    bool inval = false;
                    foreach (FieldPosition invPos in MorrisConstants.InvalidPositions)
                    {
                        if (invPos.Y == y && invPos.X == x)
                        {
                            inval = true;
                            break;
                        }
                    }

                    if (Board[x, y] == Player.None && !inval)
                    {
                        yield return new FieldPosition(x, y);
                    }
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
                    if (Board[x, y] == Player)
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
            int factorRow, factorCol;
            if (stonePos.Y != 3)
            {
                if (stonePos.X != 3)
                {
                    factorRow = Math.Abs(stonePos.Y - 3);
                    factorCol = Math.Abs(stonePos.X - 3);
                }
                else
                {
                    factorRow = 1;
                    factorCol = Math.Abs(stonePos.Y - 3);
                }
            }
            else
            {
                factorRow = Math.Abs(stonePos.X - 3);
                factorCol = 1;
            }
            foreach (FieldPosition dir in MorrisConstants.DirectionsMove)
                if (CheckValidMove(stonePos, new FieldPosition(dir.X * factorCol, dir.Y * factorRow)))
                    yield return new FieldPosition(stonePos.X + dir.X * factorCol, stonePos.Y + dir.Y * factorRow);
        }

        private GamePhase GetGamePhase()
        {
            if (Board.GetStonesAvailable(Self) > 0)
                return GamePhase.Placing;
            else
            {
                int playerStones = Board.PlayerCells(Self);
                if (playerStones < 4)
                    return GamePhase.Jumping;
                else
                    return GamePhase.Moving;
            }
        }


        /// <summary>
        /// Determines whether the specified move flips opponent's cells in given direction.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <param name="startPos">The move represented by cell coordinates.</param>
        /// <param name="direction">The direction represented by coordinate deltas.</param>
        /// <returns>True if the move flips opponent's cells in given direction, false otherwise.</returns>
        private bool CheckValidMove(FieldPosition startPos, FieldPosition direction)
        {
            FieldPosition newPos = new(startPos.X + direction.X, startPos.Y + direction.Y);
            if (newPos.Y < 0 || newPos.Y >= Board.Size || newPos.X < 0 || newPos.X >= Board.Size)
                return false;

            Player value = Board[newPos.X, newPos.Y];
            if (value != Player.None)
                return false;

            bool inval = false;
            foreach (FieldPosition position in MorrisConstants.InvalidPositions)
                if (position.Y == newPos.Y && position.X == newPos.X)
                    inval = true;

            if (inval)
                return false;

            return true;
        }

        /// <summary>
        /// Computes the heuristics of current position.
        /// </summary>
        /// <returns>Heuristic value of current position.</returns>
        private int GetHeuristics()
        {
            Player winner = GetWinner();
            if (winner == Board.HumanPlayer)
                return MorrisConstants.VictoryPoints;
            else if (winner == Board.AiPlayer)
                return -MorrisConstants.VictoryPoints;
            else
                return GetNumberStoneScore() + GetMobilityScore() + GetStabilityScore();
        }
        private int GetMobilityScore()
        {
            int scoreMax = 0;
            int scoreMin = 0;
            if (GamePhase == GamePhase.Placing)
            {
                foreach (FieldPosition playerStone in GetStonePositions(Board.HumanPlayer))
                {
                    if (playerStone.Y == 3 && (playerStone.X == 1 || playerStone.X == 5) || playerStone.X == 3 && (playerStone.Y == 1 || playerStone.Y == 5))
                        scoreMax += 2;
                    else if (playerStone.Y == 3 || playerStone.X == 3)
                        scoreMax += 1;
                }

                foreach (FieldPosition playerStone in GetStonePositions(Board.AiPlayer))
                {
                    if (playerStone.Y == 3 && (playerStone.X == 1 || playerStone.X == 5) || playerStone.X == 3 && (playerStone.Y == 1 || playerStone.Y == 5))
                        scoreMin += 2;
                    else if (playerStone.Y == 3 || playerStone.X == 3)
                        scoreMin += 1;
                }

                return (scoreMax - scoreMin) * MorrisConstants.MobilityPoints;
            }
            else
                return 0;

        }

        private int GetNumberStoneScore()
        {
            int maxPlayerCells = Board.PlayerCells(Board.HumanPlayer);
            int minPlayerCells = Board.PlayerCells(Board.AiPlayer);
            return (maxPlayerCells - minPlayerCells) * MorrisConstants.StonePoints;
        }

        private int GetStabilityScore()
        {
            // TODO: muss hier Board.HumanPlayer statt Self hin?
            return Board.StableCellScore(Board.HumanPlayer) * MorrisConstants.StabilityPoints;
        }

        public INode NodeForMove(FieldPosition posStart, FieldPosition posEnd, FieldPosition oppStoneKick)
        {
            int diffRow;
            int diffCol;
            FieldPosition direction;

            if (GamePhase != GamePhase.Placing)
            {
                diffRow = posEnd.Y - posStart.Y;
                diffCol = posEnd.X - posStart.X;

                if (diffRow == 0 && diffCol == 0)
                    return null;

                direction = new(diffCol == 0 ? 0 : diffCol / Math.Abs(diffCol), diffRow == 0 ? 0 : diffRow / Math.Abs(diffRow));
            }
            else
            {
                var newTable = GetTableForMove(posStart, posEnd, oppStoneKick);
                if (newTable != null)
                    return new MorrisNode(newTable.Item1, Opponent, newTable.Item2.ToList());
                else
                    return null;
            }

            bool validDirection = false;
            if (GamePhase == GamePhase.Moving)
            {
                if (Board[posStart.X, posStart.Y] != Self)
                    return null;

                foreach (FieldPosition dir in MorrisConstants.DirectionsMove)
                    validDirection |= dir.Equals(direction);
            }
            else
            {
                validDirection = true; //when jumping and placing, any cell is allowed to set the stone to
            }

            if (CheckValidMove(posStart, new(diffCol, diffRow)) && validDirection)
            {
                var newTable = GetTableForMove(posStart, posEnd, oppStoneKick);
                return new MorrisNode(newTable.Item1, Opponent, newTable.Item2.ToList());
            }
            else
                return null; //invalid move            
        }


        public Player GetWinner()
        {
            if(GamePhase == GamePhase.Placing)
                return Player.None;

            if (Board.PlayerCells(Player.Black) < 3)
                return Player.White;

            if (Board.PlayerCells(Player.White) < 3)
                return Player.Black;

            return Player.None;
        }

        public INode SetField(Player[][] newField)
        {
            var field = (IReadOnlyCollection<IReadOnlyCollection<Player>>)newField;
            foreach (MorrisNode child in Children)
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
                    MorrisTable newTable = new(child.Board);
                    newTable.SetField(newField, Self);
                    return new MorrisNode(newTable, Opponent, PortalCommands);
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
            var endNode = new MorrisNode(Board);

            return endNode;
        }
    }
}
