using IGOR.Interface.Communication.Job;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IGOR.Interface.Game
{
    public readonly struct PortalCommand
    {
        public readonly FieldPosition OldPos;
        public readonly FieldPosition NewPos;
        public readonly MoveType MoveType;
        public readonly Player Player;

        public PortalCommand(FieldPosition oldPos, FieldPosition newPos, MoveType moveType, Player player)
        {
            OldPos = oldPos;
            NewPos = newPos;
            MoveType = moveType;
            Player = player;
        }
    }
}