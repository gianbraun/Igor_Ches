using IGOR.Games.Kono.Views;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;

namespace IGOR.Games.Kono
{
    public class KonoGame : Game<KonoNode>
    {
        public override string Name => "Kono";

        public override string ImageRessourcePath => "pack://application:,,,/IGOR.Games.Kono;component/Icons/Kono.png";
         
        public override IGameAi GameAi { get; }
         

        public override Dictionary<string, (string board, string hmi)> Views => _views; private Dictionary<string, (string board, string hmi)> _views = new Dictionary<string, (string board, string hmi)>
        {
            {"Standard", (nameof(KonoBoardView),nameof(KonoCamView)) },
            { "Ohne Kamera", (nameof(KonoBoardView), nameof(KonoView)) }
        };

        public KonoGame()
        {
            KonoBoardView.BoardReady += (s, e) => BoardReady?.Invoke(s, e);
            GameAi = new AlphaBeta();
        }

        public override event EventHandler BoardReady;

        public override void Start()
        {
            NextState = new KonoNode(HumanPlayer, Difficulty);
            SearchDepth = Difficulty switch
            {
                Difficulty.Unbeatable => 7,
                Difficulty.Hard => 5,
                Difficulty.Normal => 3,
                Difficulty.Easy => 1,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
