using IGOR.Common;
using IGOR.Games.TicTacToe.Views;
using IGOR.Interface.Camera;
using IGOR.Interface.Communication;
using IGOR.Interface.Communication.Job;
using IGOR.Interface.Game;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;

namespace IGOR.Games.TicTacToe
{
    public class TicTacToeGame : Game<TicTacToeNode>
    {
        private readonly IPlcCom _plcCom;
        private readonly ICameraService _cameraService;
        private readonly IDialogService _dialogService;
        public override string ImageRessourcePath => "pack://application:,,,/IGOR.Games.TicTacToe;component/Icons/TicTacToe.png";
        public override string Name => "TicTacToe";

        private Dictionary<string, (string board, string hmi)> _views = new Dictionary<string, (string board, string hmi)>
        {
            { "Standard", (nameof(TicTacToeBoardView),nameof(TicTacToeCamView)) },
            //{ "Ohne Kamera", (nameof(TicTacToeBoardView), nameof(TicTacToeView)) },
            { "Ohne Kamera", (nameof(TicTacToeBoardView), nameof(TicTacToeTouchView) )}
        };

        public override event EventHandler BoardReady;

        public override Dictionary<string, (string board, string hmi)> Views => _views;

        public override IGameAi GameAi { get; }

        public TicTacToeGame(IPlcCom plcCom, ICameraService cameraService, IDialogService dialogService)
        {
            TicTacToeBoardView.BoardReady += (s, e) => BoardReady?.Invoke(s, e);
            GameAi = new AlphaBeta();
            _plcCom = plcCom;
            _cameraService = cameraService;
            _dialogService = dialogService;
        }

        public override void Start()
        {
            NextState = new TicTacToeNode(HumanPlayer);


            SearchDepth = Difficulty switch
            {
                Difficulty.Unbeatable => 9,
                Difficulty.Hard => 6,
                Difficulty.Normal => 3,
                Difficulty.Easy => 2,
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
