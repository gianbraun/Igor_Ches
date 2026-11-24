using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IGOR.Interface.Game
{
    public class FieldPosition
    {
        public int X { get; }
        public int Y { get; }

        public FieldPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is FieldPosition pos && X == pos.X && pos.Y == pos.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}