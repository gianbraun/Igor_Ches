using System.ComponentModel;

namespace IGOR.Interface.Game
{
    public enum Difficulty
    {
        [Description("Einfach")]
        Easy,
        [Description("Normal")]
        Normal,
        [Description("Schwer")]
        Hard,
        [Description("Unschlagbar")]
        Unbeatable
    }
}