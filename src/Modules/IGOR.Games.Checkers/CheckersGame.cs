using IGOR.Games.Checkers.Views;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace IGOR.Games.Checkers
{
    public class CheckersGame : Game<CheckersNode>
    {
        public override string Name => "Dame";
        public override string ImageRessourcePath => "pack://application:,,,/IGOR.Games.Checkers;component/Icons/Checkers.png";

        public override IGameAi GameAi { get; }

        private Dictionary<string, (string board, string hmi)> _views = new Dictionary<string, (string board, string hmi)>
        {
            {"Standard", (nameof(CheckersBoardView),nameof(CheckersCamView)) },
            { "Ohne Kamera", (nameof(CheckersBoardView), nameof(CheckersView)) }
        };

        public override event EventHandler BoardReady;

        public override Dictionary<string, (string board, string hmi)> Views => _views;

        public CheckersGame()
        {
            CheckersBoardView.BoardReady += (s, e) => BoardReady?.Invoke(s, e);
            GameAi = new AlphaBeta();
        }

        public override void Start()
        {
            NextState = new CheckersNode(HumanPlayer);
            SearchDepth = Difficulty switch
            {
                Difficulty.Unbeatable => 6,
                Difficulty.Hard => 5,
                Difficulty.Normal => 3,
                Difficulty.Easy => 2,
                _ => throw new NotImplementedException(),
            };
        }

        public override void End()
        {
            base.End();
        }
    }
}