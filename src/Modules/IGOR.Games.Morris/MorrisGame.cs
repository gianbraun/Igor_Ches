using IGOR.Games.Morris.Views;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;

namespace IGOR.Games.Morris
{
    public class MorrisGame : Game<MorrisNode>
    {
        public override string Name => "Mühle";
        public override string ImageRessourcePath => "pack://application:,,,/IGOR.Games.Morris;component/Icons/Morris.png";

        public override IGameAi GameAi { get; }

        private Dictionary<string, (string board, string hmi)> _views = new Dictionary<string, (string board, string hmi)>
        {
            { "Standard", (nameof(MorrisBoardView), nameof(MorrisCamView)) },
            { "Ohne Kamera", (nameof(MorrisBoardView), nameof(MorrisView)) }
        };

        public override event EventHandler BoardReady;

        public override Dictionary<string, (string board, string hmi)> Views => _views;

        public MorrisGame()
        {
            MorrisBoardView.BoardReady += (s, e) => BoardReady?.Invoke(s, e);
            GameAi = new AlphaBeta();
        }

        public override void Start()
        {
            NextState = new MorrisNode(HumanPlayer);
            SearchDepth = Difficulty switch
            {
                Difficulty.Unbeatable => 5,
                Difficulty.Hard => 4,
                Difficulty.Normal =>3,
                Difficulty.Easy => 2,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
