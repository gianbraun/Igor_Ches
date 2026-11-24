using System;
using System.Collections.Generic;
using System.Linq;

namespace IGOR.Interface.Game
{
    public abstract class BoardBase : IBoard
    {
        public Player this[int x, int y]
        {
            get => _field[x][y];
            set => _field[x][y] = value;
        }

        public abstract int Size { get; }

        protected Player[][] _field;
        public abstract Dictionary<FieldPosition, (double x, double y)> ValidPositions { get; }

        public IReadOnlyCollection<IReadOnlyCollection<Player>> Field => _field;
         
        public Player HumanPlayer {get; protected set;}

        public Player AiPlayer { get; protected set; }

        public BoardBase(BoardBase oldBoard)
        {
            _field = new Player[Size][];
            for (int i = 0; i < Size; i++)
            {
                _field[i] = new Player[Size];
            }

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    _field[x][y] = oldBoard.Field.ElementAt(x).ElementAt(y);
                }
            }

            HumanPlayer = oldBoard.HumanPlayer;
            AiPlayer = oldBoard.HumanPlayer == Player.White ? Player.Black : Player.White;
        }


        public BoardBase(Player humanPlayer)
        {
            _field = new Player[Size][];
            for (int i = 0; i < Size; i++)
            {
                _field[i] = new Player[Size];
            }
            HumanPlayer = humanPlayer;
            AiPlayer = humanPlayer == Player.White ? Player.Black : Player.White;
            InitializeBoard();
        }

        protected abstract void InitializeBoard();

        public virtual void SetField(Player[][] newField, Player actualPlayer)
        {
            _field = newField;
        }

        public bool Equals(IReadOnlyCollection<IReadOnlyCollection<Player>> other)
        {
            if (_field.Length != other.Count)
                return false;
            for (int i = 0; i < _field.Length; i++)
            {
                if (!_field[i].SequenceEqual(other.ElementAt(i)))
                    return false;
            }

            return true;
        }

        void IBoard.InitializeBoard()
        {
            throw new NotImplementedException();
        }
    }
}
