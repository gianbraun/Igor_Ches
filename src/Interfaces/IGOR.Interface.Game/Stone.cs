using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Interface.Game
{
    public record Stone(double X, double Y, Player Player, FieldPosition FieldPosition)
    {
        public bool IsInvalidPos => FieldPosition is null;

        public override string ToString()
        {
            return FieldPosition != null ? $"FieldPos: [{FieldPosition.X};{FieldPosition.Y}] = [{X:N2};{Y:N2}] Ist Schwarz: {Player}" :
                                           $"[{X:N2};{Y:N2}] Ist Schwarz: {Player}";
        }
    }
}
