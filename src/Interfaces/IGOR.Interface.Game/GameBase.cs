using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace IGOR.Interface.Game
{
    public abstract class Game<TNode> : BindableBase, IGame
        where TNode : INode
    {
        protected uint SearchDepth;

        public abstract event EventHandler BoardReady;
        public event EventHandler<Player> GameOver;

        public abstract IGameAi GameAi { get; }
        public abstract string Name { get; }
        public abstract string ImageRessourcePath { get; }
        public abstract Dictionary<string, (string board, string hmi)> Views { get; }

        public virtual string GameRulePath => Directory.GetCurrentDirectory() + @$"\Gamerules\{Name}.txt";
        public (string board, string hmi) ActualView { get; set; }

        private TNode _currentState;
        public TNode CurrentState
        {
            get => _currentState; 
            private set => SetProperty(ref _currentState, value, nameof(CurrentState));
        }


        private TNode _nextState;
        public TNode NextState
        {
            get => _nextState;
            protected set => SetProperty(ref _nextState, value, nameof(NextState));
        } 

        private TNode _enqueuedState;
        public TNode EnqueuedState
        {
            get => _enqueuedState;
            protected set => SetProperty(ref _enqueuedState, value, nameof(EnqueuedState));
        }

        public IBoard CurrentBoard => CurrentState?.Board;
        public bool? IsGameOver => CurrentState?.IsGameOver;
        public Difficulty Difficulty { get; set; }

        protected Player _humanPlayer;
        public Player HumanPlayer { get => _humanPlayer; set => SetProperty(ref _humanPlayer, value, nameof(HumanPlayer)); }

        INode IGame.CurrentState => CurrentState;
        INode IGame.NextState => NextState;

        /// <summary>
        /// Aktualisiert das spielfeld für einen AI-Zug
        /// </summary>
        public virtual void MakeAiMove()
        {
            var newState = GameAi.Best(CurrentState, SearchDepth);
            NextState = newState;
        }

        /// <summary>
        /// Aktualisiert das gesamte Spielfeld
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public virtual bool SetField(Player[][] field)
        {
            var newState = (TNode)CurrentState.SetField(field);
            if (newState == null)
                return false;

            NextState = newState;
            return true;
        }

        /// <summary>
        /// Aktualisiert das Spielfeld, wenn ein Stein gesetzt wurde
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="oppKick"></param>
        /// <returns></returns>
        public virtual bool SetField(FieldPosition start, FieldPosition end, FieldPosition oppKick)
        {
            var newState = (TNode)CurrentState.NodeForMove(start, end, oppKick);
            if (newState == null)
                return false;

            NextState = newState;
            return true;
        }


        public virtual void End()
        {
            var state = CurrentState;
            if (state == null)
                state = NextState; 
            var endState = (TNode)state.NodeForEnd();

            CurrentState = endState;
            NextState = CurrentState;
            
        }

        public abstract void Start();
         
        public void ConfirmNextState()
        {
            CurrentState = NextState;
            NextState = default;
        } 


        public void RaiseGameOver(Player player)
        {
            GameOver?.Invoke(this, player);
        }

    }
}
