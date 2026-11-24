using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Games.CamTest
{
    public class CamTestGame : IGame
    {
        public Difficulty Difficulty { get; set; }
        public Player HumanPlayer { get; set; }
        public IGameAi GameAi { get; }
        public string Name => "Kamera Test";
        public string ImageRessourcePath { get; }
        public Dictionary<string, (string board, string hmi)> Views { get; } = new Dictionary<string, (string board, string hmi)>
        {
            {"Standard", ("DefaultBoardView","CameraTestView") }
        };
        public string GameRulePath => "";
        public INode CurrentState { get; }
        public IBoard CurrentBoard { get; }
        public bool GameOver { get; }
        public (string board, string hmi) ActualView { get; set; }

        public event EventHandler BoardReady;



        public void MakeAiMove()
        {
            throw new NotImplementedException();
        }

        public bool SetField(Player[][] field)
        {
            throw new NotImplementedException();
        }

        public bool SetField(FieldPosition start, FieldPosition end, FieldPosition oppKick)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
